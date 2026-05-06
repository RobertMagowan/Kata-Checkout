namespace CheckoutKata.Console;

using System.Text.Json;
using Core.Interfaces;
using Core.Models;
using Core.Policies;

internal static class PricingRulesJsonDeserializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private const string NForXPolicyType = "n_for_x";
    private const string PercentOffPolicyType = "percent_off";
    private const string QuantityParameterName = "quantity";
    private const string PriceParameterName = "price";
    private const string PercentageParameterName = "percentage";

    public static IReadOnlyCollection<PricingRule> Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var catalog = DeserializeCatalog(json);

        if (catalog.Rules is null || catalog.Rules.Count == 0)
        {
            throw new ArgumentException("Pricing rules JSON must contain at least one rule.", nameof(json));
        }

        return catalog.Rules.Select(MapRule).ToArray();
    }

    private static PricingRulesCatalogDto DeserializeCatalog(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<PricingRulesCatalogDto>(json, SerializerOptions)
                ?? throw new ArgumentException("Pricing rules JSON could not be deserialized.", nameof(json));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Pricing rules JSON is invalid.", nameof(json), exception);
        }
    }

    private static PricingRule MapRule(PricingRuleDto rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var discountPolicies = MapDiscountPolicies(rule.DiscountPolicies);

        return new PricingRule(rule.Item, rule.UnitPrice, discountPolicies);
    }

    private static IReadOnlyList<IDiscountPolicy>? MapDiscountPolicies(IReadOnlyList<DiscountPolicyDto>? policies)
    {
        if (policies is null || policies.Count == 0)
        {
            return null;
        }

        return policies.Select(MapDiscountPolicy).ToArray();
    }

    private static IDiscountPolicy MapDiscountPolicy(DiscountPolicyDto policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentException.ThrowIfNullOrWhiteSpace(policy.Type);

        var normalizedType = policy.Type.Trim().ToLowerInvariant();
        if (normalizedType == NForXPolicyType)
        {
            return new NForXDiscountPolicy(GetRequiredParameter(policy.Quantity, policy, QuantityParameterName),
                                           GetRequiredParameter(policy.Price, policy, PriceParameterName));
        }

        if (normalizedType == PercentOffPolicyType)
        {
            return new PercentOffDiscountPolicy(GetRequiredParameter(policy.Percentage, policy, PercentageParameterName));
        }

        throw new ArgumentException($"Unsupported discount policy type '{policy.Type}'.");
    }

    private static int GetRequiredParameter(int? value, DiscountPolicyDto policy, string parameterName)
    {
        if (!value.HasValue) throw new ArgumentException($"Policy '{policy.Type}' is missing required parameter '{parameterName}'.");

        return value.Value;
    }

    private sealed record PricingRulesCatalogDto(IReadOnlyList<PricingRuleDto> Rules);

    private sealed record PricingRuleDto(string Item, int UnitPrice, IReadOnlyList<DiscountPolicyDto>? DiscountPolicies);

    private sealed record DiscountPolicyDto(string Type, int? Quantity, int? Price, int? Percentage);
}
