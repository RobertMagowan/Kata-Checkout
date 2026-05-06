namespace CheckoutKata.Core.Interfaces;

using Models;

public interface IPricingRuleValidator
{
    IReadOnlyDictionary<string, PricingRule> ValidateAndBuildLookup(IReadOnlyCollection<PricingRule> pricingRules);
}
