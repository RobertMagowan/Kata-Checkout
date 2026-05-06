using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Tests.UnitTests.Application.Models.Pricing;

[Category("Application")]
[Category("Pricing")]
public class PricingVersionIdTests
{
    [Test]
    public void New_ReturnsNonEmptyGuid()
    {
        var pricingVersionId = PricingVersionId.New();

        Assert.That(pricingVersionId.Value, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Parse_RoundTripsGuidValue()
    {
        var value = Guid.NewGuid();

        var pricingVersionId = PricingVersionId.Parse(value);

        Assert.That(pricingVersionId.Value, Is.EqualTo(value));
    }

    [Test]
    public void ToString_ReturnsGuidString()
    {
        var value = Guid.NewGuid();
        var pricingVersionId = PricingVersionId.Parse(value);

        Assert.That(pricingVersionId.ToString(), Is.EqualTo(value.ToString()));
    }
}



