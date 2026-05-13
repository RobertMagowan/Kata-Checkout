namespace CheckoutKata.Core.Interfaces;

public interface ICheckoutCostBreakdown
{
    int GetTotalBagCost();

    int GetTotalItemCost();
}
