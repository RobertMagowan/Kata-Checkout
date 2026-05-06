using CheckoutKata.Tests.Shared.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Pricing;

[Category("Core")]
[Category("Pricing")]
public class CheckoutPricingTests
{
    [TestCase("A", 50)]
    [TestCase("B", 30)]
    [TestCase("C", 20)]
    [TestCase("D", 15)]
    public void GetTotalPrice_WithSingleItem_ReturnsUnitPrice(string item, int expectedTotal)
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        checkout.Scan(item);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void GetTotalPrice_WithNoScannedItems_ReturnsZero()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(0));
    }

    [Test]
    public void GetTotalPrice_WithItemsScannedOutOfOrder_AppliesMatchingOffers()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        checkout.Scan("B");
        checkout.Scan("A");
        checkout.Scan("B");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(95));
    }

    [TestCase("AAA", 130)]
    [TestCase("AAAAAA", 260)]
    public void GetTotalPrice_WithItemASpecialOffer_AppliesOfferCorrectly(string basket, int expectedTotal)
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        CheckoutCoreTestData.ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [TestCase("BB", 45)]
    [TestCase("BBBB", 90)]
    public void GetTotalPrice_WithItemBSpecialOffer_AppliesOfferCorrectly(string basket, int expectedTotal)
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        CheckoutCoreTestData.ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void GetTotalPrice_WithMixedBasket_AppliesMultipleSpecialOffers()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        CheckoutCoreTestData.ScanMany(checkout, "AAABBCCD");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(230));
    }
}



