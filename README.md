# Checkout Kata

A `.NET 10` implementation of the supermarket checkout kata using:
- test-first development (TDD),
- clean code,
- SOLID/SRP-aligned design,
- lightweight clean architecture.

## Problem Summary

Items are scanned one at a time. Each Item has a unit price, and some Items have discount policies (for example, `3 for 130`).  
The checkout must:
- accept Items in any order,
- apply discount policies correctly,
- include checkout bag charges in the total,
- compute the correct total.

Default console rules (from `CheckoutKata.Console/pricing-rules.json`):
- Item `A`: 50, `n_for_x` (`quantity=3`, `price=130`)
- Item `B`: 30, `n_for_x` (`quantity=2`, `price=45`)
- Item `C`: 20
- Item `D`: 15
- Checkout bags: 10 each, automatically calculated as one bag per 10 scanned items in the console app

## Solution Structure

- `CheckoutKata.Core`
  - `ICheckout` minimal contract (`Scan`, `GetTotalPrice`, `Clear`).
  - `ICheckoutStateReader` typed snapshot reads (`GetScannedItems`, `GetPricingRules`).
  - `ICheckoutSession` convenience composite contract for clients that need both checkout commands and state reads.
  - `IBagCountReader` checkout bag count read contract.
  - `ICheckoutCostBreakdown` typed item/bag cost reads.
  - `IBagAwareCheckout` convenience composite contract for clients that need the full automatically calculated bag checkout surface.
  - `IScannedItemValidator` extension point for scan validation policy.
  - `IBasketPricer` extension point for total pricing policy.
  - `IBagPolicy` extension point for checkout bag charge policy.
  - `IBagQuantityPolicy` extension point for checkout bag quantity calculation.
  - `IManualBagQuantityPolicy` extension point for manually selected checkout bag quantity.
  - `Checkout` orchestration + scan state (defaulting to `ItemValidator` + `BasketPricer`).
  - `PricingRule` immutable rule model (`Item` naming only).
  - `BagPolicy` default checkout bag charge implementation.
  - `ManualBagQuantityPolicy` manually selected checkout bag quantity implementation.
  - `OneBagPerItemCountPolicy` checkout bag quantity implementation.
  - `BasketPricer` default pricing implementation.
  - `BagAwareCheckout` wrapper that decorates `ICheckoutSession`, calculates bag count from an injected quantity policy, exposes item/bag totals, and forwards item checkout behavior.
  - `ItemValidator` default scan input validation implementation.
  - `PricingRuleValidator` constructor-time rule validation.
  - Policy implementations:
    - `NForXDiscountPolicy`
    - `PercentOffDiscountPolicy`

- `CheckoutKata.Console`
  - Interactive REPL for manual verification.
  - JSON rule deserialization for data-driven pricing policy configuration.

- `CheckoutKata.Tests`
  - NUnit unit tests for `CheckoutKata.Core` behaviors.
  - Coverlet coverage configuration focused on `CheckoutKata.Core`.

## Build and Test

From repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

## Run Console App

```powershell
dotnet run --project .\CheckoutKata.Console\CheckoutKata.Console.csproj
```

Commands:
- `scan <ITEM>`
- `scanmany <ITEMS>`
- `total`
- `reset`
- `rules`
- `help`
- `exit`

## JSON pricing file format

The console app can load pricing rules from a JSON file. The JSON must match the structure expected by `PricingRulesJsonDeserializer`.

Top-level object:
- `rules`: array of pricing rule objects (required, at least one)

Per-rule object fields:
- `item`: single uppercase letter (e.g. "A")
- `unitPrice`: integer monetary value (> 0)
- `discountPolicies`: optional array of policy objects

Policy object fields:
- `type`: string, supported values: `"n_for_x"`, `"percent_off"`
- For `"n_for_x"`: required `quantity` (int) and `price` (int)
- For `"percent_off"`: required `percentage` (int)

Example `pricing-rules.json`:

```
{
  "rules": [
    { "item": "A", "unitPrice": 50, "discountPolicies": [ { "type": "n_for_x", "quantity": 3, "price": 130 } ] },
    { "item": "B", "unitPrice": 30, "discountPolicies": [ { "type": "n_for_x", "quantity": 2, "price": 45 } ] },
    { "item": "C", "unitPrice": 20 },
    { "item": "D", "unitPrice": 15, "discountPolicies": [ { "type": "percent_off", "percentage": 20 } ] }
  ]
}
```

Validation notes:
- The deserializer throws `ArgumentException` for invalid JSON or missing required fields.
- `PricingRuleValidator` enforces rule constraints at `Checkout` construction (duplicate items, invalid item format, non-positive unit price).

## Minimal client example

The following is a minimal in-code example showing how a consumer would use the core Checkout component:

```csharp
using System.IO;
using CheckoutKata.Console; // PricingRulesJsonDeserializer
using CheckoutKata.Core.Checkout;
using CheckoutKata.Core.Services;

// load pricing rules from JSON file
var json = File.ReadAllText("pricing-rules.json");
var pricingRules = PricingRulesJsonDeserializer.Deserialize(json);

var checkout = new Checkout(pricingRules, new ItemValidator(), new BasketPricer(), new PricingRuleValidator());
checkout.Scan("A");
checkout.Scan("A");
checkout.Scan("B");
var total = checkout.GetTotalPrice(); // returns int monetary value
checkout.Clear();
```

## Design Notes

- Monetary values are represented as integers (kata style).
- Core item format is one uppercase alphabetic character (`char.IsLetter` + `char.IsUpper`).
- Invalid scan inputs throw exceptions in the Core library.
- `Checkout` is stateful and non-idempotent (`Scan` increments basket quantity per call).
- Checkout bag charges are separate from scanned item SKUs and are not eligible for item discount policies.
- `BagAwareCheckout` calculates bag count through `IBagQuantityPolicy` and item/bag cost reads through `ICheckoutCostBreakdown`.
- Manual bag selection is represented by `ManualBagQuantityPolicy`; automatic bag calculation is represented by `OneBagPerItemCountPolicy`.
- `BagAwareCheckout.Clear` clears scanned items only; bag quantity lifecycle behavior belongs to the injected quantity policy.
- The console app hardcodes checkout bags at 10 monetary units each and calculates one bag per 10 scanned items.
- Discount policies are evaluated per item and the lowest total for that item is selected (no discount stacking).
- Arithmetic is guarded using `checked` to fail fast on overflow.
- Core does not normalize input casing; callers must supply valid item codes.
- `Clear` is treated as idempotent behavior.
- Mixed baskets can use different discount policy types per item, and each item still selects its own best single policy.

Additional docs:
- `docs/ASSUMPTIONS.md`
- `docs/CODING_STYLE.md`

