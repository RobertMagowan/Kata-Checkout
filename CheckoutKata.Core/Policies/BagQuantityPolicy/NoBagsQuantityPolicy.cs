namespace CheckoutKata.Core.Policies.BagQuantityPolicy;

using Interfaces;
using Models;

public sealed class NoBagsQuantityPolicy : IBagQuantityPolicy
{
    public static NoBagsQuantityPolicy Instance { get; } = new();

    private NoBagsQuantityPolicy()
    {
    }

    public int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        return 0;
    }
}
