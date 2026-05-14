namespace CheckoutKata.Core.Interfaces;

using Models;

public interface IBagQuantityPolicy
{
    int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems);
}
