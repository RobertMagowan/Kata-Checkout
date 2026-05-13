namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Pricing")]
public class CheckoutBagSelectionTests
{
    [Test]
    public void GetTotalPrice_WithSelectedBags_AddsBagChargeToItemTotal()
    {
        IBagSelectionCheckout checkout = CreateCheckout();

        checkout.Scan("A");
        checkout.SetBagCount(2);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(70));
    }

    [Test]
    public void GetTotalPrice_WhenReferencedByRoleInterfaces_AddsBagChargeToItemTotal()
    {
        var bagSelectionCheckout = CreateConcreteCheckout();
        ICheckout checkout = bagSelectionCheckout;
        IBagSelection bagSelection = bagSelectionCheckout;

        checkout.Scan("A");
        bagSelection.SetBagCount(2);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(70));
    }

    [Test]
    public void GetTotalPrice_WhenReferencedByCompositeInterface_AddsBagChargeToItemTotal()
    {
        IBagSelectionCheckout checkout = CreateCheckout();

        checkout.Scan("A");
        checkout.SetBagCount(2);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(70));
    }

    [Test]
    public void GetBagCount_WithNoSelectedBags_ReturnsZero()
    {
        IBagSelection checkout = CreateCheckout();

        var bagCount = checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(0));
    }

    [Test]
    public void GetBagCount_WithSelectedBags_ReturnsSelectedQuantity()
    {
        IBagSelection checkout = CreateCheckout();

        checkout.SetBagCount(2);

        var bagCount = checkout.GetBagCount();

        Assert.That(bagCount, Is.EqualTo(2));
    }

    [Test]
    public void GetTotalBagCost_WithSelectedBags_ReturnsBagChargeOnly()
    {
        IBagSelectionCheckout checkout = CreateCheckout();
        ICheckoutCostBreakdown costBreakdown = checkout;

        checkout.Scan("A");
        checkout.SetBagCount(2);

        var totalBagCost = costBreakdown.GetTotalBagCost();

        Assert.That(totalBagCost, Is.EqualTo(20));
    }

    [Test]
    public void GetTotalItemCost_WithSelectedBags_ReturnsItemTotalOnly()
    {
        IBagSelectionCheckout checkout = CreateCheckout();
        ICheckoutCostBreakdown costBreakdown = checkout;

        checkout.Scan("A");
        checkout.SetBagCount(2);

        var totalItemCost = costBreakdown.GetTotalItemCost();

        Assert.That(totalItemCost, Is.EqualTo(50));
    }

    [Test]
    public void CheckoutCostBreakdown_WhenReferencedByInterface_ReturnsItemAndBagTotals()
    {
        var checkout = CreateCheckout();
        ICheckoutCostBreakdown costBreakdown = checkout;

        checkout.Scan("A");
        checkout.SetBagCount(2);

        var totalBagCost = costBreakdown.GetTotalBagCost();
        var totalItemCost = costBreakdown.GetTotalItemCost();

        Assert.Multiple(() =>
        {
            Assert.That(totalBagCost, Is.EqualTo(20));
            Assert.That(totalItemCost, Is.EqualTo(50));
        });
    }

    [Test]
    public void GetTotalPrice_WithSelectedBagsAndItemOffers_AddsBagChargeAfterDiscounts()
    {
        IBagSelectionCheckout checkout = CreateCheckout();

        ScanMany(checkout, "AAABB");
        checkout.SetBagCount(3);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(205));
    }

    [Test]
    public void GetTotalBagCost_WithCustomBagPolicy_UsesInjectedPolicy()
    {
        var innerCheckout = CreateInnerCheckout();
        IBagSelectionCheckout checkout = new BagSelectionCheckout(innerCheckout, new FixedBagPolicy(25));

        checkout.SetBagCount(2);

        var totalBagCost = checkout.GetTotalBagCost();

        Assert.That(totalBagCost, Is.EqualTo(50));
    }

    [Test]
    public void Clear_WithSelectedBags_ResetsItemsAndBags()
    {
        IBagSelectionCheckout checkout = CreateCheckout();
        ICheckoutCostBreakdown costBreakdown = checkout;

        ScanMany(checkout, "AB");
        checkout.SetBagCount(2);
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var bagCount = checkout.GetBagCount();
        var totalBagCost = costBreakdown.GetTotalBagCost();
        var totalItemCost = costBreakdown.GetTotalItemCost();
        var scannedItems = checkout.GetScannedItems();

        Assert.Multiple(() =>
        {
            Assert.That(totalPrice, Is.EqualTo(0));
            Assert.That(bagCount, Is.EqualTo(0));
            Assert.That(totalBagCost, Is.EqualTo(0));
            Assert.That(totalItemCost, Is.EqualTo(0));
            Assert.That(scannedItems, Is.Empty);
        });
    }

    [Test]
    public void SetBagCount_WithNegativeQuantity_ThrowsArgumentException()
    {
        IBagSelection checkout = CreateCheckout();

        Assert.That(() => checkout.SetBagCount(-1), Throws.TypeOf<ArgumentException>());
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
        IBagSelectionCheckout checkout = CreateCheckout();
        ICheckoutStateReader stateReader = checkout;

        checkout.Scan("B");
        checkout.Scan("B");

        var scannedItems = stateReader.GetScannedItems();

        Assert.That(scannedItems, Is.EqualTo(new[] { new ScannedItemCount("B", 2) }));
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
        Assert.That(() => new BagSelectionCheckout(null!, new BagPolicy(10)), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBagPolicy_ThrowsArgumentNullException()
    {
        var innerCheckout = CreateInnerCheckout();

        Assert.That(() => new BagSelectionCheckout(innerCheckout, null!), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void BagPolicy_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        Assert.That(() => new BagPolicy(unitPrice), Throws.TypeOf<ArgumentException>());
    }

    private static IBagSelectionCheckout CreateCheckout()
    {
        return CreateConcreteCheckout();
    }

    private static BagSelectionCheckout CreateConcreteCheckout()
    {
        var innerCheckout = CreateInnerCheckout();

        return new BagSelectionCheckout(innerCheckout, new BagPolicy(10));
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

    private sealed class FixedBagPolicy(int unitPrice) : IBagPolicy
    {
        public int CalculatePrice(int quantity)
        {
            return checked(quantity * unitPrice);
        }
    }
}
