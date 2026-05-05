using System;
using System.Collections.Generic;

namespace CheckoutKata.Core;

public sealed class Checkout : ICheckout
{
    private readonly Dictionary<string, int> _scannedItemCounts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PricingRule> _pricingRulesByItem;

    public Checkout(IReadOnlyCollection<PricingRule> pricingRules)
    {
        _pricingRulesByItem = PricingRuleValidator.ValidateAndBuildLookup(pricingRules);
    }

    public void Scan(string item)
    {
        var validatedItem = ItemValidator.ValidateScannedItem(item, _pricingRulesByItem);

        if (_scannedItemCounts.TryGetValue(validatedItem, out var count))
        {
            _scannedItemCounts[validatedItem] = count + 1;
            return;
        }

        _scannedItemCounts[validatedItem] = 1;
    }

    public int GetTotalPrice()
    {
        return BasketPricer.CalculateTotalPrice(_scannedItemCounts, _pricingRulesByItem);
    }
}
