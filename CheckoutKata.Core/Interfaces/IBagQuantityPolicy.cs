namespace CheckoutKata.Core.Interfaces;

using Models;

public interface IBagQuantityPolicy
{
    int CalculateBagCount(IReadOnlyCollection<ScannedItemCount> scannedItems);
}
