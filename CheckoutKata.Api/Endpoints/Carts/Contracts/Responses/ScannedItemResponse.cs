namespace CheckoutKata.Api.Endpoints.Carts.Contracts.Responses;

public sealed record ScannedItemResponse(
    string Item,
    int Quantity);
