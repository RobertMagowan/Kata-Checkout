namespace CheckoutKata.Application.Carts;

public interface ICheckoutSessionMaintenance
{
    int EvictExpiredCarts();

    int ActiveCartCount { get; }
}
