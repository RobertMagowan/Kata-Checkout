namespace CheckoutKata.Core.Interfaces;

public interface IBagAwareCheckout : ICheckoutSession, IBagCountReader, ICheckoutCostBreakdown
{
}
