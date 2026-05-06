using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutKata.Core;

public sealed class Checkout : ICheckout, ICheckoutStateReader
{
    private readonly Dictionary<string, int> _scannedItemCounts = new(StringComparer.Ordinal);
    private readonly IReadOnlyDictionary<string, PricingRule> _pricingRulesByItem;
    private readonly IScannedItemValidator _itemValidator;
    private readonly IBasketPricer _basketPricer;

    public Checkout(IReadOnlyCollection<PricingRule> pricingRules)
        : this(
            pricingRules,
            new PricingRuleValidator(),
            new ItemValidator(),
            new BasketPricer())
    {
    }

    internal Checkout(
        IReadOnlyCollection<PricingRule> pricingRules,
        IPricingRuleValidator pricingRuleValidator,
        IScannedItemValidator itemValidator,
        IBasketPricer basketPricer)
    {
        ArgumentNullException.ThrowIfNull(pricingRuleValidator);
        ArgumentNullException.ThrowIfNull(itemValidator);
        ArgumentNullException.ThrowIfNull(basketPricer);

        _pricingRulesByItem = pricingRuleValidator.ValidateAndBuildLookup(pricingRules);
        _itemValidator = itemValidator;
        _basketPricer = basketPricer;
    }

    public void Scan(string item)
    {
        var validatedItem = _itemValidator.ValidateScannedItem(item, _pricingRulesByItem);

        if (_scannedItemCounts.TryGetValue(validatedItem, out var count))
        {
            _scannedItemCounts[validatedItem] = count + 1;
            return;
        }

        _scannedItemCounts[validatedItem] = 1;
    }

    public int GetTotalPrice()
    {
        return _basketPricer.CalculateTotalPrice(_scannedItemCounts, _pricingRulesByItem);
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
}
