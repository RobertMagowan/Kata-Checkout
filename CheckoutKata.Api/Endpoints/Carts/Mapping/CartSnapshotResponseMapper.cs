using CheckoutKata.Application.Carts;
using CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;

namespace CheckoutKata.Api.Endpoints.Carts.Mapping;

internal static class CartSnapshotResponseMapper
{
    public static CartSnapshotResponse ToResponse(CartSnapshot snapshot) =>
        new(
            snapshot.CartId,
            snapshot.PricingVersionId.Value,
            snapshot.ExpiresAtUtc,
            snapshot.ScannedItems
                .Select(item => new ScannedItemResponse(item.Item, item.Quantity))
                .ToArray(),
            snapshot.PricingRules
                .Select(rule => new PricingRuleResponse(
                    rule.Item,
                    rule.UnitPrice,
                    rule.SpecialQuantity,
                    rule.SpecialPrice))
                .ToArray(),
            snapshot.TotalPrice);
}
