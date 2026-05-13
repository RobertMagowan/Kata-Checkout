namespace CheckoutKata.Core.Checkout;

using Interfaces;
using Models;

public sealed class BagAwareCheckout : IBagAwareCheckout
{
    private readonly ICheckoutSession _inner;
    private readonly IBagPolicy _bagPolicy;
    private readonly IBagQuantityPolicy _bagQuantityPolicy;

    public BagAwareCheckout(ICheckoutSession inner,
                            IBagPolicy bagPolicy,
                            IBagQuantityPolicy bagQuantityPolicy)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(bagPolicy);
        ArgumentNullException.ThrowIfNull(bagQuantityPolicy);

        _inner = inner;
        _bagPolicy = bagPolicy;
        _bagQuantityPolicy = bagQuantityPolicy;
    }

    public void Scan(string item) { _inner.Scan(item); }

    public int GetTotalPrice()
    {
        return checked(GetTotalItemCost() + GetTotalBagCost());
    }

    public void Clear() { _inner.Clear(); }

    public int GetBagCount()
    {
        return _bagQuantityPolicy.CalculateBagCount(GetScannedItems());
    }

    public int GetTotalBagCost()
    {
        return _bagPolicy.CalculatePrice(GetBagCount());
    }

    public int GetTotalItemCost()
    {
        return _inner.GetTotalPrice();
    }

    public IReadOnlyList<ScannedItemCount> GetScannedItems()
    {
        return _inner.GetScannedItems();
    }

    public IReadOnlyList<PricingRule> GetPricingRules()
    {
        return _inner.GetPricingRules();
    }
}
