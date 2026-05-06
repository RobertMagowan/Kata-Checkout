using System.Net;
using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Lifecycle;

[Category("Api")]
[Category("Lifecycle")]
[Category("Integration")]
public class CartsLifecycleApiTests
{
    [Test]
    public async Task CartLifecycle_ReturnsExpectedSnapshots()
    {
        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(createdCart, Is.Not.Null);

        var scanManyRequest = new ScanManyRequest(new[] { "A", "A", "A", "B", "B" }, createdCart!.PricingVersionId);
        var scanManyResponse = await client.PostAsJsonAsync($"/api/carts/{createdCart.CartId}/scan-many", scanManyRequest);
        var scannedCart = await scanManyResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        Assert.That(scanManyResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(scannedCart!.TotalPrice, Is.EqualTo(175));

        var getResponse = await client.GetAsync($"/api/carts/{createdCart.CartId}");
        var fetchedCart = await getResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();
        Assert.That(fetchedCart!.TotalPrice, Is.EqualTo(175));

        var clearResponse = await client.PostAsync($"/api/carts/{createdCart.CartId}/clear", null);
        var clearedCart = await clearResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();
        Assert.That(clearedCart!.TotalPrice, Is.EqualTo(0));
        Assert.That(clearedCart.ScannedItems, Is.Empty);
    }
}



