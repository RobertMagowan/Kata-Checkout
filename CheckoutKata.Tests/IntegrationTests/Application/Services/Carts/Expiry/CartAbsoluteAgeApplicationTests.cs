using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Expiry;

[Category("Application")]
[Category("Expiry")]
public class CartAbsoluteAgeApplicationTests
{
    [Test]
    public async Task GetCartAsync_WhenAbsoluteMaxAgeExpires_ThrowsCartNotFoundException()
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
                SweepInterval = TimeSpan.FromMinutes(1),
                AbsoluteMaxAge = TimeSpan.FromMinutes(10)
            });

        var snapshot = await service.CreateCartAsync();
        timeProvider.Advance(TimeSpan.FromMinutes(9));
        _ = await service.ScanItemAsync(snapshot.CartId, "A", snapshot.PricingVersionId);
        timeProvider.Advance(TimeSpan.FromMinutes(2));

        Assert.That(
            async () => await service.GetCartAsync(snapshot.CartId, snapshot.PricingVersionId),
            Throws.TypeOf<CartNotFoundException>());
    }
}



