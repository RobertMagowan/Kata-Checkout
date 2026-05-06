using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Application.Carts;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Parity;

[Category("Api")]
[Category("Parity")]
[Category("Integration")]
public class CartsParityApiTests
{
    [Test]
    public async Task DirectServiceAndApi_WithSameBasket_ProduceSameTotal()
    {
        var basketItems = "DABABA".Select(item => item.ToString()).ToArray();

        var dbContextFactory = TestDbContextFactory.Create();
        var directService = new CheckoutSessionService(dbContextFactory);
        var directCart = await directService.CreateCartAsync();
        var directSnapshot = await directService.ScanManyAsync(directCart.CartId, basketItems, directCart.PricingVersionId);

        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();
        var scanResponse = await client.PostAsJsonAsync(
            $"/api/carts/{createdCart!.CartId}/scan-many",
            new ScanManyRequest(basketItems, createdCart.PricingVersionId));
        var apiSnapshot = await scanResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        Assert.That(apiSnapshot!.TotalPrice, Is.EqualTo(directSnapshot.TotalPrice));
    }
}



