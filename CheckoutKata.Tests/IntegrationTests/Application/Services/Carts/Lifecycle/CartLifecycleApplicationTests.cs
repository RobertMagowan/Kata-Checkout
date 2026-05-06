using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Lifecycle;

[Category("Application")]
[Category("Lifecycle")]
public class CartLifecycleApplicationTests
{
    [Test]
    public async Task CreateCartAsync_PinsLatestPricingVersion()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);

        var snapshot = await service.CreateCartAsync();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var latestVersion = await dbContext.PricingVersions.SingleAsync(version => version.IsActive);

        Assert.That(snapshot.PricingVersionId, Is.EqualTo(PricingVersionId.Parse(latestVersion.Id)));
    }

    [Test]
    public async Task ClearAsync_WhenCartContainsItems_ResetsTotalAndScannedItems()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);

        var cart = await service.CreateCartAsync();
        var scanned = await service.ScanManyAsync(cart.CartId, new[] { "A", "A", "B" }, cart.PricingVersionId);

        Assert.That(scanned.TotalPrice, Is.GreaterThan(0));

        var cleared = await service.ClearAsync(cart.CartId);

        Assert.Multiple(() =>
        {
            Assert.That(cleared.TotalPrice, Is.EqualTo(0));
            Assert.That(cleared.ScannedItems, Is.Empty);
        });
    }
}



