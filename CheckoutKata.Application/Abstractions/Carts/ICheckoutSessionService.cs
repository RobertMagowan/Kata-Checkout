using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Application.Carts;

public interface ICheckoutSessionService
{
    Task<CartSnapshot> CreateCartAsync(CancellationToken cancellationToken = default);

    Task<CartSnapshot> GetCartAsync(
        Guid cartId,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default);

    Task<CartSnapshot> ScanItemAsync(
        Guid cartId,
        string item,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default);

    Task<CartSnapshot> ScanManyAsync(
        Guid cartId,
        IReadOnlyCollection<string> items,
        PricingVersionId? requestedPricingVersionId = null,
        CancellationToken cancellationToken = default);

    Task<CartSnapshot> ClearAsync(
        Guid cartId,
        CancellationToken cancellationToken = default);
}
