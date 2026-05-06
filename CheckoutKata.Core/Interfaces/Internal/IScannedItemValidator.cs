using System.Collections.Generic;

namespace CheckoutKata.Core;

internal interface IScannedItemValidator
{
    string ValidateScannedItem(
        string item,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem);
}
