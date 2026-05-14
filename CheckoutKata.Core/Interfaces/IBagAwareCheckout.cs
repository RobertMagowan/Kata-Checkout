namespace CheckoutKata.Core.Interfaces;

public interface IBagAwareCheckout : ICheckoutSession
{
    int GetTotalBagPrice();

    int GetTotalItemPrice();
}
