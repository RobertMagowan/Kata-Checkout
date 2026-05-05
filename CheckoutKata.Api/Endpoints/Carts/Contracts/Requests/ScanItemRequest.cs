namespace CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;

public sealed record ScanItemRequest(
    string Item,
    Guid? PricingVersionId = null);
