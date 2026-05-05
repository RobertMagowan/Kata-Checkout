using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Application.Exceptions;

public sealed class UnknownPricingVersionException(PricingVersionId pricingVersionId)
    : Exception($"Unknown pricing version '{pricingVersionId.Value}'.")
{
    public PricingVersionId PricingVersionId { get; } = pricingVersionId;
}
