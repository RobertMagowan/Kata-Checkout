namespace CheckoutKata.Console;

using System.Text.Json;
using Core.Interfaces;
using Core.Models;
using Core.Policies;

internal static class PricingRulesJsonDeserializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyDictionary<string, Func<DiscountPolicyDto, IDiscountPolicy>> DiscountPolicyFactories =
        new Dictionary<string, Func<DiscountPolicyDto, IDiscountPolicy>>(StringComparer.Ordinal)
        {
            ["n_for_x"] = policy => new NForXDiscountPolicy(GetRequiredParameter(policy, QuantityParameterName),
                                                            GetRequiredParameter(policy, PriceParameterName)),

            ["percent_off"] = policy => new PercentOffDiscountPolicy(GetRequiredParameter(policy, PercentageParameterName))
        };

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
        if (DiscountPolicyFactories.TryGetValue(normalizedType, out var policyFactory))
        {
            return policyFactory(policy);
        }

        throw new ArgumentException($"Unsupported discount policy type '{policy.Type}'.", nameof(policy));
    }

    private static int GetRequiredParameter(DiscountPolicyDto policy, string parameterName)
    {
        var parameters = policy.Parameters;

        if (parameters is null || parameters.Count == 0)
        {
            throw new ArgumentException($"Policy '{policy.Type}' must define parameters.", nameof(policy));
        }

        if (!parameters.TryGetValue(parameterName, out var value))
        {
            throw new ArgumentException($"Policy '{policy.Type}' is missing required parameter '{parameterName}'.", nameof(policy));
        }

        return value;
    }

    private sealed record PricingRulesCatalogDto(IReadOnlyList<PricingRuleDto> Rules);

    private sealed record PricingRuleDto(string Item, int UnitPrice, IReadOnlyList<DiscountPolicyDto>? DiscountPolicies);

    private sealed record DiscountPolicyDto(string Type, Dictionary<string, int>? Parameters);
}
