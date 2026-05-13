namespace CheckoutKata.Core.Policies;

using Interfaces;

public sealed class BagPolicy : IBagPolicy
{
    private readonly int _unitPrice;

    public BagPolicy(int unitPrice)
    {
        if (unitPrice <= 0) throw new ArgumentException("Bag unit price must be greater than zero.", nameof(unitPrice));

        _unitPrice = unitPrice;
    }

    public int CalculatePrice(int quantity)
    {
        if (quantity < 0) throw new ArgumentException("Bag quantity cannot be negative.", nameof(quantity));

        return checked(quantity * _unitPrice);
    }
}
