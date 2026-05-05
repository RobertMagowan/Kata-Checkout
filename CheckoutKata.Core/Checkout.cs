using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutKata.Core;

public sealed class Checkout(IReadOnlyCollection<PricingRule> pricingRules) : ICheckout
{
    private readonly Dictionary<string, int> _scannedItemCounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PricingRule> _pricingRulesByItem = pricingRules
        .ToDictionary(rule => rule.Item, StringComparer.Ordinal);

    public void Scan(string item)
    {
        if (_scannedItemCounts.TryGetValue(item, out var count))
        {
            _scannedItemCounts[item] = count + 1;
            return;
        }

        _scannedItemCounts[item] = 1;
    }

    public int GetTotalPrice()
    {
        var totalPrice = 0;

        foreach (var (item, count) in _scannedItemCounts)
        {
            if (!_pricingRulesByItem.TryGetValue(item, out var rule))
            {
                continue;
            }

            totalPrice += CalculatePriceForItem(rule, count);
        }

        return totalPrice;
    }

    private static int CalculatePriceForItem(PricingRule rule, int count)
    {
        if (rule.SpecialQuantity is null || rule.SpecialPrice is null)
        {
            return rule.UnitPrice * count;
        }

        return count >= rule.SpecialQuantity
            ? rule.SpecialPrice.Value + ((count - rule.SpecialQuantity.Value) * rule.UnitPrice)
            : count * rule.UnitPrice;
    }
}
