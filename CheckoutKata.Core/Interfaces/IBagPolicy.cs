namespace CheckoutKata.Core.Interfaces;

using Models;

public interface IBagPolicy
{
    int CalculatePrice(IReadOnlyCollection<ScannedItemCount> scannedItems);

    void ClearUntilNextScan();

    void RestoreOnScan();
}
