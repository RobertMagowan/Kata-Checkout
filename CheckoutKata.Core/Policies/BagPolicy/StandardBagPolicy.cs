namespace CheckoutKata.Core.Policies.BagPolicy;

using Interfaces;
using Models;
using Policies.BagQuantityPolicy;

public sealed class StandardBagPolicy : IBagPolicy
{
    private readonly int _costPerBag;
    private IBagQuantityPolicy _bagQuantityPolicy;
    private IBagQuantityPolicy _bagQuantityPolicyToRestoreOnScan;

    public StandardBagPolicy(int costPerBag,
                             IBagQuantityPolicy bagQuantityPolicy)
    {
        if (costPerBag <= 0) throw new ArgumentException("Cost per bag must be greater than zero.", nameof(costPerBag));
        ArgumentNullException.ThrowIfNull(bagQuantityPolicy);

        _costPerBag = costPerBag;
        _bagQuantityPolicy = bagQuantityPolicy;
        _bagQuantityPolicyToRestoreOnScan = bagQuantityPolicy;
    }

    public int CalculatePrice(IReadOnlyCollection<ScannedItemCount> scannedItems)
    {
        ArgumentNullException.ThrowIfNull(scannedItems);

        var quantity = _bagQuantityPolicy.CalculateBagQuantity(scannedItems);

        if (quantity < 0) throw new ArgumentException("Bag quantity cannot be negative.", nameof(quantity));

        return checked(_costPerBag * quantity);
    }

    public void ClearUntilNextScan()
    {
        if (!IsNoBagsQuantityPolicy(_bagQuantityPolicy))
        {
            _bagQuantityPolicyToRestoreOnScan = _bagQuantityPolicy;
        }

        _bagQuantityPolicy = NoBagsQuantityPolicy.Instance;
    }

    public void RestoreOnScan()
    {
        if (IsNoBagsQuantityPolicy(_bagQuantityPolicy))
        {
            _bagQuantityPolicy = _bagQuantityPolicyToRestoreOnScan;
        }
    }

    private static bool IsNoBagsQuantityPolicy(IBagQuantityPolicy policy)
    {
        return ReferenceEquals(policy, NoBagsQuantityPolicy.Instance);
    }
}
