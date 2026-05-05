using System.Text.Json;
using CheckoutKata.Application.Pricing;
using CheckoutKata.Core;

namespace CheckoutKata.Application.Persistence;

internal static class PricingVersionEntityMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static PricingVersion ToModel(PricingVersionEntity entity)
    {
        var rules = JsonSerializer.Deserialize<PricingRule[]>(
            entity.RulesJson,
            SerializerOptions)
            ?? throw new InvalidOperationException("Pricing rules payload could not be deserialized.");

        return new PricingVersion(
            PricingVersionId.Parse(entity.Id),
            entity.CreatedAtUtc,
            entity.IsActive,
            rules);
    }

    public static PricingVersionEntity ToEntity(PricingVersion pricingVersion)
    {
        var normalizedRules = pricingVersion.Rules
            .OrderBy(rule => rule.Item, StringComparer.Ordinal)
            .ToArray();

        return new PricingVersionEntity
        {
            Id = pricingVersion.Id.Value,
            CreatedAtUtc = pricingVersion.CreatedAtUtc,
            IsActive = pricingVersion.IsActive,
            RulesJson = JsonSerializer.Serialize(normalizedRules, SerializerOptions)
        };
    }
}
