namespace CheckoutKata.Core.Checkout;

using CheckoutKata.Core.Interfaces;
using Models;
using Services;

public sealed class Checkout : ICheckout, ICheckoutStateReader
{
    private readonly Dictionary<string, int> _scannedItemCounts = new(StringComparer.Ordinal);
    private readonly IScannedItemValidator _itemValidator;
    private readonly IBasketPricer _basketPricer;
    private readonly IReadOnlyList<PricingRule> _pricingRules;

    public Checkout(IReadOnlyCollection<PricingRule> pricingRules, IScannedItemValidator itemValidator,
                    IBasketPricer basketPricer, IPricingRuleValidator pricingRuleValidator)
    {
        ArgumentNullException.ThrowIfNull(itemValidator);
        ArgumentNullException.ThrowIfNull(basketPricer);
        ArgumentNullException.ThrowIfNull(pricingRuleValidator);
        ArgumentNullException.ThrowIfNull(pricingRules);

        _pricingRules = pricingRuleValidator.ValidateAndBuildLookup(pricingRules)
                                            .Values
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
        var scannedItemCounts = _scannedItemCounts.Select(itemCount => new ScannedItemCount(itemCount.Key, itemCount.Value))
                                                  .ToArray();

        return _basketPricer.CalculateTotalPrice(scannedItemCounts, _pricingRules);
    }

    public void Clear() { _scannedItemCounts.Clear(); }

    public IReadOnlyList<ScannedItemCount> GetScannedItems()
    {
        return _scannedItemCounts.OrderBy(itemCount => itemCount.Key, StringComparer.Ordinal)
                                 .Select(itemCount => new ScannedItemCount(itemCount.Key, itemCount.Value))
                                 .ToArray();
    }

    public IReadOnlyList<PricingRule> GetPricingRules() { return _pricingRules; }
}
