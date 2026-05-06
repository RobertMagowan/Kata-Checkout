using System.Collections.Generic;

namespace CheckoutKata.Core;

public interface IScannedItemValidator
{
    string ValidateScannedItem(
        string item,
        IReadOnlyCollection<PricingRule> pricingRules);
}
