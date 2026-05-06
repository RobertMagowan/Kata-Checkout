using CheckoutKata.Tests.Shared.Core;

namespace CheckoutKata.Tests.UnitTests.Core.Checkout.Lifecycle;

[Category("Core")]
[Category("Lifecycle")]
public class CheckoutLifecycleTests
{
    [Test]
    public void Clear_WithPreviouslyScannedItems_ResetsTotalAndScannedItems()
    {
        var checkout = CheckoutCoreTestData.CreateCheckout();

        CheckoutCoreTestData.ScanMany(checkout, "AAABB");
        checkout.Clear();

        var totalPrice = checkout.GetTotalPrice();
        var scannedItems = checkout.GetScannedItems();

        Assert.That(totalPrice, Is.EqualTo(0));
        Assert.That(scannedItems, Is.Empty);
    }
}



