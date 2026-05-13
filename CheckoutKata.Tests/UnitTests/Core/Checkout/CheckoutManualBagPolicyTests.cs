namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Pricing")]
public class CheckoutManualBagPolicyTests
{
    [Test]
    public void GetTotalPrice_WithSelectedBags_AddsBagChargeToItemTotal()
    {
        var checkout = CreateCheckout();

        checkout.Checkout.Scan("A");
        checkout.BagQuantityPolicy.SetBagCount(2);

        var totalPrice = checkout.Checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(70));
    }

    [Test]
    public void GetBagCount_WithNoSelectedBags_ReturnsZero()
    {
        var checkout = CreateCheckout();

        var bagCount = checkout.Checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(0));
    }

    [Test]
    public void GetBagCount_WithSelectedBags_ReturnsSelectedQuantity()
    {
        var checkout = CreateCheckout();

        checkout.BagQuantityPolicy.SetBagCount(2);

        var bagCount = checkout.Checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(2));
    }

    [Test]
    public void GetTotalBagCost_WithSelectedBags_ReturnsBagChargeOnly()
    {
        var checkout = CreateCheckout();

        checkout.Checkout.Scan("A");
        checkout.BagQuantityPolicy.SetBagCount(2);

        var totalBagCost = checkout.Checkout.GetTotalBagCost();

        Assert.That(totalBagCost, Is.EqualTo(20));
    }

    [Test]
    public void GetTotalItemCost_WithSelectedBags_ReturnsItemTotalOnly()
    {
        var checkout = CreateCheckout();

        checkout.Checkout.Scan("A");
        checkout.BagQuantityPolicy.SetBagCount(2);

        var totalItemCost = checkout.Checkout.GetTotalItemCost();

        Assert.That(totalItemCost, Is.EqualTo(50));
    }

    [Test]
    public void GetTotalPrice_WithSelectedBagsAndItemOffers_AddsBagChargeAfterDiscounts()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout.Checkout, "AAABB");
        checkout.BagQuantityPolicy.SetBagCount(3);

        var totalPrice = checkout.Checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(205));
    }

    [Test]
    public void GetTotalBagCost_WithCustomBagPolicy_UsesInjectedPolicy()
    {
        var innerCheckout = CreateInnerCheckout();
        var bagQuantityPolicy = new ManualBagQuantityPolicy();
        IBagAwareCheckout checkout = new BagAwareCheckout(innerCheckout, new FixedBagPolicy(25), bagQuantityPolicy);

        bagQuantityPolicy.SetBagCount(2);

        var totalBagCost = checkout.GetTotalBagCost();

        Assert.That(totalBagCost, Is.EqualTo(50));
    }

    [Test]
    public void GetBagCount_WithCustomManualBagQuantityPolicy_UsesInjectedQuantityPolicy()
    {
        var innerCheckout = CreateInnerCheckout();
        IBagAwareCheckout checkout = new BagAwareCheckout(innerCheckout, new BagPolicy(10), new FixedManualBagQuantityPolicy(4));

        var bagCount = checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(4));
    }

    [Test]
    public void Clear_WithSelectedBags_ResetsItemsAndKeepsManualBagSelection()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout.Checkout, "AB");
        checkout.BagQuantityPolicy.SetBagCount(2);
        checkout.Checkout.Clear();

        var totalPrice = checkout.Checkout.GetTotalPrice();
        var bagCount = checkout.Checkout.GetBagCount();
        var totalBagCost = checkout.Checkout.GetTotalBagCost();
        var totalItemCost = checkout.Checkout.GetTotalItemCost();
        var scannedItems = checkout.Checkout.GetScannedItems();

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(20));
            Assert.That(bagCount, Is.EqualTo(2));
            Assert.That(totalBagCost, Is.EqualTo(20));
            Assert.That(totalItemCost, Is.EqualTo(0));
            Assert.That(scannedItems, Is.Empty);
        });
    }

    [Test]
    public void ManualBagQuantityPolicy_SetBagCount_WithNegativeQuantity_ThrowsArgumentException()
    {
        IManualBagQuantityPolicy policy = new ManualBagQuantityPolicy();

        Assert.That(() => policy.SetBagCount(-1), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ManualBagQuantityPolicy_CalculateBagCount_WithNullScannedItems_ThrowsArgumentNullException()
    {
        IManualBagQuantityPolicy policy = new ManualBagQuantityPolicy();

        Assert.That(() => policy.CalculateBagCount(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void BagPolicyCalculatePrice_WithNegativeQuantity_ThrowsArgumentException()
    {
        IBagPolicy policy = new BagPolicy(10);

        Assert.That(() => policy.CalculatePrice(-1), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void GetScannedItems_ForwardsToInnerCheckout()
    {
        IBagAwareCheckout checkout = CreateCheckout().Checkout;
        ICheckoutStateReader stateReader = checkout;

        checkout.Scan("B");
        checkout.Scan("B");

        var scannedItems = stateReader.GetScannedItems();

        Assert.That(scannedItems, Is.EqualTo(new[] { new ScannedItemCount("B", 2) }));
    }

    [Test]
    public void GetPricingRules_ForwardsToInnerCheckout()
    {
        ICheckoutStateReader checkout = CreateCheckout().Checkout;

        var pricingRules = checkout.GetPricingRules();

        Assert.That(pricingRules.Select(rule => rule.Item), Is.EqualTo(new[] { "A", "B", "C", "D" }));
    }

    [Test]
    public void Constructor_WithNullInnerCheckout_ThrowsArgumentNullException()
    {
        Assert.That(() => new BagAwareCheckout(null!, new BagPolicy(10), new ManualBagQuantityPolicy()), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBagPolicy_ThrowsArgumentNullException()
    {
        var innerCheckout = CreateInnerCheckout();

        Assert.That(() => new BagAwareCheckout(innerCheckout, null!, new ManualBagQuantityPolicy()), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullManualBagQuantityPolicy_ThrowsArgumentNullException()
    {
        var innerCheckout = CreateInnerCheckout();

        Assert.That(() => new BagAwareCheckout(innerCheckout, new BagPolicy(10), null!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void BagPolicy_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        Assert.That(() => new BagPolicy(unitPrice), Throws.TypeOf<ArgumentException>());
    }

    private static ManualBagCheckout CreateCheckout()
    {
        var bagQuantityPolicy = new ManualBagQuantityPolicy();
        var checkout = new BagAwareCheckout(CreateInnerCheckout(), new BagPolicy(10), bagQuantityPolicy);

        return new ManualBagCheckout(checkout, bagQuantityPolicy);
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

    private sealed record ManualBagCheckout(IBagAwareCheckout Checkout,
                                            IManualBagQuantityPolicy BagQuantityPolicy);

    private sealed class FixedBagPolicy(int unitPrice) : IBagPolicy
    {
        public int CalculatePrice(int quantity)
        {
            return checked(quantity * unitPrice);
        }
    }

    private sealed class FixedManualBagQuantityPolicy(int bagCount) : IManualBagQuantityPolicy
    {
        public void SetBagCount(int quantity) { }

        public int CalculateBagCount(IReadOnlyCollection<ScannedItemCount> scannedItems)
        {
            return bagCount;
        }

    }
}
