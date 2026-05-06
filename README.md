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
- compute the correct total.

Default console rules (from `CheckoutKata.Console/pricing-rules.json`):
- Item `A`: 50, `n_for_x` (`quantity=3`, `price=130`)
- Item `B`: 30, `n_for_x` (`quantity=2`, `price=45`)
- Item `C`: 20
- Item `D`: 15

## Solution Structure

- `CheckoutKata.Core`
  - `ICheckout` minimal contract (`Scan`, `GetTotalPrice`, `Clear`).
  - `ICheckoutStateReader` typed snapshot reads (`GetScannedItems`, `GetPricingRules`).
  - `IScannedItemValidator` extension point for scan validation policy.
  - `IBasketPricer` extension point for total pricing policy.
  - `Checkout` orchestration + scan state (defaulting to `ItemValidator` + `BasketPricer`).
  - `PricingRule` immutable rule model (`Item` naming only).
  - `BasketPricer` default pricing implementation.
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

## Design Notes

- Monetary values are represented as integers (kata style).
- Core item format is one uppercase alphabetic character (`char.IsLetter` + `char.IsUpper`).
- Invalid scan inputs throw exceptions in the Core library.
- `Checkout` is stateful and non-idempotent (`Scan` increments basket quantity per call).
- Discount policies are evaluated per item and the lowest total for that item is selected (no discount stacking).
- Arithmetic is guarded using `checked` to fail fast on overflow.
- Core does not normalize input casing; callers must supply valid item codes.
- `Clear` is treated as idempotent behavior.
- Mixed baskets can use different discount policy types per item, and each item still selects its own best single policy.

Additional docs:
- `docs/ASSUMPTIONS.md`
- `docs/CODING_STYLE.md`

## TDD and Commit Process

The repository history is intentionally granular to show red/green/refactor progression:
- scaffold,
- failing tests,
- minimal implementations,
- validation hardening,
- console harness,
- documentation finalization.
