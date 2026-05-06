using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Capacity;

[Category("Application")]
[Category("Capacity")]
public class CartCapacityApplicationTests
{
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
}



