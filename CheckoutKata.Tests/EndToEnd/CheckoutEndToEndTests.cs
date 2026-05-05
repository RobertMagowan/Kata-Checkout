using CheckoutKata.Core;

namespace CheckoutKata.Tests.EndToEnd;

[TestFixture]
public class CheckoutEndToEndTests
{
    [TestCase("", 0)]
    [TestCase("A", 50)]
    [TestCase("AB", 80)]
    [TestCase("CDBA", 115)]
    [TestCase("AA", 100)]
    [TestCase("AAA", 130)]
    [TestCase("AAAA", 180)]
    [TestCase("AAAAA", 230)]
    [TestCase("AAAAAA", 260)]
    [TestCase("AAAB", 160)]
    [TestCase("AAABB", 175)]
    [TestCase("AAABBD", 190)]
    [TestCase("DABABA", 190)]
    public void EndToEnd_BasketTotals_MatchExpectedForConfiguredRules(string basket, int expectedTotal)
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, basket);

        var totalPrice = checkout.GetTotalPrice();

        Assert.That(totalPrice, Is.EqualTo(expectedTotal));
    }

    [Test]
    public void EndToEnd_ScanTotalClearRescan_WorksAcrossFullLifecycle()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "AAABB");
        var firstTotal = checkout.GetTotalPrice();
        checkout.Clear();
        ScanMany(checkout, "DABABA");
        var secondTotal = checkout.GetTotalPrice();

        Assert.Multiple(() =>
        {
            Assert.That(firstTotal, Is.EqualTo(175));
            Assert.That(secondTotal, Is.EqualTo(190));
            Assert.That(checkout.GetScannedItemCounts().Count, Is.EqualTo(3));
        });
    }

    [Test]
    public void EndToEnd_ExposesPricingRulesAndScannedCounts_ForUiAdapters()
    {
        var checkout = CreateCheckout();

        ScanMany(checkout, "BABA");

        var pricingRules = checkout.GetPricingRules();
        var scannedItemCounts = checkout.GetScannedItemCounts();

        Assert.Multiple(() =>
        {
            Assert.That(pricingRules.Count, Is.EqualTo(4));
            Assert.That(pricingRules.Any(rule => rule.Item == "A" && rule.SpecialQuantity == 3 && rule.SpecialPrice == 130), Is.True);
            Assert.That(pricingRules.Any(rule => rule.Item == "B" && rule.SpecialQuantity == 2 && rule.SpecialPrice == 45), Is.True);
            Assert.That(scannedItemCounts["A"], Is.EqualTo(2));
            Assert.That(scannedItemCounts["B"], Is.EqualTo(2));
        });
    }

    private static ICheckout CreateCheckout()
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
