using System.Collections.Generic;

namespace CheckoutKata.Core;

internal interface IBasketPricer
{
    int CalculateTotalPrice(
        IReadOnlyDictionary<string, int> scannedItemCounts,
        IReadOnlyDictionary<string, PricingRule> pricingRulesByItem);
}
