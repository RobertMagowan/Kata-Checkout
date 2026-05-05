namespace CheckoutKata.Application.Pricing;

public readonly record struct PricingVersionId(Guid Value)
{
    public static PricingVersionId New() => new(Guid.NewGuid());

    public static PricingVersionId Parse(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
