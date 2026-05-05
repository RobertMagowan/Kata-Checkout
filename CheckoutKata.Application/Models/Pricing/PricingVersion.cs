using CheckoutKata.Core;

namespace CheckoutKata.Application.Pricing;

public sealed record PricingVersion(
    PricingVersionId Id,
    DateTimeOffset CreatedAtUtc,
    bool IsActive,
    IReadOnlyList<PricingRule> Rules);
