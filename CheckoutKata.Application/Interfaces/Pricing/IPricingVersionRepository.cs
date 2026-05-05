using CheckoutKata.Core;

namespace CheckoutKata.Application.Pricing;

public interface IPricingVersionRepository
{
    Task<PricingVersion> GetLatestVersionAsync(CancellationToken cancellationToken = default);

    Task<PricingVersion?> GetVersionAsync(
        PricingVersionId pricingVersionId,
        CancellationToken cancellationToken = default);

    Task<PricingVersion> CreateVersionAsync(
        IReadOnlyCollection<PricingRule> pricingRules,
        bool setAsActive,
        CancellationToken cancellationToken = default);

    Task SetActiveVersionAsync(
        PricingVersionId pricingVersionId,
        CancellationToken cancellationToken = default);
}
