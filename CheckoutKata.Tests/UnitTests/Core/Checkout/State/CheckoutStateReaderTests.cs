using CheckoutKata.Tests.Shared.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.State;

[Category("Core")]
[Category("State")]
public class CheckoutStateReaderTests
{
    [Test]
    public void GetScannedItems_WithScannedItems_ReturnsAggregatedCounts()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        checkout.Scan("B");
        checkout.Scan("A");
        checkout.Scan("B");

        var scannedItems = checkout.GetScannedItems();
        var itemA = scannedItems.Single(item => item.Item == "A");
        var itemB = scannedItems.Single(item => item.Item == "B");

        Assert.Multiple(() =>
        {
            Assert.That(scannedItems.Count, Is.EqualTo(2));
            Assert.That(itemA.Quantity, Is.EqualTo(1));
            Assert.That(itemB.Quantity, Is.EqualTo(2));
        });
    }

    [Test]
    public void GetPricingRules_ReturnsConfiguredPricingRules()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        var pricingRules = checkout.GetPricingRules();

        var itemARule = pricingRules.Single(rule => rule.Item == "A");
        var itemBRule = pricingRules.Single(rule => rule.Item == "B");
        var itemCRule = pricingRules.Single(rule => rule.Item == "C");

        Assert.Multiple(() =>
        {
            Assert.That(pricingRules.Count, Is.EqualTo(4));
            Assert.That(itemARule.UnitPrice, Is.EqualTo(50));
            Assert.That(itemARule.SpecialQuantity, Is.EqualTo(3));
            Assert.That(itemARule.SpecialPrice, Is.EqualTo(130));
            Assert.That(itemBRule.SpecialQuantity, Is.EqualTo(2));
            Assert.That(itemBRule.SpecialPrice, Is.EqualTo(45));
            Assert.That(itemCRule.SpecialQuantity, Is.Null);
            Assert.That(itemCRule.SpecialPrice, Is.Null);
        });
    }
}



