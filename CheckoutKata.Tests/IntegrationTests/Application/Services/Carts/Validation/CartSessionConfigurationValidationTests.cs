using CheckoutKata.Application.Carts;
using CheckoutKata.Tests.Shared.Infrastructure.Persistence;

namespace CheckoutKata.Tests.IntegrationTests.Application.Services.Carts.Validation;

[Category("Application")]
[Category("Validation")]
public class CartSessionConfigurationValidationTests
{
    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveSlidingTtl_ThrowsArgumentException(int minutes)
    {
        var dbContextFactory = TestDbContextFactory.Create();

        Assert.That(
            () => new CheckoutSessionService(
                dbContextFactory,
                TimeProvider.System,
                new CartSessionOptions
                {
                    SlidingTtl = TimeSpan.FromMinutes(minutes),
                    MaxCarts = 10,
                    SweepInterval = TimeSpan.FromMinutes(1)
                }),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveMaxCarts_ThrowsArgumentException(int maxCarts)
    {
        var dbContextFactory = TestDbContextFactory.Create();

        Assert.That(
            () => new CheckoutSessionService(
                dbContextFactory,
                TimeProvider.System,
                new CartSessionOptions
                {
                    SlidingTtl = TimeSpan.FromMinutes(30),
                    MaxCarts = maxCarts,
                    SweepInterval = TimeSpan.FromMinutes(1)
                }),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveSweepInterval_ThrowsArgumentException(int minutes)
    {
        var dbContextFactory = TestDbContextFactory.Create();

        Assert.That(
            () => new CheckoutSessionService(
                dbContextFactory,
                TimeProvider.System,
                new CartSessionOptions
                {
                    SlidingTtl = TimeSpan.FromMinutes(30),
                    MaxCarts = 10,
                    SweepInterval = TimeSpan.FromMinutes(minutes)
                }),
            Throws.TypeOf<ArgumentException>());
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_WithNonPositiveAbsoluteMaxAge_ThrowsArgumentException(int minutes)
    {
        var dbContextFactory = TestDbContextFactory.Create();

        Assert.That(
            () => new CheckoutSessionService(
                dbContextFactory,
                TimeProvider.System,
                new CartSessionOptions
                {
                    SlidingTtl = TimeSpan.FromMinutes(30),
                    MaxCarts = 10,
                    SweepInterval = TimeSpan.FromMinutes(1),
                    AbsoluteMaxAge = TimeSpan.FromMinutes(minutes)
                }),
            Throws.TypeOf<ArgumentException>());
    }
}



