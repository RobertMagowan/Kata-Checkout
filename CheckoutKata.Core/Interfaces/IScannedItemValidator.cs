namespace CheckoutKata.Core.Interfaces;

using System.Collections.Generic;
using Models;

public interface IScannedItemValidator
{
    string ValidateScannedItem(string item,
                               IReadOnlyCollection<PricingRule> pricingRules);
}
