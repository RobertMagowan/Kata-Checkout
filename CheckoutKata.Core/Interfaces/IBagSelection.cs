namespace CheckoutKata.Core.Interfaces;

public interface IBagSelection
{
    void SetBagCount(int quantity);

    int GetBagCount();
}
