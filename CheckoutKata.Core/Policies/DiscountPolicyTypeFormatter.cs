namespace CheckoutKata.Core.Policies;

internal static class DiscountPolicyTypeFormatter
{
    private const string PolicySuffix = "DiscountPolicy";

    public static string FromPolicyClassName(string className)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        var policyName = className.EndsWith(PolicySuffix, StringComparison.Ordinal)
            ? className[..^PolicySuffix.Length]
            : className;

        return ConvertPascalToSnakeCase(policyName);
    }

    private static string ConvertPascalToSnakeCase(string value)
    {
        if (value.Length == 0)
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder(value.Length + 8);

        for (var index = 0; index < value.Length; index++)
        {
            var currentCharacter = value[index];
            if (char.IsUpper(currentCharacter) && index > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(currentCharacter));
        }

        return builder.ToString();
    }
}

