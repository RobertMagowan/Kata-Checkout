namespace CheckoutKata.Core.Interfaces;

public interface IBagSelectionCheckout : ICheckout, ICheckoutStateReader, IBagSelection, ICheckoutCostBreakdown
{
}
