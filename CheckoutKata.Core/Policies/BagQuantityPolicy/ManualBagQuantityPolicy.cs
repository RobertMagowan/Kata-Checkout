namespace CheckoutKata.Core.Policies.BagQuantityPolicy;

using Interfaces;
using Models;

public sealed class ManualBagQuantityPolicy : IBagQuantityPolicy
{
    private readonly int _bagQuantity;

    public ManualBagQuantityPolicy(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Bag quantity cannot be negative.", nameof(quantity));

        _bagQuantity = quantity;
    }

    public int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        return _bagQuantity;
    }
}
