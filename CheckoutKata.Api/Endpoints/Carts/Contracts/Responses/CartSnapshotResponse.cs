namespace CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;

public sealed record CartSnapshotResponse(
    Guid CartId,
    Guid PricingVersionId,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<ScannedItemResponse> ScannedItems,
    IReadOnlyList<PricingRuleResponse> PricingRules,
    int TotalPrice);
