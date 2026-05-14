namespace CheckoutKata.Core.Policies.DiscountPolicy;

using Interfaces;

public sealed class NForXDiscountPolicy : IDiscountPolicy
{
    private readonly int _quantity;
    private readonly int _price;

    public NForXDiscountPolicy(int quantity, int price)
    {
        if (quantity <= 1) throw new ArgumentException("n_for_x policy quantity must be greater than one.");
        if (price <= 0) throw new ArgumentException("n_for_x policy price must be greater than zero.");

        _quantity = quantity;
        _price = price;
    }

    public int CalculatePrice(int itemCount, int unitPrice)
    {
        if (itemCount < 0) throw new ArgumentException("Item count cannot be negative.");
        if (unitPrice <= 0) throw new ArgumentException("Unit price must be greater than zero.");

        var offerApplications = itemCount / _quantity;
        var remainingItems = itemCount % _quantity;

        return checked((offerApplications * _price) + (remainingItems * unitPrice));
    }
}
