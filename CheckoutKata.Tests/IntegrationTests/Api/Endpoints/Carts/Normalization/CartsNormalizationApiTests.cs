using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Normalization;

[Category("Api")]
[Category("Normalization")]
[Category("Integration")]
public class CartsNormalizationApiTests
{
    [Test]
    public async Task ScanItem_NormalizesBoundaryInput()
    {
        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        var scanResponse = await client.PostAsJsonAsync(
            $"/api/carts/{createdCart!.CartId}/scan",
            new ScanItemRequest(" a ", createdCart.PricingVersionId));
        var scannedCart = await scanResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        var itemA = scannedCart!.ScannedItems.Single(item => item.Item == "A");

        Assert.That(itemA.Quantity, Is.EqualTo(1));
    }
}



