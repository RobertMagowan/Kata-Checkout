using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
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
    public void GetTotalPrice_WithPercentOffPolicy_AppliesPercentDiscount()
    {
        var checkout = CreateCheckout([
                                          CreatePercentOffRule("A", 100, 20)
                                      ]);

        ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(240));
    }

    [Test]
    public void GetTotalPrice_WithMixedItemsUsingDifferentPolicyTypes_AppliesBestPolicyPerItem()
    {
        var checkout = CreateCheckout([
                                          new PricingRule("A", 100, DiscountPolicies: [new NForXDiscountPolicy(3, 250)]),
                                          new PricingRule("B", 50, DiscountPolicies: [new PercentOffDiscountPolicy(20)])
                                      ]);

        ScanMany(checkout, "AAABBB");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(370));
    }

    [Test]
    public void GetTotalPrice_WithNForXAndPercentPolicies_SelectsSingleBestDiscount()
    {
        var checkout = CreateCheckout([
                                          new PricingRule("A", 100, DiscountPolicies: [ 
                                                                                          new NForXDiscountPolicy(3, 250),
                                                                                          new PercentOffDiscountPolicy(20)
                                                                                      ])
                                      ]);

        ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(240));
    }

    [Test]
    public void GetTotalPrice_WithNForXAndPercentPolicies_DoesNotStackDiscounts()
    {
        var checkout = CreateCheckout([
                                          new PricingRule("A", 100, DiscountPolicies: [
                                                                                          new NForXDiscountPolicy(3, 200),
                                                                                          new PercentOffDiscountPolicy(20)
                                                                                      ])
                                      ]);

        ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(200));
    }

    [Test]
    public void GetTotalPrice_WithWorseDiscountPolicy_FallsBackToBasePrice()
    {
        var checkout = CreateCheckout([
                                          new PricingRule("A", 100, DiscountPolicies: [
                                                                                          new NForXDiscountPolicy(3, 320)
                                                                                      ])
                                      ]);

        ScanMany(checkout, "AAA");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(300));
    }

    [TestCase("AAAAAA", 450)]
    [TestCase("AAAAAAAAAA", 700)]
    public void GetTotalPrice_WithMultipleNForXPolicies_SelectsBestDiscount(string basket, int expectedTotal)
    {
        var checkout = CreateCheckout([new PricingRule("A", 100, DiscountPolicies: [
                                                                                       new NForXDiscountPolicy(2, 170),
                                                                                       new NForXDiscountPolicy(3, 240),
                                                                                       new NForXDiscountPolicy(5, 350)
                                                                                   ])
                                      ]);

        ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void GetTotalPrice_WithCustomBasketPricer_UsesInjectedPricingPolicy()
    {
        var basketPricer = new FixedTotalBasketPricer(999);
        var checkout = new CheckoutKata.Core.Checkout.Checkout([new PricingRule("A", 50)], new ItemValidator(), basketPricer, new PricingRuleValidator());

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

        public int CalculateTotalPrice(IReadOnlyCollection<ScannedItemCount> scannedItemCounts,
                                       IReadOnlyDictionary<string, PricingRule> pricingRulesByItem)
        {
            CalculateTotalPriceCallCount++;
            return totalPrice;
        }
    }

    private static CheckoutKata.Core.Checkout.Checkout CreateCheckout()
    {
        return CreateCheckout(CreateDefaultRules());
    }

    private static CheckoutKata.Core.Checkout.Checkout CreateCheckout(IReadOnlyCollection<PricingRule> rules)
    {
        return new CheckoutKata.Core.Checkout.Checkout(rules, new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
    }

    private static IReadOnlyCollection<PricingRule> CreateDefaultRules() =>
    [
        CreateNForXRule("A", 50, 3, 130),
        CreateNForXRule("B", 30, 2, 45),
        new("C", 20),
        new("D", 15)
    ];

    private static PricingRule CreateNForXRule(string item, int unitPrice, int quantity, int price)
    {
        return new PricingRule(item, unitPrice, DiscountPolicies: [new NForXDiscountPolicy(quantity, price)]);
    }

    private static PricingRule CreatePercentOffRule(string item, int unitPrice, int percentage)
    {
        return new PricingRule(item, unitPrice, DiscountPolicies: [new PercentOffDiscountPolicy(percentage)]);
    }

    private static void ScanMany(ICheckout checkout, string basket)
    {
        foreach (var item in basket)
        {
            checkout.Scan(item.ToString());
        }
    }
}



