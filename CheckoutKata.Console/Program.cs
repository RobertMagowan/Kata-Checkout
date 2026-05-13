using CheckoutKata.Console;
using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
using CheckoutKata.Core.Services;
using CheckoutKata.Core.Interfaces;

const int BagUnitPrice = 10;
const int ItemsPerBag = 10;
IReadOnlyCollection<PricingRule> pricingRules;

try
{
    pricingRules = LoadPricingRules("pricing-rules.json");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to load pricing rules: {ex.Message}");
    return;
}

var checkout = CreateCheckout(pricingRules, BagUnitPrice, ItemsPerBag);

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
                checkout = CreateCheckout(pricingRules, BagUnitPrice, ItemsPerBag);
                Console.WriteLine("Checkout reset.");
                break;

            case "rules":
                PrintRules(pricingRules, BagUnitPrice, ItemsPerBag);
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
    catch (Exception ex) when (ex is ArgumentException or ArgumentNullException or OverflowException)
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

static void PrintRules(IEnumerable<PricingRule> rules,
                       int bagUnitPrice,
                       int itemsPerBag)
{
    Console.WriteLine("Pricing rules:");

    foreach (var rule in rules)
    {
        if (rule.DiscountPolicies is null || rule.DiscountPolicies.Count == 0)
        {
            Console.WriteLine($"- Item {rule.Item}: {rule.UnitPrice}");
            continue;
        }

        var policyDescriptions = rule.DiscountPolicies.Select(DescribePolicy).ToArray();

        Console.WriteLine($"- Item {rule.Item}: {rule.UnitPrice} [{string.Join(", ", policyDescriptions)}]");
    }

    Console.WriteLine($"- Checkout bag: {bagUnitPrice}, automatically one per {itemsPerBag} scanned items");
}

static string DescribePolicy(IDiscountPolicy policy)
{
    ArgumentNullException.ThrowIfNull(policy);

    var policyClassName = policy.GetType().Name;
    var policyName = policyClassName.EndsWith("DiscountPolicy", StringComparison.Ordinal)
        ? policyClassName[..^"DiscountPolicy".Length]
        : policyClassName;

    return ConvertPascalToSnakeCase(policyName);
}

static string ConvertPascalToSnakeCase(string value)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(value);

    var builder = new System.Text.StringBuilder(value.Length + 8);

    for (var index = 0; index < value.Length; index++)
    {
        var character = value[index];
        if (char.IsUpper(character) && index > 0)
        {
            builder.Append('_');
        }

        builder.Append(char.ToLowerInvariant(character));
    }

    return builder.ToString();
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

static IReadOnlyCollection<PricingRule> LoadPricingRules(string relativePath)
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

static IBagAwareCheckout CreateCheckout(IReadOnlyCollection<PricingRule> pricingRules,
                                        int bagUnitPrice,
                                        int itemsPerBag)
{
    var innerCheckout = new Checkout(pricingRules, new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
    return new BagAwareCheckout(innerCheckout, new BagPolicy(bagUnitPrice), new OneBagPerItemCountPolicy(itemsPerBag));
}
