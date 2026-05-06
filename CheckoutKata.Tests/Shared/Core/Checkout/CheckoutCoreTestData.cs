using CheckoutKata.Core;

namespace CheckoutKata.Tests.Shared.Core;

using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Services;

internal static class CheckoutCoreTestData
{
    public static global::CheckoutKata.Core.Checkout.Checkout CreateCheckout()
    {
        return CreateCheckout(CreateDefaultRules());
    }

    public static global::CheckoutKata.Core.Checkout.Checkout CreateCheckout(IReadOnlyCollection<PricingRule> rules)
    {
        return new global::CheckoutKata.Core.Checkout.Checkout(
                                                               rules,
                                                               new ItemValidator(),
                                                               new BasketPricer(),
                                                               new PricingRuleValidator());
    }

    public static IReadOnlyCollection<PricingRule> CreateDefaultRules() =>
        new[]
        {
            CreateNForXRule("A", 50, 3, 130),
            CreateNForXRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

    public static PricingRule CreateNForXRule(string item, int unitPrice, int quantity, int price)
    {
        return new PricingRule(
            item,
            unitPrice,
            DiscountPolicies:
            [
                new NForXDiscountPolicy(quantity, price)
            ]);
    }

    public static PricingRule CreatePercentOffRule(string item, int unitPrice, int percentage)
    {
        return new PricingRule(
            item,
            unitPrice,
            DiscountPolicies:
            [
                new PercentOffDiscountPolicy(percentage)
            ]);
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
