namespace CheckoutKata.Core.Policies;

using Interfaces;


public sealed class PercentOffDiscountPolicy : IDiscountPolicy
{
    private readonly int _percentage;

    public PercentOffDiscountPolicy(int percentage, string? type = null)
    {
        if (percentage is <= 0 or > 100) throw new ArgumentException("percent_off policy percentage must be between 1 and 100."); 

        _percentage = percentage;
        Type = string.IsNullOrWhiteSpace(type) ? CreateDefaultType() : NormalizeType(type);
    }

    public string Type { get; }

    public int CalculatePrice(int itemCount, int unitPrice)
    {
        if (itemCount < 0) throw new ArgumentException("Item count cannot be negative.");
        if (unitPrice <= 0) { throw new ArgumentException("Unit price must be greater than zero."); }

        var basePrice = checked(itemCount * unitPrice);
        var discountAmount = checked(basePrice * _percentage / 100);

        return checked(basePrice - discountAmount);
    }

    private static string NormalizeType(string type)
    {
        return type.Trim().ToLowerInvariant();
    }

    private static string CreateDefaultType()
    {
        return DiscountPolicyTypeFormatter.FromPolicyClassName(nameof(PercentOffDiscountPolicy));
    }
}
