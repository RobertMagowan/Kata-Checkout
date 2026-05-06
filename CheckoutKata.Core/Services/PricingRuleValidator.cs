using System;
using System.Collections.Generic;

namespace CheckoutKata.Core;

internal sealed class PricingRuleValidator : IPricingRuleValidator
{
    public IReadOnlyDictionary<string, PricingRule> ValidateAndBuildLookup(IReadOnlyCollection<PricingRule> pricingRules)
    {
        ArgumentNullException.ThrowIfNull(pricingRules);

        var pricingRulesByItem = new Dictionary<string, PricingRule>(StringComparer.Ordinal);

        foreach (var rule in pricingRules)
        {
            ValidateRule(rule);

            if (!pricingRulesByItem.TryAdd(rule.Item, rule))
            {
                throw new ArgumentException(
                    $"Duplicate pricing rule detected for item '{rule.Item}'.",
                    nameof(pricingRules));
            }
        }

        return pricingRulesByItem;
    }

    private static void ValidateRule(PricingRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (string.IsNullOrWhiteSpace(rule.Item))
        {
            throw new ArgumentException("Rule item cannot be null, empty, or whitespace.", nameof(rule));
        }

        if (rule.Item.Length != 1 || !char.IsLetter(rule.Item[0]) || !char.IsUpper(rule.Item[0]))
        {
            throw new ArgumentException("Rule item must be a single uppercase letter.", nameof(rule));
        }

        if (rule.UnitPrice <= 0)
        {
            throw new ArgumentException("Unit price must be greater than zero.", nameof(rule));
        }

        var hasSpecialQuantity = rule.SpecialQuantity.HasValue;
        var hasSpecialPrice = rule.SpecialPrice.HasValue;

        if (hasSpecialQuantity != hasSpecialPrice)
        {
            throw new ArgumentException(
                "Special quantity and special price must both be supplied together.",
                nameof(rule));
        }

        if (hasSpecialQuantity && rule.SpecialQuantity!.Value <= 1)
        {
            throw new ArgumentException("Special quantity must be greater than one.", nameof(rule));
        }

        if (hasSpecialPrice && rule.SpecialPrice!.Value <= 0)
        {
            throw new ArgumentException("Special price must be greater than zero.", nameof(rule));
        }
    }
}
