namespace CheckoutKata.Console;

using Core.Checkout;
using Core.Interfaces;
using Core.Models;
using Core.Policies.BagPolicy;
using Core.Policies.BagQuantityPolicy;
using Core.Services;

internal static class ConsoleCheckoutFactory
{
    public static ConsoleCheckoutState Create(IReadOnlyCollection<PricingRule> pricingRules,
                                              string bagPolicyName)
    {
        var innerCheckout = new Checkout(pricingRules,
                                         new ItemValidator(),
                                         new BasketPricer(),
                                         new PricingRuleValidator());

        var bagSettings = BagSettings.Default;
        var bagPolicy = CreateBagPolicy(bagPolicyName, bagSettings, out var normalizedPolicyName);
        var checkout = CreateBagAwareCheckout(innerCheckout, bagPolicy);

        return new ConsoleCheckoutState(innerCheckout, checkout, normalizedPolicyName, bagSettings);
    }

    public static IBagAwareCheckout CreateBagAwareCheckout(ICheckoutSession checkout,
                                                           IBagPolicy bagPolicy)
    {
        return new BagAwareCheckout(checkout, bagPolicy);
    }

    public static IBagPolicy CreateBagPolicy(string policyName,
                                             BagSettings settings,
                                             out string normalizedPolicyName)
    {
        var bagQuantityPolicy = CreateBagQuantityPolicy(policyName, settings, out normalizedPolicyName);

        return CreateStandardBagPolicy(settings.BagCost, bagQuantityPolicy);
    }

    public static IBagPolicy CreateStandardBagPolicy(int bagCost,
                                                     IBagQuantityPolicy bagQuantityPolicy)
    {
        return new StandardBagPolicy(bagCost, bagQuantityPolicy);
    }

    private static IBagQuantityPolicy CreateBagQuantityPolicy(string policyName,
                                                              BagSettings settings,
                                                              out string normalizedPolicyName)
    {
        normalizedPolicyName = policyName.Trim().ToLowerInvariant();

        return normalizedPolicyName switch
        {
            BagPolicyKind.ItemCount => new ItemCountBagQuantityPolicy(settings.ItemsPerBag),
            BagPolicyKind.Volume => new VolumeBagQuantityPolicy(BagPolicyDefaults.VolumeByItem,
                                                                BagPolicyDefaults.BagVolumeCapacity),
            BagPolicyKind.Manual => new ManualBagQuantityPolicy(settings.ManualBagQuantity),
            BagPolicyKind.None => NoBagsQuantityPolicy.Instance,
            _ => throw new ArgumentException("Bag policy must be one of: itemcount, volume, manual, none.", nameof(policyName))
        };
    }
}
