namespace CheckoutKata.Core.Interfaces;

public interface ICheckout
{
    void Scan(string item);
    
    int GetTotalPrice();

    void Clear();
}
