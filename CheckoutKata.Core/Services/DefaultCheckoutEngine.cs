using System;
using System.Collections.Generic;

namespace CheckoutKata.Core;

internal sealed class DefaultCheckoutEngine : ICheckoutEngine
{
    private readonly IPricingRuleValidator _pricingRuleValidator;
    private readonly IScannedItemValidator _scannedItemValidator;
    private readonly IBasketPricer _basketPricer;

    public DefaultCheckoutEngine(
        IPricingRuleValidator pricingRuleValidator,
        IScannedItemValidator scannedItemValidator,
        IBasketPricer basketPricer)
    {
        _pricingRuleValidator = pricingRuleValidator ?? throw new ArgumentNullException(nameof(pricingRuleValidator));
        _scannedItemValidator = scannedItemValidator ?? throw new ArgumentNullException(nameof(scannedItemValidator));
        _basketPricer = basketPricer ?? throw new ArgumentNullException(nameof(basketPricer));
    }

    public IReadOnlyDictionary<string, PricingRule> BuildPricingRulesLookup(IReadOnlyCollection<PricingRule> pricingRules)
    {
        return _pricingRuleValidator.ValidateAndBuildLookup(pricingRules);
    }

    public string ValidateScannedItem(
        string item,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
    {
        return _scannedItemValidator.ValidateScannedItem(item, pricingRulesByItem);
    }

    public int CalculateTotalPrice(
        IReadOnlyDictionary<string, int> scannedItemCounts,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
    {
        return _basketPricer.CalculateTotalPrice(scannedItemCounts, pricingRulesByItem);
    }
}
