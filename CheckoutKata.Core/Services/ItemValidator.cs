namespace CheckoutKata.Core.Services;

using System;
using System.Collections.Generic;
using CheckoutKata.Core.Interfaces;

using Models;

public sealed class ItemValidator : IScannedItemValidator
{
    public string ValidateScannedItem(string item, IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(pricingRulesByItem);

        if (string.IsNullOrWhiteSpace(item))
        {
            throw new ArgumentException("Item cannot be empty or whitespace.", nameof(item));
        }

        if (item.Length != 1)
        {
            throw new ArgumentException("Item must be a single character.", nameof(item));
        }

        if (!char.IsLetter(item[0]) || !char.IsUpper(item[0]))
        {
            throw new ArgumentException("Item must be a single uppercase letter.", nameof(item));
        }

        if (!pricingRulesByItem.ContainsKey(item))
        {
            throw new ArgumentException($"No pricing rule exists for item '{item}'.", nameof(item));
        }

        return item;
    }
}
