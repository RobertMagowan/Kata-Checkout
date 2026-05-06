using CheckoutKata.Tests.Shared.Core;
using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Pricing;

using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Services;

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

    [Test]
    public void GetTotalPrice_WithPercentOffPolicy_AppliesPercentDiscount()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout(
            new[]
            {
                CheckoutCoreTestData.CreatePercentOffRule("A", 100, 20)
            });

        CheckoutCoreTestData.ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(240));
    }

    [Test]
    public void GetTotalPrice_WithNForXAndPercentPolicies_SelectsSingleBestDiscount()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout(
            new[]
            {
                new PricingRule(
                    "A",
                    100,
                    DiscountPolicies:
                    [
                        new NForXDiscountPolicy(3, 250),
                        new PercentOffDiscountPolicy(20)
                    ])
            });

        CheckoutCoreTestData.ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(240));
    }

    [Test]
    public void GetTotalPrice_WithNForXAndPercentPolicies_DoesNotStackDiscounts()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout(
            new[]
            {
                new PricingRule(
                    "A",
                    100,
                    DiscountPolicies:
                    [
                        new NForXDiscountPolicy(3, 200),
                        new PercentOffDiscountPolicy(20)
                    ])
            });

        CheckoutCoreTestData.ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(200));
    }

    [Test]
    public void GetTotalPrice_WithCustomBasketPricer_UsesInjectedPricingPolicy()
    {
        var basketPricer = new FixedTotalBasketPricer(999);
        var checkout = new global::CheckoutKata.Core.Checkout.Checkout(
                                                                       new[] { new PricingRule("A", 50) },
                                                                       new ItemValidator(),
                                                                       basketPricer,
                                                                       new PricingRuleValidator());

        checkout.Scan("A");

        var totalPrice = checkout.GetTotalPrice();

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(999));
            Assert.That(basketPricer.CalculateTotalPriceCallCount, Is.EqualTo(1));
        });
    }

    private sealed class FixedTotalBasketPricer(int totalPrice) : IBasketPricer
    {
        public int CalculateTotalPriceCallCount { get; private set; }

        public int CalculateTotalPrice(
            IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
            IReadOnlyCollection<PricingRule> pricingRules)
        {
            CalculateTotalPriceCallCount++;
            return totalPrice;
        }
    }
}



