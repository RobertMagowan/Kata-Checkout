namespace CheckoutKata.Core.Policies;

using Interfaces;
using Models;

public sealed class ManualBagQuantityPolicy : IManualBagQuantityPolicy
{
    private int _bagCount;

    public void SetBagCount(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Bag quantity cannot be negative.", nameof(quantity));

        _bagCount = quantity;
    }

    public int CalculateBagCount(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        return _bagCount;
    }

}
