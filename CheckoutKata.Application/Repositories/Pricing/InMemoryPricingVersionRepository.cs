using System.Collections.Concurrent;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Core;

namespace CheckoutKata.Application.Pricing;

public sealed class InMemoryPricingVersionRepository : IPricingVersionRepository
{
    private readonly ConcurrentDictionary<PricingVersionId, PricingVersion> _versionsById = new();
    private readonly object _syncLock = new();
    private PricingVersionId _activeVersionId;

    public InMemoryPricingVersionRepository(IReadOnlyCollection<PricingRule>? initialRules = null)
    {
        var defaultRules = initialRules ?? GetDefaultPricingRules();
        var initialVersion = CreateVersionInternal(defaultRules, setAsActive: true, DateTimeOffset.UtcNow);
        _activeVersionId = initialVersion.Id;
    }

    public Task<PricingVersion> GetLatestVersionAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncLock)
        {
            if (!_versionsById.TryGetValue(_activeVersionId, out var activeVersion))
            {
                throw new UnknownPricingVersionException(_activeVersionId);
            }

            return Task.FromResult(activeVersion);
        }
    }

    public Task<PricingVersion?> GetVersionAsync(
        PricingVersionId pricingVersionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _versionsById.TryGetValue(pricingVersionId, out var pricingVersion);
        return Task.FromResult(pricingVersion);
    }

    public Task<PricingVersion> CreateVersionAsync(
        IReadOnlyCollection<PricingRule> pricingRules,
        bool setAsActive,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncLock)
        {
            var createdVersion = CreateVersionInternal(pricingRules, setAsActive, DateTimeOffset.UtcNow);
            return Task.FromResult(createdVersion);
        }
    }

    public Task SetActiveVersionAsync(
        PricingVersionId pricingVersionId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncLock)
        {
            if (!_versionsById.ContainsKey(pricingVersionId))
            {
                throw new UnknownPricingVersionException(pricingVersionId);
            }

            SetActiveVersionUnsafe(pricingVersionId);
        }

        return Task.CompletedTask;
    }

    private PricingVersion CreateVersionInternal(
        IReadOnlyCollection<PricingRule> pricingRules,
        bool setAsActive,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(pricingRules);

        // Validate pricing rules through domain checkout construction.
        _ = new Checkout(pricingRules);

        var normalizedRules = pricingRules
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();

        var createdVersion = new PricingVersion(
            PricingVersionId.New(),
            createdAtUtc,
            IsActive: false,
            normalizedRules);

        _versionsById[createdVersion.Id] = createdVersion;

        if (setAsActive)
        {
            SetActiveVersionUnsafe(createdVersion.Id);
        }

        return _versionsById[createdVersion.Id];
    }

    private void SetActiveVersionUnsafe(PricingVersionId pricingVersionId)
    {
        foreach (var (id, version) in _versionsById)
        {
            if (version.IsActive && id != pricingVersionId)
            {
                _versionsById[id] = version with { IsActive = false };
            }
        }

        var activeVersion = _versionsById[pricingVersionId];
        _versionsById[pricingVersionId] = activeVersion with { IsActive = true };
        _activeVersionId = pricingVersionId;
    }

    private static IReadOnlyCollection<PricingRule> GetDefaultPricingRules() =>
        new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };
}
