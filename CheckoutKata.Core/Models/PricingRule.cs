namespace CheckoutKata.Core.Models;

using Interfaces;


public sealed record PricingRule(string Item,
                                 int UnitPrice,
                                 IReadOnlyList<IDiscountPolicy>? DiscountPolicies = null);
