namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Interfaces;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies.BagPolicy;
using CheckoutKata.Core.Policies.BagQuantityPolicy;
using CheckoutKata.Core.Policies.DiscountPolicy;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Pricing")]
public class BagQuantityPolicyTests
{
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(10, 1)]
    [TestCase(11, 2)]
    [TestCase(20, 2)]
    [TestCase(21, 3)]
    public void ItemCountBagQuantityPolicy_CalculateBagQuantity_ReturnsOneBagPerStartedItemBlock(int itemCount,
                                                                                                  int expectedBagQuantity)
    {
        var policy = new ItemCountBagQuantityPolicy(10);
        var scannedItems = itemCount == 0
            ? Array.Empty<ScannedItemCount>()
            : [new ScannedItemCount("A", itemCount)];

        var bagQuantity = policy.CalculateBagQuantity(scannedItems);

        Assert.That(bagQuantity, Is.EqualTo(expectedBagQuantity));
    }

    [Test]
    public void VolumeBagQuantityPolicy_CalculateBagQuantity_ReturnsOneBagPerStartedVolumeBlock()
    {
        var policy = new VolumeBagQuantityPolicy(new Dictionary<string, int>(StringComparer.Ordinal)
                                                 {
                                                     ["A"] = 2,
                                                     ["B"] = 5
                                                 },
                                                 bagVolumeCapacity: 10);

        var bagQuantity = policy.CalculateBagQuantity([
                                                          new ScannedItemCount("A", 3),
                                                          new ScannedItemCount("B", 1)
                                                      ]);

        Assert.That(bagQuantity, Is.EqualTo(2));
    }

    [Test]
    public void VolumeBagQuantityPolicy_CalculateBagQuantity_WithNoScannedItems_ReturnsZero()
    {
        var policy = new VolumeBagQuantityPolicy(new Dictionary<string, int>(StringComparer.Ordinal)
                                                 {
                                                     ["A"] = 2
                                                 },
                                                 bagVolumeCapacity: 10);

        var bagQuantity = policy.CalculateBagQuantity([]);

        Assert.That(bagQuantity, Is.EqualTo(0));
    }

    [Test]
    public void BagPolicy_CalculatePrice_ReturnsCostForQuantity()
    {
        IBagPolicy policy = new StandardBagPolicy(10, new ManualBagQuantityPolicy(2));

        var price = policy.CalculatePrice([]);

        Assert.That(price, Is.EqualTo(20));
    }

    [Test]
    public void ManualBagQuantityPolicy_CalculateBagQuantity_ReturnsSelectedQuantity()
    {
        var policy = new ManualBagQuantityPolicy(2);

        var bagQuantity = policy.CalculateBagQuantity([]);

        Assert.That(bagQuantity, Is.EqualTo(2));
    }

    [Test]
    public void NoBagsQuantityPolicy_CalculateBagQuantity_ReturnsZero()
    {
        var bagQuantity = NoBagsQuantityPolicy.Instance.CalculateBagQuantity([new ScannedItemCount("A", 2)]);

        Assert.That(bagQuantity, Is.EqualTo(0));
    }

    [Test]
    public void NoBagsQuantityPolicy_Instance_ReturnsSingleton()
    {
        var instance = NoBagsQuantityPolicy.Instance;
        var secondInstance = NoBagsQuantityPolicy.Instance;

        Assert.That(secondInstance, Is.SameAs(instance));
    }

    [Test]
    public void BagAwareCheckout_GetTotalPrice_AddsBagPriceToItemTotal()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ItemCountBagQuantityPolicy(10)));

        checkout.Scan("A");

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(60));
    }

    [Test]
    public void BagAwareCheckout_Clear_ForwardsToInnerCheckout()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ItemCountBagQuantityPolicy(10)));

        checkout.Scan("A");
        checkout.Clear();

        var scannedItems = checkout.GetScannedItems();

        Assert.That(scannedItems, Is.Empty);
    }

    [Test]
    public void BagAwareCheckout_Clear_SelectsNoBagsQuantityPolicy()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ManualBagQuantityPolicy(2)));

        checkout.Scan("A");
        checkout.Clear();

        var bagTotal = checkout.GetTotalBagPrice();

        Assert.That(bagTotal, Is.EqualTo(0));
    }

    [Test]
    public void BagAwareCheckout_ScanAfterClear_RestoresPreviousBagQuantityPolicy()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ManualBagQuantityPolicy(2)));

        checkout.Scan("A");
        checkout.Clear();
        checkout.Scan("A");

        var bagTotal = checkout.GetTotalBagPrice();

        Assert.That(bagTotal, Is.EqualTo(20));
    }

    [Test]
    public void BagAwareCheckout_RepeatedClear_RestoresPreviousBagQuantityPolicy()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ManualBagQuantityPolicy(2)));

        checkout.Scan("A");
        checkout.Clear();
        checkout.Clear();
        checkout.Scan("A");

        var bagTotal = checkout.GetTotalBagPrice();

        Assert.That(bagTotal, Is.EqualTo(20));
    }

    [Test]
    public void BagAwareCheckout_WithNoBagsPolicy_DoesNotRestoreBagQuantityPolicyOnScan()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, NoBagsQuantityPolicy.Instance));

        checkout.Scan("A");

        var bagTotal = checkout.GetTotalBagPrice();

        Assert.That(bagTotal, Is.EqualTo(0));
    }

    [Test]
    public void BagAwareCheckout_GetPricingRules_ForwardsToInnerCheckout()
    {
        IBagAwareCheckout checkout = new BagAwareCheckout(CreateInnerCheckout(),
                                                          new StandardBagPolicy(10, new ItemCountBagQuantityPolicy(10)));

        var pricingRules = checkout.GetPricingRules();

        Assert.That(pricingRules.Select(rule => rule.Item), Is.EqualTo(new[] { "A", "B", "C", "D" }));
    }

    [Test]
    public void VolumeBagQuantityPolicy_CalculateBagQuantity_WithMissingVolume_ThrowsInvalidOperationException()
    {
        var policy = new VolumeBagQuantityPolicy(new Dictionary<string, int>(StringComparer.Ordinal)
                                                 {
                                                     ["A"] = 2
                                                 },
                                                 bagVolumeCapacity: 10);

        Assert.That(() => policy.CalculateBagQuantity([new ScannedItemCount("B", 1)]),
                    Throws.TypeOf<InvalidOperationException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void ItemCountBagQuantityPolicy_WithNonPositiveItemsPerBag_ThrowsArgumentException(int itemsPerBag)
    {
        Assert.That(() => new ItemCountBagQuantityPolicy(itemsPerBag), Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void VolumeBagQuantityPolicy_WithNonPositiveBagVolumeCapacity_ThrowsArgumentException(int bagVolumeCapacity)
    {
        Assert.That(() => new VolumeBagQuantityPolicy(new Dictionary<string, int>(StringComparer.Ordinal), bagVolumeCapacity),
                    Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void BagPolicy_WithNonPositiveCostPerBag_ThrowsArgumentException(int costPerBag)
    {
        Assert.That(() => new StandardBagPolicy(costPerBag, NoBagsQuantityPolicy.Instance), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void BagPolicy_CalculatePrice_WithNegativeQuantity_ThrowsArgumentException()
    {
        var policy = new StandardBagPolicy(10, new NegativeBagQuantityPolicy());

        Assert.That(() => policy.CalculatePrice([]), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void BagPolicy_WithNullBagQuantityPolicy_ThrowsArgumentNullException()
    {
        Assert.That(() => new StandardBagPolicy(10, null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ManualBagQuantityPolicy_WithNegativeQuantity_ThrowsArgumentException()
    {
        Assert.That(() => new ManualBagQuantityPolicy(-1), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void ManualBagQuantityPolicy_CalculateBagQuantity_WithNullScannedItems_ThrowsArgumentNullException()
    {
        var policy = new ManualBagQuantityPolicy(0);

        Assert.That(() => policy.CalculateBagQuantity(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void NoBagsQuantityPolicy_CalculateBagQuantity_WithNullScannedItems_ThrowsArgumentNullException()
    {
        Assert.That(() => NoBagsQuantityPolicy.Instance.CalculateBagQuantity(null!), Throws.TypeOf<ArgumentNullException>());
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

    private sealed class NegativeBagQuantityPolicy : IBagQuantityPolicy
    {
        public int CalculateBagQuantity(IReadOnlyCollection<ScannedItemCount> scannedItems)
        {
            return -1;
        }
    }
}
