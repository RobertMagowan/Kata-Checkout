using System.Net;
using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Application.Carts;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Expiry;

[Category("Api")]
[Category("Expiry")]
[Category("Integration")]
public class CartsExpiryApiTests
{
    [Test]
    public async Task GetCart_WhenCartIsExpired_ReturnsNotFound()
    {
        var timeProvider = new TestTimeProvider(DateTimeOffset.UtcNow);
        using var factory = new ApiTestWebApplicationFactory(
            timeProvider,
            new CartSessionOptions
            {
                SlidingTtl = TimeSpan.FromMinutes(1),
                MaxCarts = 10_000,
                SweepInterval = TimeSpan.FromMinutes(1)
            });
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();
        timeProvider.Advance(TimeSpan.FromMinutes(2));

        var getResponse = await client.GetAsync($"/api/carts/{createdCart!.CartId}");

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}



