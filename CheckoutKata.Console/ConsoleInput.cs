namespace CheckoutKata.Console;

internal static class ConsoleInput
{
    public static int ParsePositiveInteger(string argument,
                                           string valueName)
    {
        var value = ParseInteger(argument, valueName);

        if (value <= 0)
        {
            throw new ArgumentException($"{valueName} must be greater than zero.", nameof(argument));
        }

        return value;
    }

    public static int ParseNonNegativeInteger(string argument,
                                              string valueName)
    {
        var value = ParseInteger(argument, valueName);

        if (value < 0)
        {
            throw new ArgumentException($"{valueName} cannot be negative.", nameof(argument));
        }

        return value;
    }

    private static int ParseInteger(string argument,
                                    string valueName)
    {
        if (!int.TryParse(argument, out var value))
        {
            throw new ArgumentException($"{valueName} must be a whole number.", nameof(argument));
        }

        return value;
    }
}
