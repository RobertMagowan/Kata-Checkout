namespace CheckoutKata.Core;

public sealed record PricingRule(
    string Item,
    int UnitPrice,
    int? SpecialQuantity = null,
    int? SpecialPrice = null);
