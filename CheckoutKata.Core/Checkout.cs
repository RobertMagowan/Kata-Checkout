using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

    public void Clear()
    {
        _scannedItemCounts.Clear();
    }

    public IReadOnlyDictionary<string, int> GetScannedItemCounts()
    {
        return new ReadOnlyDictionary<string, int>(
            new Dictionary<string, int>(_scannedItemCounts, StringComparer.Ordinal));
    }

    public IReadOnlyCollection<PricingRule> GetPricingRules()
    {
        return _pricingRulesByItem.Values.ToArray();
    }
}
