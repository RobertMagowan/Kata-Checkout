namespace CheckoutKata.Application.Exceptions;

public sealed class CartCapacityExceededException(int maxCarts)
    : Exception($"Cannot create cart because max cart capacity ({maxCarts}) has been reached.")
{
    public int MaxCarts { get; } = maxCarts;
}
