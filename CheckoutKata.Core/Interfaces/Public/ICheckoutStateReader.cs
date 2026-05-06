using System.Collections.Generic;

namespace CheckoutKata.Core;

public interface ICheckoutStateReader
{
    IReadOnlyList<ScannedItemCount> GetScannedItems();

    IReadOnlyList<PricingRule> GetPricingRules();
}
