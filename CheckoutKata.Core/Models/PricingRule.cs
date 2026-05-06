namespace CheckoutKata.Core.Models;

using CheckoutKata.Core.Interfaces;


public sealed record PricingRule(string Item,
                                 int UnitPrice,
                                 IReadOnlyList<IDiscountPolicy>? DiscountPolicies = null);
