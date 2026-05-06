using System.Net;
using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Tests.Shared.Infrastructure.Api;
using CheckoutKata.Tests.Shared.Infrastructure.Time;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutKata.Tests.IntegrationTests.Api.Endpoints.Carts.Conflicts;

[Category("Api")]
[Category("Conflicts")]
[Category("Integration")]
public class CartsVersionConflictApiTests
{
    [Test]
    public async Task ScanItem_WhenPricingVersionMismatches_ReturnsConflictProblemDetails()
    {
        using var factory = new ApiTestWebApplicationFactory(new TestTimeProvider(DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsync("/api/carts", null);
        var createdCart = await createResponse.Content.ReadFromJsonAsync<CartSnapshotResponse>();
        var mismatchedVersion = Guid.NewGuid();

        var scanResponse = await client.PostAsJsonAsync(
            $"/api/carts/{createdCart!.CartId}/scan",
            new ScanItemRequest("A", mismatchedVersion));

        var problemDetails = await scanResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Multiple(() =>
        {
            Assert.That(scanResponse.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(problemDetails, Is.Not.Null);
            Assert.That(problemDetails!.Title, Is.EqualTo("Pricing Version Mismatch"));
        });
    }
}



