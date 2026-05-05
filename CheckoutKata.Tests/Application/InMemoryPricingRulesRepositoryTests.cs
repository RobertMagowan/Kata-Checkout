using CheckoutKata.Application.Exceptions;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;

namespace CheckoutKata.Tests.Application;

public class InMemoryPricingVersionRepositoryTests
{
    [Test]
    public async Task GetLatestVersionAsync_WithDefaultSetup_ReturnsActiveVersionWithRules()
    {
        var repository = new InMemoryPricingVersionRepository();

        var latestVersion = await repository.GetLatestVersionAsync();

        Assert.Multiple(() =>
        {
            Assert.That(latestVersion.IsActive, Is.True);
            Assert.That(latestVersion.Rules.Count, Is.EqualTo(4));
            Assert.That(latestVersion.Rules.Any(rule => rule.Item == "A"), Is.True);
        });
    }

    [Test]
    public async Task CreateVersionAsync_WithSetAsActiveTrue_MakesCreatedVersionLatest()
    {
        var repository = new InMemoryPricingVersionRepository();
        var newRules = new[]
        {
            new PricingRule("A", 60, 3, 150),
            new PricingRule("B", 35, 2, 50),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        var createdVersion = await repository.CreateVersionAsync(newRules, setAsActive: true);
        var latestVersion = await repository.GetLatestVersionAsync();

        Assert.That(latestVersion.Id, Is.EqualTo(createdVersion.Id));
    }

    [Test]
    public async Task SetActiveVersionAsync_WithUnknownVersion_ThrowsUnknownPricingVersionException()
    {
        var repository = new InMemoryPricingVersionRepository();

        Assert.That(
            async () => await repository.SetActiveVersionAsync(PricingVersionId.New()),
            Throws.TypeOf<UnknownPricingVersionException>());
    }

    [Test]
    public async Task GetVersionAsync_WithUnknownVersion_ReturnsNull()
    {
        var repository = new InMemoryPricingVersionRepository();

        var pricingVersion = await repository.GetVersionAsync(PricingVersionId.New());

        Assert.That(pricingVersion, Is.Null);
    }
}
