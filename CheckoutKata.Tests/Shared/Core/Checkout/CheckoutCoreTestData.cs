using CheckoutKata.Core;

namespace CheckoutKata.Tests.Shared.Core;

internal static class CheckoutCoreTestData
{
    public static global::CheckoutKata.Core.Checkout CreateCheckout()
    {
        var rules = new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        return new global::CheckoutKata.Core.Checkout(rules);
    }

    public static void ScanMany(ICheckout checkout, string basket)
    {
        foreach (var item in basket)
        {
            checkout.Scan(item.ToString());
        }
    }

    public static void ScanMany(ICheckout checkout, IReadOnlyCollection<string> items)
    {
        foreach (var item in items)
        {
            checkout.Scan(item);
        }
    }
}
