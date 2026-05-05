namespace CheckoutKata.Application.Persistence;

public sealed class PricingVersionEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }

    public string RulesJson { get; set; } = string.Empty;
}
