namespace CheckoutKata.Console;

using Core.Interfaces;
using Core.Models;
using Core.Policies.BagQuantityPolicy;

internal sealed class CheckoutConsoleApp
{
    private ConsoleCheckoutState _state;

    private CheckoutConsoleApp(ConsoleCheckoutState state)
    {
        _state = state;
    }

    public static CheckoutConsoleApp Create(IReadOnlyCollection<PricingRule> pricingRules)
    {
        return new CheckoutConsoleApp(ConsoleCheckoutFactory.Create(pricingRules, BagPolicyKind.ItemCount));
    }

    public void Run()
    {
        System.Console.WriteLine("Checkout Kata Console");
        System.Console.WriteLine("Type 'help' for available commands.");

        while (true)
        {
            System.Console.Write("> ");
            var input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var command = ConsoleCommand.Parse(input);

            try
            {
                if (!Execute(command))
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or ArgumentNullException or InvalidOperationException or OverflowException)
            {
                System.Console.WriteLine($"Input error: {ex.Message}");
            }
        }
    }

    private bool Execute(ConsoleCommand command)
    {
        switch (command.Name)
        {
            case "scan":
                _state.Checkout.Scan(command.Argument);
                System.Console.WriteLine($"Scanned item '{command.Argument}'.");
                return true;

            case "scanmany":
                ScanMany(command.Argument);
                System.Console.WriteLine($"Scanned basket '{command.Argument}'.");
                return true;

            case "total":
                System.Console.WriteLine($"Total price: {_state.Checkout.GetTotalPrice()}");
                return true;

            case "itemtotal":
                System.Console.WriteLine($"Item total price: {_state.Checkout.GetTotalItemPrice()}");
                return true;

            case "bagtotal":
                System.Console.WriteLine($"Bag total price: {_state.Checkout.GetTotalBagPrice()}");
                return true;

            case "bags":
                SetManualBagQuantity(command.Argument);
                return true;

            case "itemsperbag":
                SetItemsPerBag(command.Argument);
                return true;

            case "bagprice":
            case "bagcost":
                SetBagCost(command.Argument);
                return true;

            case "bagpolicy":
                ChangeBagPolicy(command.Argument);
                return true;

            case "scanned":
                ConsoleOutput.PrintScannedItems(_state.Checkout.GetScannedItems());
                return true;

            case "reset":
                ResetCheckout();
                return true;

            case "rules":
                ConsoleOutput.PrintRules(_state.Checkout.GetPricingRules(), _state);
                return true;

            case "help":
                ConsoleOutput.PrintHelp();
                return true;

            case "exit":
                return false;

            default:
                System.Console.WriteLine($"Unknown command '{command.Name}'. Type 'help' to view commands.");
                return true;
        }
    }

    private void ScanMany(string basket)
    {
        if (string.IsNullOrWhiteSpace(basket))
        {
            throw new ArgumentException("Basket cannot be empty.", nameof(basket));
        }

        foreach (var item in basket)
        {
            _state.Checkout.Scan(item.ToString());
        }
    }

    private void SetManualBagQuantity(string argument)
    {
        if (_state.BagPolicyName != BagPolicyKind.Manual)
        {
            throw new InvalidOperationException("Switch to the manual bag policy before setting bag quantity.");
        }

        var bagQuantity = ConsoleInput.ParseNonNegativeInteger(argument, "Bag quantity");
        var settings = _state.BagSettings with { ManualBagQuantity = bagQuantity };
        var bagPolicy = ConsoleCheckoutFactory.CreateStandardBagPolicy(settings.BagCost, new ManualBagQuantityPolicy(bagQuantity));

        _state = _state.WithCheckout(ConsoleCheckoutFactory.CreateBagAwareCheckout(_state.InnerCheckout, bagPolicy),
                                     BagPolicyKind.Manual,
                                     settings);

        System.Console.WriteLine($"Selected {bagQuantity} checkout bag(s).");
    }

    private void SetItemsPerBag(string argument)
    {
        if (_state.BagPolicyName != BagPolicyKind.ItemCount)
        {
            throw new InvalidOperationException("Switch to the itemcount bag policy before setting items per bag.");
        }

        var itemsPerBag = ConsoleInput.ParsePositiveInteger(argument, "Items per bag");
        var settings = _state.BagSettings with { ItemsPerBag = itemsPerBag };
        var bagPolicy = ConsoleCheckoutFactory.CreateStandardBagPolicy(settings.BagCost, new ItemCountBagQuantityPolicy(itemsPerBag));

        _state = _state.WithCheckout(ConsoleCheckoutFactory.CreateBagAwareCheckout(_state.InnerCheckout, bagPolicy),
                                     BagPolicyKind.ItemCount,
                                     settings);

        System.Console.WriteLine($"Items per bag set to {itemsPerBag}.");
    }

    private void SetBagCost(string argument)
    {
        var bagCost = ConsoleInput.ParsePositiveInteger(argument, "Bag price");
        var settings = _state.BagSettings with { BagCost = bagCost };
        var bagPolicy = ConsoleCheckoutFactory.CreateBagPolicy(_state.BagPolicyName, settings, out var normalizedPolicyName);

        _state = _state.WithCheckout(ConsoleCheckoutFactory.CreateBagAwareCheckout(_state.InnerCheckout, bagPolicy),
                                     normalizedPolicyName,
                                     settings);

        System.Console.WriteLine($"Bag price set to {bagCost}.");
    }

    private void ChangeBagPolicy(string policyName)
    {
        var bagPolicy = ConsoleCheckoutFactory.CreateBagPolicy(policyName, _state.BagSettings, out var normalizedPolicyName);

        _state = _state.WithCheckout(ConsoleCheckoutFactory.CreateBagAwareCheckout(_state.InnerCheckout, bagPolicy),
                                     normalizedPolicyName,
                                     _state.BagSettings);

        System.Console.WriteLine($"Bag policy set to '{_state.BagPolicyName}'.");
    }

    private void ResetCheckout()
    {
        _state.Checkout.Clear();
        System.Console.WriteLine("Checkout reset.");
    }
}
