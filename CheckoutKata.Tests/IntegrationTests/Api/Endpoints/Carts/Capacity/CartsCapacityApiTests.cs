using System.Net;
using CheckoutKata.Application.Carts;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Capacity;

[Category("Api")]
[Category("Capacity")]
[Category("Integration")]
public class CartsCapacityApiTests
{
    [Test]
    public async Task CreateCart_WhenCapacityReached_ReturnsTooManyRequests()
    {
        using var factory = new ApiTestWebApplicationFactory(
            new TestTimeProvider(DateTimeOffset.UtcNow),
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(30),
                MaxCarts = 1,
                SweepInterval = TimeSpan.FromMinutes(1)
            });
        using var client = factory.CreateClient();

        var firstCreateResponse = await client.PostAsync("/api/carts", null);
        var secondCreateResponse = await client.PostAsync("/api/carts", null);

        Assert.Multiple(() =>
        {
            Assert.That(firstCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(secondCreateResponse.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }
}



