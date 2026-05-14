namespace CheckoutKata.Console;

internal sealed record BagSettings(int BagCost,
                                   int ItemsPerBag,
                                   int ManualBagQuantity)
{
    public static BagSettings Default { get; } = new(BagPolicyDefaults.BagCost,
                                                     BagPolicyDefaults.ItemsPerBag,
                                                     ManualBagQuantity: 0);
}
