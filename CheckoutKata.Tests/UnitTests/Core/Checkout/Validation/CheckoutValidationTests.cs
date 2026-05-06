using CheckoutKata.Core;
using CheckoutKata.Tests.Shared.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Validation;

using CheckoutKata.Core.Models;
using CheckoutKata.Core.Services;

[Category("Core")]
[Category("Validation")]
public class CheckoutValidationTests
{
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
    public void Constructor_WithNoRules_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateCheckout(Array.Empty<PricingRule>()),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullScannedItemValidator_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout.Checkout(
                                                                   rules,
                                                                   null!,
                                                                   new BasketPricer(),
                                                                   new PricingRuleValidator()),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithNullBasketPricer_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout.Checkout(
                                                                   rules,
                                                                   new ItemValidator(),
                                                                   null!,
                                                                   new PricingRuleValidator()),
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

        Assert.That(
            () => CreateCheckout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullRule_ThrowsArgumentNullException()
    {
        var rules = new PricingRule[] { null! };

        Assert.That(
            () => CreateCheckout(rules),
            Throws.TypeOf<ArgumentNullException>());
    }

    [TestCase(" ")]
    [TestCase("AA")]
    [TestCase("a")]
    [TestCase("1")]
    public void Constructor_WithInvalidRuleItem_ThrowsArgumentException(string item)
    {
        var rules = new[] { new PricingRule(item, 50) };

        Assert.That(
            () => CreateCheckout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        var rules = new[] { new PricingRule("A", unitPrice) };

        Assert.That(
            () => CreateCheckout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void NForXDiscountPolicy_WithInvalidQuantity_ThrowsArgumentException()
    {
        Assert.That(
            () => new NForXDiscountPolicy(1, 130),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void NForXDiscountPolicy_WithInvalidPrice_ThrowsArgumentException()
    {
        Assert.That(
            () => new NForXDiscountPolicy(3, 0),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNonPositiveNForXPolicyQuantity_ThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => CreateCheckout(
                    [
                        CheckoutCoreTestData.CreateNForXRule("A", 50, 0, 130)
                    ]),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => CreateCheckout(
                    [
                        CheckoutCoreTestData.CreateNForXRule("A", 50, 1, 130)
                    ]),
                Throws.TypeOf<ArgumentException>());
        });
    }

    [Test]
    public void Constructor_WithNonPositiveNForXPolicyPrice_ThrowsArgumentException()
    {
        Assert.That(
            () => CreateCheckout(
                [
                    CheckoutCoreTestData.CreateNForXRule("A", 50, 3, 0)
                ]),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullDiscountPolicy_ThrowsArgumentNullException()
    {
        var rules = new[]
        {
            new PricingRule(
                "A",
                50,
                DiscountPolicies:
                [
                    null!
                ])
        };

        Assert.That(
            () => CreateCheckout(rules),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Constructor_WithInvalidPercentOffDiscountPolicy_ThrowsArgumentException()
    {
        Assert.That(
            () => new PercentOffDiscountPolicy(0),
            Throws.TypeOf<ArgumentException>());
    }

    private static global::CheckoutKata.Core.Checkout.Checkout CreateCheckout()
    {
        return CreateCheckout(CheckoutCoreTestData.CreateDefaultRules());
    }

    private static global::CheckoutKata.Core.Checkout.Checkout CreateCheckout(IReadOnlyCollection<PricingRule> pricingRules)
    {
        return new global::CheckoutKata.Core.Checkout.Checkout(
                                                               pricingRules,
                                                               new ItemValidator(),
                                                               new BasketPricer(),
                                                               new PricingRuleValidator());
    }
}
