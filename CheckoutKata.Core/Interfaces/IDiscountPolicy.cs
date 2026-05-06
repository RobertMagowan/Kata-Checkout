namespace CheckoutKata.Core.Interfaces;

public interface IDiscountPolicy
{
    int CalculatePrice(int itemCount, int unitPrice);
}
