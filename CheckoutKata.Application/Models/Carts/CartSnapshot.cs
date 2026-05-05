using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;

namespace CheckoutKata.Application.Carts;

public sealed record CartSnapshot(
    Guid CartId,
    PricingVersionId PricingVersionId,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<ScannedItemCount> ScannedItems,
    IReadOnlyList<PricingRule> PricingRules,
    int TotalPrice);
