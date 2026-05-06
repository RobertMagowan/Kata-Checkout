namespace CheckoutKata.Core.Interfaces;

using Models;

public interface IPricingRuleValidator
{
    void Validate(IReadOnlyCollection<PricingRule> pricingRules);
}
