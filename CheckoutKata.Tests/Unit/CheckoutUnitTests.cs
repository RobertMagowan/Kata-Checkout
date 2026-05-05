using CheckoutKata.Core;

namespace CheckoutKata.Tests;

public class CheckoutUnitTests
{
    [TestCase("A", 50)]
    [TestCase("B", 30)]
    [TestCase("C", 20)]
    [TestCase("D", 15)]
    public void GetTotalPrice_WithSingleItem_ReturnsUnitPrice(string item, int expectedTotal)
    {
        var checkout = CreateCheckout();

        checkout.Scan(item);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void GetTotalPrice_WithNoScannedItems_ReturnsZero()
    {
        var checkout = CreateCheckout();

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(0));
    }

    [Test]
    public void GetTotalPrice_WithItemsScannedOutOfOrder_AppliesMatchingOffers()
    {
        var checkout = CreateCheckout();

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
        var checkout = CreateCheckout();

        ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [TestCase("BB", 45)]
    [TestCase("BBBB", 90)]
    public void GetTotalPrice_WithItemBSpecialOffer_AppliesOfferCorrectly(string basket, int expectedTotal)
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void GetTotalPrice_WithMixedBasket_AppliesMultipleSpecialOffers()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "AAABBCCD");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(230));
    }

    [Test]
    public void Clear_WithPreviouslyScannedItems_ResetsTotalAndScannedItems()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "AAABB");
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var scannedItems = checkout.GetScannedItems();

        Assert.That(totalPrice, Is.EqualTo(0));
        Assert.That(scannedItems, Is.Empty);
    }

    [Test]
    public void GetScannedItems_WithScannedItems_ReturnsAggregatedCounts()
    {
        var checkout = CreateCheckout();

        checkout.Scan("B");
        checkout.Scan("A");
        checkout.Scan("B");

        var scannedItems = checkout.GetScannedItems();
        var itemA = scannedItems.Single(item => item.Item == "A");
        var itemB = scannedItems.Single(item => item.Item == "B");

        Assert.Multiple(() =>
        {
            Assert.That(scannedItems.Count, Is.EqualTo(2));
            Assert.That(itemA.Quantity, Is.EqualTo(1));
            Assert.That(itemB.Quantity, Is.EqualTo(2));
        });
    }

    [Test]
    public void GetPricingRules_ReturnsConfiguredPricingRules()
    {
        var checkout = CreateCheckout();

        var pricingRules = checkout.GetPricingRules();

        var itemARule = pricingRules.Single(rule => rule.Item == "A");
        var itemBRule = pricingRules.Single(rule => rule.Item == "B");
        var itemCRule = pricingRules.Single(rule => rule.Item == "C");

        Assert.Multiple(() =>
        {
            Assert.That(pricingRules.Count, Is.EqualTo(4));
            Assert.That(itemARule.UnitPrice, Is.EqualTo(50));
            Assert.That(itemARule.SpecialQuantity, Is.EqualTo(3));
            Assert.That(itemARule.SpecialPrice, Is.EqualTo(130));
            Assert.That(itemBRule.SpecialQuantity, Is.EqualTo(2));
            Assert.That(itemBRule.SpecialPrice, Is.EqualTo(45));
            Assert.That(itemCRule.SpecialQuantity, Is.Null);
            Assert.That(itemCRule.SpecialPrice, Is.Null);
        });
    }

    [Test]
    public void Scan_WithNullItem_ThrowsArgumentNullException()
    {
        var checkout = CreateCheckout();

        Assert.That(
            () => checkout.Scan(null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("AA")]
    [TestCase("a")]
    [TestCase("1")]
    [TestCase("E")]
    public void Scan_WithInvalidItem_ThrowsArgumentException(string item)
    {
        var checkout = CreateCheckout();

        Assert.That(
            () => checkout.Scan(item),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithDuplicateItemRules_ThrowsArgumentException()
    {
        var rules = new[]
        {
            new PricingRule("A", 50),
            new PricingRule("A", 40)
        };

        Assert.That(
            () => new Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        var rules = new[] { new PricingRule("A", unitPrice) };

        Assert.That(
            () => new Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithMissingSpecialPrice_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, 3, null) };

        Assert.That(
            () => new Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithMissingSpecialQuantity_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, null, 130) };

        Assert.That(
            () => new Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNonPositiveSpecialQuantity_ThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => new Checkout(new[] { new PricingRule("A", 50, 0, 130) }),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new Checkout(new[] { new PricingRule("A", 50, 1, 130) }),
                Throws.TypeOf<ArgumentException>());
        });
    }

    [Test]
    public void Constructor_WithNonPositiveSpecialPrice_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, 3, 0) };

        Assert.That(
            () => new Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    private static Checkout CreateCheckout()
    {
        var rules = new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        return new Checkout(rules);
    }

    private static void ScanMany(ICheckout checkout, string basket)
    {
        foreach (var item in basket)
        {
            checkout.Scan(item.ToString());
        }
    }
}
