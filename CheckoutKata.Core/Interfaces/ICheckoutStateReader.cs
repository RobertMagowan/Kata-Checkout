namespace CheckoutKata.Core.Interfaces;

using System.Collections.Generic;
using Models;

public interface ICheckoutStateReader
{
    IReadOnlyList<ScannedItemCount> GetScannedItems();

    IReadOnlyList<PricingRule> GetPricingRules();
}
