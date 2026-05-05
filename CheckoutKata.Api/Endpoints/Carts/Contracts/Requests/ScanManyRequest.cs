namespace CheckoutKata.Api.Endpoints.Carts.Contracts.Requests;

public sealed record ScanManyRequest(
    IReadOnlyList<string> Items,
    Guid? PricingVersionId = null);
