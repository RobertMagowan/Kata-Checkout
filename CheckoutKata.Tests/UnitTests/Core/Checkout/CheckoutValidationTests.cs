namespace CheckoutKata.Tests.UnitTests.Core.Checkout;

using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Models;
using CheckoutKata.Core.Policies;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Validation")]
public class CheckoutValidationTests
{
    [Test]
    public void Scan_WithNullItem_ThrowsArgumentNullException()
    {
        var checkout = CreateCheckout();

        Assert.That(() => checkout.Scan(null!), Throws.TypeOf<ArgumentNullException>());
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

        Assert.That(() => checkout.Scan(item), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNoRules_ThrowsArgumentException()
    {
        Assert.That(() => CreateCheckout(Array.Empty<PricingRule>()), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullScannedItemValidator_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50) };

        Assert.That(() => new Checkout(rules, null!, new BasketPricer(), new PricingRuleValidator()), 
                    Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBasketPricer_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50) };

        Assert.That(() => new Checkout(rules, new ItemValidator(), null!, new PricingRuleValidator()), 
                    Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithDuplicateItemRules_ThrowsArgumentException()
    {
        var rules = new[]
        {
            new PricingRule("A", 50),
            new PricingRule("A", 40)
        };

        Assert.That(() => CreateCheckout(rules), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullRule_ThrowsArgumentNullException()
    {
        var rules = new PricingRule[] { null! };

        Assert.That(() => CreateCheckout(rules), Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(" ")]
    [TestCase("AA")]
    [TestCase("a")]
    [TestCase("1")]
    public void Constructor_WithInvalidRuleItem_ThrowsArgumentException(string item)
    {
        var rules = new[] { new PricingRule(item, 50) };

        Assert.That(() => CreateCheckout(rules), Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        var rules = new[] { new PricingRule("A", unitPrice) };

        Assert.That(() => CreateCheckout(rules), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void NForXDiscountPolicy_WithInvalidQuantity_ThrowsArgumentException()
    {
        Assert.That(() => new NForXDiscountPolicy(1, 130), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void NForXDiscountPolicy_WithInvalidPrice_ThrowsArgumentException()
    {
        Assert.That(() => new NForXDiscountPolicy(3, 0), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNonPositiveNForXPolicyQuantity_ThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.That(() => CreateCheckout([CreateNForXRule("A", 50, 0, 130)]), Throws.TypeOf<ArgumentException>());
            Assert.That(() => CreateCheckout([CreateNForXRule("A", 50, 1, 130)]), Throws.TypeOf<ArgumentException>());
        });
    }

    [Test]
    public void Constructor_WithNonPositiveNForXPolicyPrice_ThrowsArgumentException()
    {
        Assert.That(() => CreateCheckout([CreateNForXRule("A", 50, 3, 0)]), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullDiscountPolicy_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50, DiscountPolicies: [null!]) };

        Assert.That(() => CreateCheckout(rules), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithInvalidPercentOffDiscountPolicy_ThrowsArgumentException()
    {
        Assert.That(() => new PercentOffDiscountPolicy(0), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void NForXDiscountPolicy_WithoutExplicitType_UsesDefaultType()
    {
        var policy = new NForXDiscountPolicy(3, 130);

        Assert.That(policy.Type, Is.EqualTo("n_for_x"));
    }

    [Test]
    public void PercentOffDiscountPolicy_WithoutExplicitType_UsesDefaultType()
    {
        var policy = new PercentOffDiscountPolicy(20);

        Assert.That(policy.Type, Is.EqualTo("percent_off"));
    }

    [Test]
    public void DiscountPolicies_WithExplicitType_NormalizeTypeValue()
    {
        var nForXPolicy = new NForXDiscountPolicy(3, 130, " N_FOR_X ");
        var percentOffPolicy = new PercentOffDiscountPolicy(20, " Percent_Off ");

        Assert.Multiple(() =>
        {
            Assert.That(nForXPolicy.Type, Is.EqualTo("n_for_x"));
            Assert.That(percentOffPolicy.Type, Is.EqualTo("percent_off"));
        });
    }

    private static Checkout CreateCheckout()
    {
        return CreateCheckout(CreateDefaultRules());
    }

    private static Checkout CreateCheckout(IReadOnlyCollection<PricingRule> pricingRules)
    {
        return new Checkout(pricingRules, new ItemValidator(),
                                                       new BasketPricer(),
                                                       new PricingRuleValidator());
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
}
