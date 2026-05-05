using System.Collections.Generic;

namespace CheckoutKata.Core;

public interface ICheckout
{
    void Scan(string item);

    int GetTotalPrice();

    void Clear();

    IReadOnlyDictionary<string, int> GetScannedItemCounts();

    IReadOnlyCollection<PricingRule> GetPricingRules();
}
