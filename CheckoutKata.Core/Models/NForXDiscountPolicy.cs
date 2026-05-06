namespace CheckoutKata.Core.Models;

using CheckoutKata.Core.Interfaces;


public sealed class NForXDiscountPolicy : IDiscountPolicy
{
    public const string PolicyType = "n_for_x";

    public NForXDiscountPolicy(int quantity, int price)
    {
        if (quantity <= 1)
        {
            throw new ArgumentException("n_for_x policy quantity must be greater than one.", nameof(quantity));
        }

        if (price <= 0)
        {
            throw new ArgumentException("n_for_x policy price must be greater than zero.", nameof(price));
        }

        Quantity = quantity;
        Price = price;
    }

    public int Quantity { get; }

    public int Price { get; }

    public string Type => PolicyType;

    public int CalculatePrice(int itemCount, int unitPrice)
    {
        if (itemCount < 0)
        {
            throw new ArgumentException("Item count cannot be negative.", nameof(itemCount));
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentException("Unit price must be greater than zero.", nameof(unitPrice));
        }

        var offerApplications = itemCount / Quantity;
        var remainingItems = itemCount % Quantity;

        return checked((offerApplications * Price) + (remainingItems * unitPrice));
    }
}
