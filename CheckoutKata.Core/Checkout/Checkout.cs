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
    private readonly IReadOnlyList<PricingRule> _pricingRules;

    public Checkout(IReadOnlyCollection<PricingRule> pricingRules)
        : this(
            pricingRules,
            new ItemValidator(),
            new BasketPricer(),
            new PricingRuleValidator())
    {
    }

    public Checkout(
        IReadOnlyCollection<PricingRule> pricingRules,
        IScannedItemValidator itemValidator,
        IBasketPricer basketPricer)
        : this(
            pricingRules,
            itemValidator,
            basketPricer,
            new PricingRuleValidator())
    {
    }

    internal Checkout(
        IReadOnlyCollection<PricingRule> pricingRules,
        IScannedItemValidator itemValidator,
        IBasketPricer basketPricer,
        IPricingRuleValidator pricingRuleValidator)
    {
        ArgumentNullException.ThrowIfNull(itemValidator);
        ArgumentNullException.ThrowIfNull(basketPricer);
        ArgumentNullException.ThrowIfNull(pricingRuleValidator);

        _pricingRulesByItem = pricingRuleValidator.ValidateAndBuildLookup(pricingRules);
        _pricingRules = _pricingRulesByItem.Values
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();
        _itemValidator = itemValidator;
        _basketPricer = basketPricer;
    }

    public void Scan(string item)
    {
        var validatedItem = _itemValidator.ValidateScannedItem(item, _pricingRules);

        if (_scannedItemCounts.TryGetValue(validatedItem, out var count))
        {
            _scannedItemCounts[validatedItem] = checked(count + 1);
            return;
        }

        _scannedItemCounts[validatedItem] = 1;
    }

    public int GetTotalPrice()
    {
        var scannedItemCounts = _scannedItemCounts
            .Select(itemCount => new ScannedItemCount(itemCount.Key, itemCount.Value))
            .ToArray();

        return _basketPricer.CalculateTotalPrice(scannedItemCounts, _pricingRules);
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
        return _pricingRules;
    }
}
