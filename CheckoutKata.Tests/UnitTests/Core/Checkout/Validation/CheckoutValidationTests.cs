using CheckoutKata.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Validation;

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
            () => new global::CheckoutKata.Core.Checkout(Array.Empty<PricingRule>()),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullCheckoutEngine_ThrowsArgumentNullException()
    {
        var rules = new[] { new PricingRule("A", 50) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules, null!),
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
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNullRule_ThrowsArgumentNullException()
    {
        var rules = new PricingRule[] { null! };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules),
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
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveUnitPrice_ThrowsArgumentException(int unitPrice)
    {
        var rules = new[] { new PricingRule("A", unitPrice) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithMissingSpecialPrice_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, 3, null) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithMissingSpecialQuantity_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, null, 130) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Constructor_WithNonPositiveSpecialQuantity_ThrowsArgumentException()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => new global::CheckoutKata.Core.Checkout(new[] { new PricingRule("A", 50, 0, 130) }),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new global::CheckoutKata.Core.Checkout(new[] { new PricingRule("A", 50, 1, 130) }),
                Throws.TypeOf<ArgumentException>());
        });
    }

    [Test]
    public void Constructor_WithNonPositiveSpecialPrice_ThrowsArgumentException()
    {
        var rules = new[] { new PricingRule("A", 50, 3, 0) };

        Assert.That(
            () => new global::CheckoutKata.Core.Checkout(rules),
            Throws.TypeOf<ArgumentException>());
    }

    private static global::CheckoutKata.Core.Checkout CreateCheckout()
    {
        var rules = new[]
        {
            new PricingRule("A", 50, 3, 130),
            new PricingRule("B", 30, 2, 45),
            new PricingRule("C", 20),
            new PricingRule("D", 15)
        };

        return new global::CheckoutKata.Core.Checkout(rules);
    }
}
