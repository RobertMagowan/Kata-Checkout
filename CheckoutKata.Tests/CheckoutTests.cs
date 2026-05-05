using CheckoutKata.Core;

namespace CheckoutKata.Tests;

public class CheckoutTests
{
    [Test]
    public void GetTotalPrice_WithNoScannedItems_ReturnsZero()
    {
        var rules = new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        ICheckout checkout = new Checkout(rules);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(0));
    }
}
