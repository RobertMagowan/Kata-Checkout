namespace CheckoutKata.Core.Interfaces;

using System.Collections.Generic;
using Models;

public interface IBasketPricer
{
    int CalculateTotalPrice(IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
                            IReadOnlyDictionary<string, PricingRule> pricingRulesByItem);
}
