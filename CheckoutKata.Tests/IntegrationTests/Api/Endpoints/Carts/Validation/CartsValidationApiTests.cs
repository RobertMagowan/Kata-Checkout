using System.Net;
using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Validation;

[Category("Api")]
[Category("Validation")]
[Category("Integration")]
public class CartsValidationApiTests
{
    [Test]
    public async Task ScanMany_WithNoItems_ReturnsBadRequest()
    {
        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/carts/{createdCart!.CartId}/scan-many",
            new ScanManyRequest(Array.Empty<string>(), createdCart.PricingVersionId));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Scan_WithNullItem_ReturnsBadRequest()
    {
        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();

        var response = await client.PostAsJsonAsync(
            $"/api/carts/{createdCart!.CartId}/scan",
            new ScanItemRequest(null!, createdCart.PricingVersionId));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}



