namespace CheckoutKata.Console;

internal readonly record struct ConsoleCommand(string Name,
                                               string Argument)
{
    public static ConsoleCommand Parse(string input)
    {
        var commandParts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = commandParts[0].ToLowerInvariant();
        var argument = commandParts.Length > 1 ? commandParts[1].Trim() : string.Empty;

        return new ConsoleCommand(command, argument);
    }
}
