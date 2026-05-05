namespace CheckoutKata.Application.Carts;

public sealed class CartSessionOptions
{
    public TimeSpan SlidingTtl { get; init; } = TimeSpan.FromMinutes(30);

    public int MaxCarts { get; init; } = 10_000;

    public TimeSpan SweepInterval { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan? AbsoluteMaxAge { get; init; }
}
