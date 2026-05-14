namespace CheckoutKata.Console;

using Core.Interfaces;
using Core.Models;

internal static class ConsoleOutput
{
    public static void PrintRules(IEnumerable<PricingRule> rules,
                                  ConsoleCheckoutState state)
    {
        System.Console.WriteLine("Pricing rules:");

        foreach (var rule in rules)
        {
            if (rule.DiscountPolicies is null || rule.DiscountPolicies.Count == 0)
            {
                System.Console.WriteLine($"- Item {rule.Item}: {rule.UnitPrice}");
                continue;
            }

            var policyDescriptions = rule.DiscountPolicies.Select(DescribePolicy).ToArray();

            System.Console.WriteLine($"- Item {rule.Item}: {rule.UnitPrice} [{string.Join(", ", policyDescriptions)}]");
        }

        System.Console.WriteLine($"- Bag cost: {state.BagSettings.BagCost}");
        System.Console.WriteLine($"- Bag policy: {state.BagPolicyName}");

        if (state.BagPolicyName == BagPolicyKind.ItemCount)
        {
            System.Console.WriteLine($"- Items per bag: {state.BagSettings.ItemsPerBag}");
        }

        if (state.BagPolicyName == BagPolicyKind.Manual)
        {
            System.Console.WriteLine($"- Selected bags: {state.BagSettings.ManualBagQuantity}");
        }
    }

    public static void PrintScannedItems(IReadOnlyList<ScannedItemCount> scannedItems)
    {
        if (scannedItems.Count == 0)
        {
            System.Console.WriteLine("No scanned items.");
            return;
        }

        System.Console.WriteLine("Scanned items:");

        foreach (var scannedItem in scannedItems)
        {
            System.Console.WriteLine($"- Item {scannedItem.Item}: {scannedItem.Quantity}");
        }
    }

    public static void PrintHelp()
    {
        System.Console.WriteLine("Available commands:");
        System.Console.WriteLine("- scan <ITEM>             : Scan one item (example: scan A)");
        System.Console.WriteLine("- scanmany <ITEMS>        : Scan many items from a basket string (example: scanmany ABBA)");
        System.Console.WriteLine("- bagpolicy <POLICY>      : Select bag policy: itemcount, volume, manual, none");
        System.Console.WriteLine("- bags <COUNT>            : Set manual bag quantity when policy is manual");
        System.Console.WriteLine("- itemsperbag <COUNT>     : Set item-count policy capacity when policy is itemcount");
        System.Console.WriteLine("- bagprice <PRICE>        : Set the price of one bag");
        System.Console.WriteLine("- total                   : Display item plus bag total price");
        System.Console.WriteLine("- itemtotal               : Display item total price");
        System.Console.WriteLine("- bagtotal                : Display bag total price");
        System.Console.WriteLine("- scanned                 : Show scanned item counts");
        System.Console.WriteLine("- reset                   : Reset current basket");
        System.Console.WriteLine("- rules                   : Show pricing and active bag rules");
        System.Console.WriteLine("- help                    : Show help");
        System.Console.WriteLine("- exit                    : Exit the application");
    }

    private static string DescribePolicy(IDiscountPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var policyClassName = policy.GetType().Name;
        var policyName = policyClassName.EndsWith("DiscountPolicy", StringComparison.Ordinal)
            ? policyClassName[..^"DiscountPolicy".Length]
            : policyClassName;

        return ConvertPascalToSnakeCase(policyName);
    }

    private static string ConvertPascalToSnakeCase(string value)
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
}
