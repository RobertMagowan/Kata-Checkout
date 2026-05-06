namespace CheckoutKata.Core.Services;

using System;
using System.Collections.Generic;
using Interfaces;

using Models;

public sealed class PricingRuleValidator : IPricingRuleValidator
{
    public void Validate(IReadOnlyCollection<PricingRule> pricingRules)
    {
        ArgumentNullException.ThrowIfNull(pricingRules);

        if (pricingRules.Count == 0)
        {
            throw new ArgumentException("At least one pricing rule is required.", nameof(pricingRules));
        }

        var itemSet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var rule in pricingRules)
        {
            ValidateRule(rule);

            if (!itemSet.Add(rule.Item))
            {
                throw new ArgumentException($"Duplicate pricing rule detected for item '{rule.Item}'.", nameof(pricingRules));
            }
        }
    }

    private static void ValidateRule(PricingRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (string.IsNullOrWhiteSpace(rule.Item)) throw new ArgumentException("Rule item cannot be null, empty, or whitespace.", nameof(rule));

        if (rule.Item.Length != 1 || !char.IsLetter(rule.Item[0]) || !char.IsUpper(rule.Item[0])) throw new ArgumentException("Rule item must be a single uppercase letter.", nameof(rule));
        if (rule.UnitPrice <= 0) throw new ArgumentException("Unit price must be greater than zero.", nameof(rule));
        if (rule.DiscountPolicies is null || rule.DiscountPolicies.Count == 0) return;

        foreach (var policy in rule.DiscountPolicies)
        {
            ArgumentNullException.ThrowIfNull(policy);
        }
    }
}
