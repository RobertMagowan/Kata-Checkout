namespace CheckoutKata.Core.Policies.BagQuantityPolicy;

using Interfaces;
using Models;

public sealed class VolumeBagQuantityPolicy : IBagQuantityPolicy
{
    private readonly IReadOnlyDictionary<string, int> _volumeByItem;
    private readonly int _bagVolumeCapacity;

    public VolumeBagQuantityPolicy(IReadOnlyDictionary<string, int> volumeByItem,
                                   int bagVolumeCapacity)
    {
        ArgumentNullException.ThrowIfNull(volumeByItem);
        if (bagVolumeCapacity <= 0) throw new ArgumentException("Bag volume capacity must be greater than zero.", nameof(bagVolumeCapacity));

        _volumeByItem = volumeByItem;
        _bagVolumeCapacity = bagVolumeCapacity;
    }

    public int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        var totalVolume = 0;

        foreach (var scannedItem in scannedItems)
        {
            if (!_volumeByItem.TryGetValue(scannedItem.Item, out var itemVolume))
            {
                throw new InvalidOperationException($"No volume configured for item '{scannedItem.Item}'.");
            }

            totalVolume = checked(totalVolume + checked(itemVolume * scannedItem.Quantity));
        }

        if (totalVolume == 0)
        {
            return 0;
        }

        return checked(((totalVolume - 1) / _bagVolumeCapacity) + 1);
    }
}
