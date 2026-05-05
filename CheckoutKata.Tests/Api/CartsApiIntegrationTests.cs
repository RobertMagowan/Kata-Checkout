using System.Net;
using System.Net.Http.Json;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;
using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Core;
using CheckoutKata.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;

namespace CheckoutKata.Tests.Api;

public class CartsApiIntegrationTests
{
    [Test]
    public async Task CartLifecycle_EndToEnd_ReturnsExpectedSnapshots()
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
