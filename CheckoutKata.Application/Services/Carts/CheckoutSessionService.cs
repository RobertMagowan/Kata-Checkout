using System.Collections.Concurrent;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Application.Carts;

public sealed class CheckoutSessionService : ICheckoutSessionService, ICheckoutSessionMaintenance
{
    private readonly IDbContextFactory<CheckoutKataDbContext> _dbContextFactory;
    private readonly TimeProvider _timeProvider;
    private readonly CartSessionOptions _options;
    private readonly ConcurrentDictionary<Guid, CartSession> _sessionsByCartId = new();
    private readonly object _sessionsSyncRoot = new();
    private readonly object _pricingSeedSyncRoot = new();

    public CheckoutSessionService(
        IDbContextFactory<CheckoutKataDbContext> dbContextFactory,
        TimeProvider? timeProvider = null,
        CartSessionOptions? options = null)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _timeProvider = timeProvider ?? TimeProvider.System;
        _options = options ?? new CartSessionOptions();

        if (_options.SlidingTtl <= TimeSpan.Zero)
        {
            throw new ArgumentException("Sliding TTL must be greater than zero.", nameof(options));
        }

        if (_options.MaxCarts <= 0)
        {
            throw new ArgumentException("Max carts must be greater than zero.", nameof(options));
        }

        if (_options.SweepInterval <= TimeSpan.Zero)
        {
            throw new ArgumentException("Sweep interval must be greater than zero.", nameof(options));
        }

        if (_options.AbsoluteMaxAge is { } absoluteMaxAge && absoluteMaxAge <= TimeSpan.Zero)
        {
            throw new ArgumentException("Absolute max age must be greater than zero when provided.", nameof(options));
        }

        EnsureDefaultPricingVersionSeeded();
    }

    public async Task<CartSnapshot> CreateCartAsync(CancellationToken cancellationToken = default)
    {
        var pricingVersion = await GetLatestActivePricingVersionAsync(cancellationToken);
        var now = _timeProvider.GetUtcNow();
        Guid cartId;

        lock (_sessionsSyncRoot)
        {
            EvictExpiredCartsUnsafe(now);

            if (_sessionsByCartId.Count >= _options.MaxCarts)
            {
                throw new CartCapacityExceededException(_options.MaxCarts);
            }

            cartId = Guid.NewGuid();
        }

        var session = new CartSession(
            cartId,
            pricingVersion.Id,
            new Checkout(pricingVersion.Rules),
            now,
            now);

        if (!_sessionsByCartId.TryAdd(cartId, session))
        {
            throw new InvalidOperationException($"Failed to create cart '{cartId}'.");
        }

        return BuildSnapshot(session, now);
    }

    public Task<CartSnapshot> GetCartAsync(
        Guid cartId,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default) =>
        AccessSessionAsync(
            cartId,
            requestedPricingVersionId,
            (session, now) => BuildSnapshot(session, now),
            enforcePricingVersionMatch: true,
            cancellationToken);

    public Task<CartSnapshot> ScanItemAsync(
        Guid cartId,
        string item,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default) =>
        AccessSessionAsync(
            cartId,
            requestedPricingVersionId,
            (session, now) =>
            {
                session.Checkout.Scan(item);
                return BuildSnapshot(session, now);
            },
            enforcePricingVersionMatch: true,
            cancellationToken);

    public Task<CartSnapshot> ScanManyAsync(
        Guid cartId,
        IReadOnlyCollection<string> items,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("At least one item is required.", nameof(items));
        }

        return AccessSessionAsync(
            cartId,
            requestedPricingVersionId,
            (session, now) =>
            {
                ValidateItemsAsAtomicBatch(items, session.Checkout.GetPricingRules());

                foreach (var item in items)
                {
                    session.Checkout.Scan(item);
                }

                return BuildSnapshot(session, now);
            },
            enforcePricingVersionMatch: true,
            cancellationToken);
    }

    public Task<CartSnapshot> ClearAsync(
        Guid cartId,
        CancellationToken cancellationToken = default) =>
        AccessSessionAsync(
            cartId,
            requestedPricingVersionId: null,
            (session, now) =>
            {
                session.Checkout.Clear();
                return BuildSnapshot(session, now);
            },
            enforcePricingVersionMatch: false,
            cancellationToken);

    public int EvictExpiredCarts()
    {
        var now = _timeProvider.GetUtcNow();

        lock (_sessionsSyncRoot)
        {
            return EvictExpiredCartsUnsafe(now);
        }
    }

    public int ActiveCartCount => _sessionsByCartId.Count;

    private void EnsureDefaultPricingVersionSeeded()
    {
        lock (_pricingSeedSyncRoot)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (dbContext.PricingVersions.Any(version => version.IsActive))
            {
                return;
            }

            var defaultRules = GetDefaultPricingRules()
                .OrderBy(rule => rule.Item, StringComparer.Ordinal)
                .ToArray();

            _ = new Checkout(defaultRules);

            var defaultPricingVersion = new PricingVersion(
                PricingVersionId.New(),
                _timeProvider.GetUtcNow(),
                IsActive: true,
                defaultRules);

            dbContext.PricingVersions.Add(PricingVersionEntityMapper.ToEntity(defaultPricingVersion));
            dbContext.SaveChanges();
        }
    }

    private async Task<PricingVersion> GetLatestActivePricingVersionAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var activeVersion = await dbContext.PricingVersions
            .AsNoTracking()
            .SingleOrDefaultAsync(version => version.IsActive, cancellationToken);

        if (activeVersion is null)
        {
            throw new InvalidOperationException("No active pricing version is configured.");
        }

        return PricingVersionEntityMapper.ToModel(activeVersion);
    }

    private Task<CartSnapshot> AccessSessionAsync(
        Guid cartId,
        PricingVersionId? requestedPricingVersionId,
        Func<CartSession, DateTimeOffset, CartSnapshot> action,
        bool enforcePricingVersionMatch,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_sessionsByCartId.TryGetValue(cartId, out var session))
        {
            throw new CartNotFoundException(cartId);
        }

        var now = _timeProvider.GetUtcNow();

        lock (session.SyncRoot)
        {
            if (IsExpired(session, now))
            {
                _sessionsByCartId.TryRemove(cartId, out _);
                throw new CartNotFoundException(cartId);
            }

            if (enforcePricingVersionMatch &&
                requestedPricingVersionId.HasValue &&
                requestedPricingVersionId.Value != session.PricingVersionId)
            {
                throw new PricingVersionMismatchException(
                    cartId,
                    session.PricingVersionId,
                    requestedPricingVersionId.Value);
            }

            var snapshot = action(session, now);
            session.LastAccessedAtUtc = now;
            return Task.FromResult(snapshot);
        }
    }

    private static void ValidateItemsAsAtomicBatch(
        IReadOnlyCollection<string> items,
        IReadOnlyList<PricingRule> pricingRules)
    {
        var validationCheckout = new Checkout(pricingRules);

        foreach (var item in items)
        {
            validationCheckout.Scan(item);
        }
    }

    private bool IsExpired(CartSession session, DateTimeOffset now)
    {
        var isSlidingExpired = now - session.LastAccessedAtUtc > _options.SlidingTtl;
        if (isSlidingExpired)
        {
            return true;
        }

        if (!_options.AbsoluteMaxAge.HasValue)
        {
            return false;
        }

        return now - session.CreatedAtUtc > _options.AbsoluteMaxAge.Value;
    }

    private CartSnapshot BuildSnapshot(CartSession session, DateTimeOffset now)
    {
        var checkoutStateReader = (ICheckoutStateReader)session.Checkout;
        var expiresAtUtc = now + _options.SlidingTtl;

        return new CartSnapshot(
            session.CartId,
            session.PricingVersionId,
            expiresAtUtc,
            checkoutStateReader.GetScannedItems(),
            checkoutStateReader.GetPricingRules(),
            session.Checkout.GetTotalPrice());
    }

    private int EvictExpiredCartsUnsafe(DateTimeOffset now)
    {
        var removedCount = 0;

        foreach (var (cartId, session) in _sessionsByCartId)
        {
            lock (session.SyncRoot)
            {
                if (!IsExpired(session, now))
                {
                    continue;
                }

                if (_sessionsByCartId.TryRemove(cartId, out _))
                {
                    removedCount++;
                }
            }
        }

        return removedCount;
    }

    private static IReadOnlyCollection<PricingRule> GetDefaultPricingRules() =>
        new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

    private sealed class CartSession(
        Guid cartId,
        PricingVersionId pricingVersionId,
        Checkout checkout,
        DateTimeOffset createdAtUtc,
        DateTimeOffset lastAccessedAtUtc)
    {
        public Guid CartId { get; } = cartId;
        public PricingVersionId PricingVersionId { get; } = pricingVersionId;
        public Checkout Checkout { get; } = checkout;
        public DateTimeOffset CreatedAtUtc { get; } = createdAtUtc;
        public DateTimeOffset LastAccessedAtUtc { get; set; } = lastAccessedAtUtc;
        public object SyncRoot { get; } = new();
    }
}
