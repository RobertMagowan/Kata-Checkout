namespace CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;

public sealed record PricingRuleResponse(
    string Item,
    int UnitPrice,
    int? SpecialQuantity,
    int? SpecialPrice);
