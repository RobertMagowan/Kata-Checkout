using CheckoutKata.Core;

var pricingRules = new[]
{
    new PricingRule("A", 50, 3, 130),
    new PricingRule("B", 30, 2, 45),
    new PricingRule("C", 20),
    new PricingRule("D", 15)
};

ICheckout checkout = new Checkout(pricingRules);

Console.WriteLine("Checkout Kata Console");
Console.WriteLine("Type 'help' for available commands.");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    var commandParts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = commandParts[0].ToLowerInvariant();
    var argument = commandParts.Length > 1 ? commandParts[1].Trim() : string.Empty;

    try
    {
        switch (command)
        {
            case "scan":
                checkout.Scan(argument);
                Console.WriteLine($"Scanned item '{argument}'.");
                break;

            case "scanmany":
                ScanMany(checkout, argument);
                Console.WriteLine($"Scanned basket '{argument}'.");
                break;

            case "total":
                Console.WriteLine($"Total price: {checkout.GetTotalPrice()}");
                break;

            case "reset":
                checkout = new Checkout(pricingRules);
                Console.WriteLine("Checkout reset.");
                break;

            case "rules":
                PrintRules(pricingRules);
                break;

            case "help":
                PrintHelp();
                break;

            case "exit":
                return;

            default:
                Console.WriteLine($"Unknown command '{command}'. Type 'help' to view commands.");
                break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
    {
        Console.WriteLine($"Input error: {ex.Message}");
    }
}

static void ScanMany(ICheckout checkout, string basket)
{
    if (string.IsNullOrWhiteSpace(basket))
    {
        throw new ArgumentException("Basket cannot be empty.", nameof(basket));
    }

    foreach (var item in basket)
    {
        checkout.Scan(item.ToString());
    }
}

static void PrintRules(IEnumerable<PricingRule> rules)
{
    Console.WriteLine("Pricing rules:");

    foreach (var rule in rules)
    {
        if (rule.SpecialQuantity is null || rule.SpecialPrice is null)
        {
            Console.WriteLine($"- Item {rule.Item}: {rule.UnitPrice}");
            continue;
        }

        Console.WriteLine(
            $"- Item {rule.Item}: {rule.UnitPrice} ({rule.SpecialQuantity} for {rule.SpecialPrice})");
    }
}

static void PrintHelp()
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("- scan <ITEM>      : Scan one item (example: scan A)");
    Console.WriteLine("- scanmany <ITEMS> : Scan many items from a basket string (example: scanmany ABBA)");
    Console.WriteLine("- total            : Display the current total price");
    Console.WriteLine("- reset            : Reset current basket");
    Console.WriteLine("- rules            : Show pricing rules");
    Console.WriteLine("- help             : Show help");
    Console.WriteLine("- exit             : Exit the application");
}
