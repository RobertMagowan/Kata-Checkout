using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies.DiscountPolicy;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("State")]
public class CheckoutStateReaderTests
{
    [Test]
    public void GetScannedItems_WithScannedItems_ReturnsAggregatedCounts()
    {
        var checkout = CreateCheckout();

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
        var checkout = CreateCheckout();

        var pricingRules = checkout.GetPricingRules();

        var itemARule = pricingRules.Single(rule => rule.Item == "A");
        var itemBRule = pricingRules.Single(rule => rule.Item == "B");
        var itemCRule = pricingRules.Single(rule => rule.Item == "C");
        var itemAPolicy = itemARule.DiscountPolicies!.Single();
        var itemBPolicy = itemBRule.DiscountPolicies!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(pricingRules.Count, Is.EqualTo(4));
            Assert.That(itemARule.UnitPrice, Is.EqualTo(50));
            Assert.That(itemAPolicy.CalculatePrice(3, itemARule.UnitPrice), Is.EqualTo(130));
            Assert.That(itemBPolicy.CalculatePrice(2, itemBRule.UnitPrice), Is.EqualTo(45));
            Assert.That(itemCRule.DiscountPolicies, Is.Null.Or.Empty);
        });
    }

    private static CheckoutKata.Core.Checkout.Checkout CreateCheckout()
    {
        return new CheckoutKata.Core.Checkout.Checkout(CreateDefaultRules(), new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
    }

    private static IReadOnlyCollection<PricingRule> CreateDefaultRules() =>
    [
        CreateNForXRule("A", 50, 3, 130),
        CreateNForXRule("B", 30, 2, 45),
        new("C", 20),
        new("D", 15)
    ];

    private static PricingRule CreateNForXRule(string item, int unitPrice, int quantity, int price)
    {
        return new PricingRule(item, unitPrice, DiscountPolicies: [new NForXDiscountPolicy(quantity, price)]);
    }
}



