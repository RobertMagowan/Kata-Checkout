using System.Collections.Generic;
using System.Linq;

namespace CheckoutKata.Core;

public sealed class BasketPricer : IBasketPricer
{
    public int CalculateTotalPrice(
        IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
        IReadOnlyCollection<PricingRule> pricingRules)
    {
        ArgumentNullException.ThrowIfNull(scannedItemCounts);
        ArgumentNullException.ThrowIfNull(pricingRules);

        var pricingRulesByItem = pricingRules
            .ToDictionary(rule => rule.Item, StringComparer.Ordinal);
        var totalPrice = 0;

        foreach (var scannedItem in scannedItemCounts)
        {
            var item = scannedItem.Item;
            var count = scannedItem.Quantity;

            if (!pricingRulesByItem.TryGetValue(item, out var rule))
            {
                throw new InvalidOperationException($"No pricing rule found for scanned item '{item}'.");
            }

            totalPrice = checked(totalPrice + CalculateItemPrice(rule, count));
        }

        return totalPrice;
    }

    private static int CalculateItemPrice(PricingRule rule, int count)
    {
        if (rule.SpecialQuantity is null || rule.SpecialPrice is null)
        {
            return checked(rule.UnitPrice * count);
        }

        var specialQuantity = rule.SpecialQuantity.Value;
        var specialApplications = count / specialQuantity;
        var remainingItems = count % specialQuantity;

        return checked((specialApplications * rule.SpecialPrice.Value) + (remainingItems * rule.UnitPrice));
    }
}
