namespace CheckoutKata.Core.Interfaces;

public interface IBagPolicy
{
    int CalculatePrice(int quantity);
}
