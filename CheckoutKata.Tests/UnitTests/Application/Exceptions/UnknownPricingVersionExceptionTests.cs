using CheckoutKata.Application.Exceptions;
using CheckoutKata.Application.Pricing;

namespace CheckoutKata.Tests.UnitTests.Application.Exceptions;

[Category("Application")]
[Category("Validation")]
public class UnknownPricingVersionExceptionTests
{
    [Test]
    public void Constructor_SetsPricingVersionIdAndMessage()
    {
        var pricingVersionId = PricingVersionId.New();

        var exception = new UnknownPricingVersionException(pricingVersionId);

        Assert.Multiple(() =>
        {
            Assert.That(exception.PricingVersionId, Is.EqualTo(pricingVersionId));
            Assert.That(exception.Message, Does.Contain(pricingVersionId.Value.ToString()));
        });
    }
}



