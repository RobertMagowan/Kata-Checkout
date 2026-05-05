namespace CheckoutKata.Application.Exceptions;

public sealed class CartNotFoundException(Guid cartId)
    : Exception($"Cart '{cartId}' was not found or has expired.")
{
    public Guid CartId { get; } = cartId;
}
