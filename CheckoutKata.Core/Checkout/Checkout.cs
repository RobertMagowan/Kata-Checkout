using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutKata.Core;

public sealed class Checkout : ICheckout, ICheckoutStateReader
{
    private readonly Dictionary<string, int> _scannedItemCounts = new(StringComparer.Ordinal);
    private readonly IReadOnlyDictionary<string, PricingRule> _pricingRulesByItem;
    private readonly ICheckoutEngine _checkoutEngine;

    public Checkout(IReadOnlyCollection<PricingRule> pricingRules)
        : this(pricingRules, CreateDefaultEngine())
    {
    }

    public Checkout(
        IReadOnlyCollection<PricingRule> pricingRules,
        ICheckoutEngine checkoutEngine)
    {
        _checkoutEngine = checkoutEngine ?? throw new ArgumentNullException(nameof(checkoutEngine));

        _pricingRulesByItem = _checkoutEngine.BuildPricingRulesLookup(pricingRules);
    }

    public void Scan(string item)
    {
        var validatedItem = _checkoutEngine.ValidateScannedItem(item, _pricingRulesByItem);

        if (_scannedItemCounts.TryGetValue(validatedItem, out var count))
        {
            _scannedItemCounts[validatedItem] = checked(count + 1);
            return;
        }

        _scannedItemCounts[validatedItem] = 1;
    }

    public int GetTotalPrice()
    {
        return _checkoutEngine.CalculateTotalPrice(_scannedItemCounts, _pricingRulesByItem);
    }

    public void Clear()
    {
        _scannedItemCounts.Clear();
    }

    public IReadOnlyList<ScannedItemCount> GetScannedItems()
    {
        return _scannedItemCounts
            .OrderBy(itemCount => itemCount.Key, StringComparer.Ordinal)
            .Select(itemCount => new ScannedItemCount(itemCount.Key, itemCount.Value))
            .ToArray();
    }

    public IReadOnlyList<PricingRule> GetPricingRules()
    {
        return _pricingRulesByItem.Values
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();
    }

    private static ICheckoutEngine CreateDefaultEngine()
    {
        return new DefaultCheckoutEngine(
            new PricingRuleValidator(),
            new ItemValidator(),
            new BasketPricer());
    }
}
