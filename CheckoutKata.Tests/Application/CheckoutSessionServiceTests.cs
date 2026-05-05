using System.Text.Json;
using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;
using CheckoutKata.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Tests.Application;

public class CheckoutSessionServiceTests
{
    [Test]
    public async Task CreateCartAsync_PinsLatestPricingVersion()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);

        var snapshot = await service.CreateCartAsync();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var latestVersion = await dbContext.PricingVersions.SingleAsync(version => version.IsActive);

        Assert.That(snapshot.PricingVersionId, Is.EqualTo(PricingVersionId.Parse(latestVersion.Id)));
    }

    [Test]
    public async Task ScanItemAsync_WhenPricingVersionChangesAfterCartCreation_UsesPinnedCartVersion()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);

        var firstCart = await service.CreateCartAsync();
        await ActivatePricingVersionAsync(
            dbContextFactory,
            new[]
            {
                new PricingRule("A", 70, 3, 190),
                new PricingRule("B", 35, 2, 55),
                new PricingRule("C", 20),
                new PricingRule("D", 15)
            });

        var updatedFirstCart = await service.ScanItemAsync(firstCart.CartId, "A");
        var secondCart = await service.CreateCartAsync();
        var updatedSecondCart = await service.ScanItemAsync(secondCart.CartId, "A");

        Assert.Multiple(() =>
        {
            Assert.That(updatedFirstCart.TotalPrice, Is.EqualTo(50));
            Assert.That(updatedSecondCart.TotalPrice, Is.EqualTo(70));
        });
    }

    [Test]
    public async Task ScanItemAsync_WhenRequestedPricingVersionDiffersFromPinnedVersion_ThrowsPricingVersionMismatchException()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);
        var snapshot = await service.CreateCartAsync();
        var mismatchedVersionId = PricingVersionId.New();

        Assert.That(
            async () => await service.ScanItemAsync(
                snapshot.CartId,
                "A",
                mismatchedVersionId),
            Throws.TypeOf<PricingVersionMismatchException>());
    }

    [Test]
    public async Task GetCartAsync_WhenCartExpires_ThrowsCartNotFoundException()
    {
        var now = DateTimeOffset.UtcNow;
        var timeProvider = new TestTimeProvider(now);
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(
            dbContextFactory,
            timeProvider,
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(30),
                MaxCarts = 10_000,
                SweepInterval = TimeSpan.FromMinutes(1)
            });

        var snapshot = await service.CreateCartAsync();
        timeProvider.Advance(TimeSpan.FromMinutes(31));

        Assert.That(
            async () => await service.GetCartAsync(snapshot.CartId),
            Throws.TypeOf<CartNotFoundException>());
    }

    [Test]
    public async Task ScanItemAsync_WhenValidationFails_DoesNotRefreshTtl()
    {
        var now = DateTimeOffset.UtcNow;
        var timeProvider = new TestTimeProvider(now);
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(
            dbContextFactory,
            timeProvider,
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(30),
                MaxCarts = 10_000,
                SweepInterval = TimeSpan.FromMinutes(1)
            });

        var snapshot = await service.CreateCartAsync();
        timeProvider.Advance(TimeSpan.FromMinutes(20));

        Assert.That(
            async () => await service.ScanItemAsync(snapshot.CartId, "a"),
            Throws.TypeOf<ArgumentException>());

        timeProvider.Advance(TimeSpan.FromMinutes(11));

        Assert.That(
            async () => await service.GetCartAsync(snapshot.CartId),
            Throws.TypeOf<CartNotFoundException>());
    }

    [Test]
    public async Task EvictExpiredCarts_RemovesExpiredSessions()
    {
        var now = DateTimeOffset.UtcNow;
        var timeProvider = new TestTimeProvider(now);
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(
            dbContextFactory,
            timeProvider,
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(1),
                MaxCarts = 10_000,
                SweepInterval = TimeSpan.FromMinutes(1)
            });

        await service.CreateCartAsync();
        timeProvider.Advance(TimeSpan.FromMinutes(2));

        var removed = service.EvictExpiredCarts();

        Assert.Multiple(() =>
        {
            Assert.That(removed, Is.EqualTo(1));
            Assert.That(service.ActiveCartCount, Is.EqualTo(0));
        });
    }

    [Test]
    public async Task CreateCartAsync_WhenMaxCapacityReached_ThrowsCartCapacityExceededException()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(
            dbContextFactory,
            TimeProvider.System,
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(30),
                MaxCarts = 1,
                SweepInterval = TimeSpan.FromMinutes(1)
            });

        await service.CreateCartAsync();

        Assert.That(
            async () => await service.CreateCartAsync(),
            Throws.TypeOf<CartCapacityExceededException>());
    }

    private static async Task ActivatePricingVersionAsync(
        IDbContextFactory<CheckoutKataDbContext> dbContextFactory,
        IReadOnlyCollection<PricingRule> rules)
    {
        _ = new Checkout(rules);

        var normalizedRules = rules
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        foreach (var activeVersion in dbContext.PricingVersions.Where(version => version.IsActive))
        {
            activeVersion.IsActive = false;
        }

        dbContext.PricingVersions.Add(new PricingVersionEntity
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
            RulesJson = JsonSerializer.Serialize(normalizedRules)
        });

        await dbContext.SaveChangesAsync();
    }
}
