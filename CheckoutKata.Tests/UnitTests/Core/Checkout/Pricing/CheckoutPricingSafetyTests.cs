using System.Reflection;
using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Pricing;

[Category("Core")]
[Category("Pricing")]
public class CheckoutPricingSafetyTests
{
    [Test]
    public void GetTotalPrice_WhenUnitPriceMultiplicationOverflows_ThrowsOverflowException()
    {
        var checkout = new global::CheckoutKata.Core.Checkout(new[]
        {
            new PricingRule("A", int.MaxValue)
        });

        checkout.Scan("A");
        checkout.Scan("A");

        Assert.That(
            () => checkout.GetTotalPrice(),
            Throws.TypeOf<OverflowException>());
    }

    [Test]
    public void GetTotalPrice_WhenScannedItemHasNoPricingRule_ThrowsInvalidOperationException()
    {
        var checkout = new global::CheckoutKata.Core.Checkout(new[]
        {
            new PricingRule("A", 50)
        });

        checkout.Scan("A");
        RemovePricingRuleViaReflection(checkout, "A");

        Assert.That(
            () => checkout.GetTotalPrice(),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void Scan_WhenItemCountOverflows_ThrowsOverflowException()
    {
        var checkout = new global::CheckoutKata.Core.Checkout(new[]
        {
            new PricingRule("A", 50)
        });

        SetScannedCountViaReflection(checkout, "A", int.MaxValue);

        Assert.That(
            () => checkout.Scan("A"),
            Throws.TypeOf<OverflowException>());
    }

    private static void RemovePricingRuleViaReflection(global::CheckoutKata.Core.Checkout checkout, string item)
    {
        var field = typeof(global::CheckoutKata.Core.Checkout)
            .GetField("_pricingRulesByItem", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var pricingRulesByItem = (IDictionary<string, PricingRule>)field.GetValue(checkout)!;
        pricingRulesByItem.Remove(item);
    }

    private static void SetScannedCountViaReflection(global::CheckoutKata.Core.Checkout checkout, string item, int count)
    {
        var field = typeof(global::CheckoutKata.Core.Checkout)
            .GetField("_scannedItemCounts", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var scannedItemCounts = (IDictionary<string, int>)field.GetValue(checkout)!;
        scannedItemCounts[item] = count;
    }
}
