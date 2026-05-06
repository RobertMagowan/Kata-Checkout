using System.Collections.Generic;

namespace CheckoutKata.Core;

public interface IBasketPricer
{
    int CalculateTotalPrice(
        IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
        IReadOnlyCollection<PricingRule> pricingRules);
}
