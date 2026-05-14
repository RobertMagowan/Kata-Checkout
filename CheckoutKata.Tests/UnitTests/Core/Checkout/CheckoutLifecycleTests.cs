using CheckoutKata.Core;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies.DiscountPolicy;
using CheckoutKata.Core.Services;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;

[Category("Core")]
[Category("Lifecycle")]
public class CheckoutLifecycleTests
{
    [Test]
    public void Clear_WithPreviouslyScannedItems_ResetsTotalAndScannedItems()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "AAABB");
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var scannedItems = checkout.GetScannedItems();

        Assert.That(totalPrice, Is.EqualTo(0));
        Assert.That(scannedItems, Is.Empty);
    }

    [Test]
    public void Clear_WhenCalledMultipleTimes_RemainsSafeAndKeepsBasketEmpty()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "ABCD");
        checkout.Clear();
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var scannedItems = checkout.GetScannedItems();

        Assert.That(totalPrice, Is.EqualTo(0));
        Assert.That(scannedItems, Is.Empty);
    }

    private static Checkout CreateCheckout()
    {
        return new Checkout(CreateDefaultRules(), new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
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

    private static void ScanMany(ICheckout checkout, string basket)
    {
        foreach (var item in basket)
        {
            checkout.Scan(item.ToString());
        }
    }
}
