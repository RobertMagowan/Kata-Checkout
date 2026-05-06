using System.Reflection;
using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Pricing")]
public class CheckoutPricingSafetyTests
{
    [Test]
    public void GetTotalPrice_WhenUnitPriceMultiplicationOverflows_ThrowsOverflowException()
    {
        var checkout = CreateCheckout([new PricingRule("A", int.MaxValue)]);

        checkout.Scan("A");
        checkout.Scan("A");

        Assert.That(checkout.GetTotalPrice, Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void GetTotalPrice_WhenScannedItemHasNoPricingRule_ThrowsInvalidOperationException()
    {
        var checkout = CreateCheckout([new PricingRule("A", 50)]);

        checkout.Scan("A");
        RemovePricingRuleViaReflection(checkout, "A");

        Assert.That(checkout.GetTotalPrice, Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Scan_WhenItemCountOverflows_ThrowsOverflowException()
    {
        var checkout = CreateCheckout([new PricingRule("A", 50)]);

        SetScannedCountViaReflection(checkout, "A", int.MaxValue);

        Assert.That(() => checkout.Scan("A"), Throws.TypeOf<OverflowException>());
    }

    private static void RemovePricingRuleViaReflection(CheckoutKata.Core.Checkout.Checkout checkout, string item)
    {
        var field = typeof(CheckoutKata.Core.Checkout.Checkout).GetField("_pricingRulesByItem", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var pricingRulesByItem = (IReadOnlyDictionary<string, PricingRule>)field.GetValue(checkout)!;

        var updatedLookup = pricingRulesByItem.Where(pair => pair.Key != item)
                                              .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        field.SetValue(checkout, updatedLookup);
    }

    private static void SetScannedCountViaReflection(Checkout checkout, string item, int count)
    {
        var field = typeof(Checkout).GetField("_scannedItemCounts", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var scannedItemCounts = (IDictionary<string, int>)field.GetValue(checkout)!;
        scannedItemCounts[item] = count;
    }

    private static Checkout CreateCheckout(IReadOnlyCollection<PricingRule> rules)
    {
        return new Checkout(rules, new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
    }
}
