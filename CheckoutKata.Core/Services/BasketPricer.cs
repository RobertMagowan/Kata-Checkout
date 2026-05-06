namespace CheckoutKata.Core.Services;

using Interfaces;
using Models;

public sealed class BasketPricer : IBasketPricer
{
    public int CalculateTotalPrice(IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
                                   IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
    {
        ArgumentNullException.ThrowIfNull(scannedItemCounts);
        ArgumentNullException.ThrowIfNull(pricingRulesByItem);
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
        var basePrice = checked(rule.UnitPrice * count);
        var bestPrice = basePrice;

        if (rule.DiscountPolicies is null || rule.DiscountPolicies.Count == 0)
        {
            return bestPrice;
        }

        foreach (var policy in rule.DiscountPolicies)
        {
            ArgumentNullException.ThrowIfNull(policy);

            var candidatePrice = policy.CalculatePrice(count, rule.UnitPrice);
            bestPrice = Math.Min(bestPrice, candidatePrice);
        }

        return bestPrice;
    }
}
