using System.Text.Json;
using CheckoutKata.Application.Carts;
using CheckoutKata.Application.Exceptions;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Versioning;

[Category("Application")]
[Category("Versioning")]
public class CartPricingVersionApplicationTests
{
    [Test]
    public async Task ScanItemAsync_WhenPricingVersionChangesAfterCartCreation_UsesPinnedCartVersion()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);

        var firstCart = await service.CreateCartAsync();
        await ActivatePricingVersionAsync(
            dbContextFactory,
            new[]
            {
                new PricingRule("A", 70, 3, 190),
                new PricingRule("B", 35, 2, 55),
                new PricingRule("C", 20),
                new PricingRule("D", 15)
            });

        var updatedFirstCart = await service.ScanItemAsync(firstCart.CartId, "A");
        var secondCart = await service.CreateCartAsync();
        var updatedSecondCart = await service.ScanItemAsync(secondCart.CartId, "A");

        Assert.Multiple(() =>
        {
            Assert.That(updatedFirstCart.TotalPrice, Is.EqualTo(50));
            Assert.That(updatedSecondCart.TotalPrice, Is.EqualTo(70));
        });
    }

    [Test]
    public async Task ScanItemAsync_WhenRequestedPricingVersionDiffersFromPinnedVersion_ThrowsPricingVersionMismatchException()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var service = new CheckoutSessionService(dbContextFactory);
        var snapshot = await service.CreateCartAsync();
        var mismatchedVersionId = PricingVersionId.New();

        Assert.That(
            async () => await service.ScanItemAsync(
                snapshot.CartId,
                "A",
                mismatchedVersionId),
            Throws.TypeOf<PricingVersionMismatchException>());
    }

    private static async Task ActivatePricingVersionAsync(
        IDbContextFactory<CheckoutKataDbContext> dbContextFactory,
        IReadOnlyCollection<PricingRule> rules)
    {
        _ = new Checkout(rules);

        var normalizedRules = rules
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        foreach (var activeVersion in dbContext.PricingVersions.Where(version => version.IsActive))
        {
            activeVersion.IsActive = false;
        }

        dbContext.PricingVersions.Add(new PricingVersionEntity
        {
            Id = Guid.NewGuid(),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
            RulesJson = JsonSerializer.Serialize(normalizedRules)
        });

        await dbContext.SaveChangesAsync();
    }
}



