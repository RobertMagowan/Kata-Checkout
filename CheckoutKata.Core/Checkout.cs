using System.Collections.Generic;

namespace CheckoutKata.Core;

public sealed class Checkout(IReadOnlyCollection<PricingRule> pricingRules) : ICheckout
{
    private readonly IReadOnlyCollection<PricingRule> _pricingRules = pricingRules;

    public void Scan(string item)
    {
        _ = item;
    }

    public int GetTotalPrice()
    {
        return 0;
    }
}
