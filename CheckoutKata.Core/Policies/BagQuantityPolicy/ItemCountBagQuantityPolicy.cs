namespace CheckoutKata.Core.Policies.BagQuantityPolicy;

using Interfaces;
using Models;

public sealed class ItemCountBagQuantityPolicy : IBagQuantityPolicy
{
    private readonly int _itemsPerBag;

    public ItemCountBagQuantityPolicy(int itemsPerBag)
    {
        if (itemsPerBag <= 0) throw new ArgumentException("Items per bag must be greater than zero.", nameof(itemsPerBag));

        _itemsPerBag = itemsPerBag;
    }

    public int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        var itemCount = 0;

        foreach (var scannedItem in scannedItems)
        {
            itemCount = checked(itemCount + scannedItem.Quantity);
        }

        if (itemCount == 0)
        {
            return 0;
        }

        return checked(((itemCount - 1) / _itemsPerBag) + 1);
    }
}
