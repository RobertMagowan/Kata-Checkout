namespace CheckoutKata.Core.Checkout;

using Interfaces;
using Models;

public sealed class BagAwareCheckout(ICheckoutSession checkoutSession,
                                     IBagPolicy bagPolicy) : IBagAwareCheckout
{
    public void Scan(string item)
    {
        checkoutSession.Scan(item);
        bagPolicy.RestoreOnScan();
    }

    public int GetTotalPrice()
    {
        return checked(GetTotalItemPrice() + GetTotalBagPrice());
    }

    public void Clear()
    {
        checkoutSession.Clear();
        bagPolicy.ClearUntilNextScan();
    }

    public IReadOnlyList<ScannedItemCount> GetScannedItems()
    {
        return checkoutSession.GetScannedItems();
    }

    public IReadOnlyList<PricingRule> GetPricingRules()
    {
        return checkoutSession.GetPricingRules();
    }

    public int GetTotalBagPrice()
    {
        return bagPolicy.CalculatePrice(GetScannedItems());
    }

    public int GetTotalItemPrice()
    {
        return checkoutSession.GetTotalPrice();
    }
}
