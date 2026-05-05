using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Application.Exceptions;

public sealed class PricingVersionMismatchException(
    Guid cartId,
    PricingVersionId expectedPricingVersionId,
    PricingVersionId requestedPricingVersionId)
    : Exception(
        $"Cart '{cartId}' is pinned to pricing version '{expectedPricingVersionId.Value}' " +
        $"but request used '{requestedPricingVersionId.Value}'.")
{
    public Guid CartId { get; } = cartId;

    public PricingVersionId ExpectedPricingVersionId { get; } = expectedPricingVersionId;

    public PricingVersionId RequestedPricingVersionId { get; } = requestedPricingVersionId;
}
