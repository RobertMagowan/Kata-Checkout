using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckoutKata.Core;

public sealed class ItemValidator : IScannedItemValidator
{
    public string ValidateScannedItem(
        string item,
        IReadOnlyCollection<PricingRule> pricingRules)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(pricingRules);

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

        if (!pricingRules.Any(rule => rule.Item == item))
        {
            throw new ArgumentException($"No pricing rule exists for item '{item}'.", nameof(item));
        }

        return item;
    }
}
