using System.Collections.Generic;

namespace CheckoutKata.Core;

public interface ICheckoutEngine
{
    IReadOnlyDictionary<string, PricingRule> BuildPricingRulesLookup(IReadOnlyCollection<PricingRule> pricingRules);

    string ValidateScannedItem(
        string item,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem);

    int CalculateTotalPrice(
        IReadOnlyDictionary<string, int> scannedItemCounts,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem);
}
