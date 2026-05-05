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
        return BasketPricer.CalculateTotalPrice(_scannedItemCounts, _pricingRulesByItem);
    }
}
