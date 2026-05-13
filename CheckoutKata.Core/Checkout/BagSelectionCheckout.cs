namespace CheckoutKata.Core.Checkout;

using Interfaces;
using Models;

public sealed class BagSelectionCheckout : IBagSelectionCheckout
{
    private readonly Checkout _inner;
    private readonly IBagPolicy _bagPolicy;
    private int _bagCount;

    public BagSelectionCheckout(Checkout inner,
                                IBagPolicy bagPolicy)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(bagPolicy);

        _inner = inner;
        _bagPolicy = bagPolicy;
    }

    public void Scan(string item) { _inner.Scan(item); }

    public int GetTotalPrice()
    {
        return checked(GetTotalItemCost() + GetTotalBagCost());
    }

    public void Clear()
    {
        _inner.Clear();
        _bagCount = 0;
    }

    public void SetBagCount(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Bag quantity cannot be negative.", nameof(quantity));

        _bagPolicy.CalculatePrice(quantity);
        _bagCount = quantity;
    }

    public int GetBagCount()
    {
        return _bagCount;
    }

    public int GetTotalBagCost()
    {
        return _bagPolicy.CalculatePrice(_bagCount);
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
