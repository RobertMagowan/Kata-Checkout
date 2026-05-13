namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Pricing")]
public class CheckoutAutomaticBagPolicyTests
{
    [TestCase("", 0)]
    [TestCase("A", 1)]
    [TestCase("AAAAAAAAAA", 1)]
    [TestCase("AAAAAAAAAAA", 2)]
    [TestCase("AAAAAAAAAAAAAAAAAAAA", 2)]
    [TestCase("AAAAAAAAAAAAAAAAAAAAA", 3)]
    public void GetBagCount_WithOneBagPerTenItemsPolicy_CalculatesBagCountFromScannedItems(string basket,
                                                                                           int expectedBagCount)
    {
        IBagAwareCheckout checkout = CreateCheckout();

        ScanMany(checkout, basket);

        var bagCount = checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(expectedBagCount));
    }

    [Test]
    public void GetTotalPrice_WithAutomaticBags_AddsBagChargeToItemTotal()
    {
        IBagAwareCheckout checkout = CreateCheckout();

        checkout.Scan("A");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(60));
    }

    [Test]
    public void GetTotalPrice_WithAutomaticBagsAndItemOffers_AddsBagChargeAfterDiscounts()
    {
        IBagAwareCheckout checkout = CreateCheckout();

        ScanMany(checkout, "AAABB");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(185));
    }

    [Test]
    public void Clear_WithAutomaticBags_ResetsItemsAndBagCount()
    {
        IBagAwareCheckout checkout = CreateCheckout();

        ScanMany(checkout, "AB");
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var bagCount = checkout.GetBagCount();
        var scannedItems = checkout.GetScannedItems();

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(0));
            Assert.That(bagCount, Is.EqualTo(0));
            Assert.That(scannedItems, Is.Empty);
        });
    }

    [Test]
    public void GetTotalBagCost_WithCustomBagQuantityPolicy_UsesInjectedQuantityPolicy()
    {
        var innerCheckout = CreateInnerCheckout();
        IBagAwareCheckout checkout = new BagAwareCheckout(innerCheckout, new BagPolicy(10), new FixedBagQuantityPolicy(4));

        checkout.Scan("A");

        var totalBagCost = checkout.GetTotalBagCost();

        Assert.That(totalBagCost, Is.EqualTo(40));
    }

    [Test]
    public void GetPricingRules_ForwardsToInnerCheckout()
    {
        ICheckoutStateReader checkout = CreateCheckout();

        var pricingRules = checkout.GetPricingRules();

        Assert.That(pricingRules.Select(rule => rule.Item), Is.EqualTo(new[] { "A", "B", "C", "D" }));
    }

    [Test]
    public void Constructor_WithNullInnerCheckout_ThrowsArgumentNullException()
    {
        Assert.That(() => new BagAwareCheckout(null!, new BagPolicy(10), new OneBagPerItemCountPolicy(10)), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBagPolicy_ThrowsArgumentNullException()
    {
        var innerCheckout = CreateInnerCheckout();

        Assert.That(() => new BagAwareCheckout(innerCheckout, null!, new OneBagPerItemCountPolicy(10)), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBagQuantityPolicy_ThrowsArgumentNullException()
    {
        var innerCheckout = CreateInnerCheckout();

        Assert.That(() => new BagAwareCheckout(innerCheckout, new BagPolicy(10), null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void OneBagPerItemCountPolicy_WithNullScannedItems_ThrowsArgumentNullException()
    {
        var policy = new OneBagPerItemCountPolicy(10);

        Assert.That(() => policy.CalculateBagCount(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void OneBagPerItemCountPolicy_WithNonPositiveItemsPerBag_ThrowsArgumentException(int itemsPerBag)
    {
        Assert.That(() => new OneBagPerItemCountPolicy(itemsPerBag), Throws.TypeOf<ArgumentException>());
    }

    private static IBagAwareCheckout CreateCheckout()
    {
        return new BagAwareCheckout(CreateInnerCheckout(), new BagPolicy(10), new OneBagPerItemCountPolicy(10));
    }

    private static ICheckoutSession CreateInnerCheckout()
    {
        return new Checkout(CreateDefaultRules(), new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
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

    private static void ScanMany(ICheckout checkout, string basket)
    {
        foreach (var item in basket)
        {
            checkout.Scan(item.ToString());
        }
    }

    private sealed class FixedBagQuantityPolicy(int bagCount) : IBagQuantityPolicy
    {
        public int CalculateBagCount(IReadOnlyCollection<ScannedItemCount> scannedItems)
        {
            return bagCount;
        }

    }
}
