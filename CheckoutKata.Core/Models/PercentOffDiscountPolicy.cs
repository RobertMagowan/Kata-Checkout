namespace CheckoutKata.Core.Models;

using CheckoutKata.Core.Interfaces;


public sealed class PercentOffDiscountPolicy : IDiscountPolicy
{
    public const string PolicyType = "percent_off";

    public PercentOffDiscountPolicy(int percentage)
    {
        if (percentage <= 0 || percentage > 100)
        {
            throw new ArgumentException("percent_off policy percentage must be between 1 and 100.", nameof(percentage));
        }

        Percentage = percentage;
    }

    public int Percentage { get; }

    public string Type { get => PolicyType; }

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

        var basePrice = checked(itemCount * unitPrice);
        var discountAmount = checked(basePrice * Percentage / 100);
        return checked(basePrice - discountAmount);
    }
}
