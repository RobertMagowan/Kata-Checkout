using System.Collections.Generic;

namespace CheckoutKata.Core;

internal sealed class BasketPricer : IBasketPricer
{
    public int CalculateTotalPrice(
        IReadOnlyDictionary<string, int> scannedItemCounts,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
    {
        var totalPrice = 0;

        foreach (var (item, count) in scannedItemCounts)
        {
            if (!pricingRulesByItem.TryGetValue(item, out var rule))
            {
                continue;
            }

            totalPrice = checked(totalPrice + CalculateItemPrice(rule, count));
        }

        return totalPrice;
    }

    private static int CalculateItemPrice(PricingRule rule, int count)
    {
        if (rule.SpecialQuantity is null || rule.SpecialPrice is null)
        {
            return rule.UnitPrice * count;
        }

        var specialQuantity = rule.SpecialQuantity.Value;
        var specialApplications = count / specialQuantity;
        var remainingItems = count % specialQuantity;

        return checked((specialApplications * rule.SpecialPrice.Value) + (remainingItems * rule.UnitPrice));
    }
}
