using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Expiry;

[Category("Application")]
[Category("Expiry")]
public class CartExpiryApplicationTests
{
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
}



