using System.Collections.Generic;

namespace CheckoutKata.Core;

internal interface IPricingRuleValidator
{
    IReadOnlyDictionary<string, PricingRule> ValidateAndBuildLookup(IReadOnlyCollection<PricingRule> pricingRules);
}
