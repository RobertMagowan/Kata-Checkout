namespace CheckoutKata.Console;

using Core.Interfaces;

internal sealed record ConsoleCheckoutState(ICheckoutSession InnerCheckout,
                                            IBagAwareCheckout Checkout,
                                            string BagPolicyName,
                                            BagSettings BagSettings)
{
    public ConsoleCheckoutState WithCheckout(IBagAwareCheckout checkout,
                                             string bagPolicyName,
                                             BagSettings bagSettings)
    {
        return this with
        {
            Checkout = checkout,
            BagPolicyName = bagPolicyName,
            BagSettings = bagSettings
        };
    }
}
