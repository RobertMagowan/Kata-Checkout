namespace CheckoutKata.Core.Interfaces;

public interface IManualBagQuantityPolicy : IBagQuantityPolicy
{
    void SetBagCount(int quantity);
}
