using System.Text.Json;
using CheckoutKata.Application.Persistence;
using CheckoutKata.Core;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CheckoutKata.Tests.IntegrationTests.Application.Persistence.Pricing;

[Category("Application")]
[Category("Persistence")]
public class PricingVersionPersistenceApplicationTests
{
    [Test]
    public async Task DbContext_CanStoreAndLoadActivePricingVersionWithRules()
    {
        var dbContextFactory = TestDbContextFactory.Create();
        var rules = new[]
        {
            new PricingRule("A", 60, 3, 150),
            new PricingRule("B", 35, 2, 50),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            dbContext.PricingVersions.Add(new PricingVersionEntity
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                IsActive = true,
                RulesJson = JsonSerializer.Serialize(rules)
            });

            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var activeVersion = await dbContext.PricingVersions.SingleAsync(version => version.IsActive);
            var loadedRules = JsonSerializer.Deserialize<PricingRule[]>(activeVersion.RulesJson);

            Assert.Multiple(() =>
            {
                Assert.That(activeVersion.IsActive, Is.True);
                Assert.That(loadedRules, Is.Not.Null);
                Assert.That(loadedRules!.Length, Is.EqualTo(4));
                Assert.That(loadedRules.Any(rule => rule.Item == "A" && rule.SpecialQuantity == 3 && rule.SpecialPrice == 150), Is.True);
            });
        }
    }

    [Test]
    public async Task DbContext_WhenNewVersionActivated_DeactivatesPreviousActiveVersion()
    {
        var dbContextFactory = TestDbContextFactory.Create();

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            dbContext.PricingVersions.Add(new PricingVersionEntity
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-5),
                IsActive = true,
                RulesJson = JsonSerializer.Serialize(new[] { new PricingRule("A", 50, 3, 130) })
            });

            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            foreach (var activeVersion in dbContext.PricingVersions.Where(version => version.IsActive))
            {
                activeVersion.IsActive = false;
            }

            dbContext.PricingVersions.Add(new PricingVersionEntity
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTimeOffset.UtcNow,
                IsActive = true,
                RulesJson = JsonSerializer.Serialize(new[] { new PricingRule("A", 70, 3, 190) })
            });

            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var activeVersions = await dbContext.PricingVersions.Where(version => version.IsActive).ToListAsync();
            Assert.That(activeVersions, Has.Count.EqualTo(1));
        }
    }
}



