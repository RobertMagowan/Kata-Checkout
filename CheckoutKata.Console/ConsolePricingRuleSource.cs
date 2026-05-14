namespace CheckoutKata.Console;

using Core.Models;

internal static class ConsolePricingRuleSource
{
    public static IReadOnlyCollection<PricingRule> Load(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var absolutePath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Pricing rules file was not found at '{absolutePath}'.", absolutePath);
        }

        var json = File.ReadAllText(absolutePath);

        return PricingRulesJsonDeserializer.Deserialize(json);
    }
}
