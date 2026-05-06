namespace CheckoutKata.Core.Interfaces;

public interface IDiscountPolicy
{
    string Type { get; }

    int CalculatePrice(int itemCount, int unitPrice);
}
