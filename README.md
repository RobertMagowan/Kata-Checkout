# Checkout Kata

A `.NET 10` implementation of the supermarket checkout kata using:
- test-first development (TDD),
- clean code,
- SOLID/SRP-aligned design,
- lightweight clean architecture.

## Problem Summary

Items are scanned one at a time. Each Item has a unit price, and some Items have special prices (for example, `3 for 130`).  
The checkout must:
- accept Items in any order,
- apply special prices as many times as eligible,
- compute the correct total.

Default rules used in tests and console app:
- Item `A`: 50, special `3 for 130`
- Item `B`: 30, special `2 for 45`
- Item `C`: 20
- Item `D`: 15

## Solution Structure

- `CheckoutKata.Core`
  - `ICheckout` interface from the kata prompt.
  - `Checkout` orchestration + scan state.
  - `PricingRule` immutable rule model (`Item` naming only).
  - `BasketPricer` pure pricing calculation.
  - `ItemValidator` fail-fast scan input validation.
  - `PricingRuleValidator` constructor-time rule validation.

- `CheckoutKata.Tests`
  - `NUnit` unit tests for totals, specials, mixed baskets, invalid scan inputs, and invalid pricing rules.

- `CheckoutKata.Console`
  - Interactive REPL for manual verification.

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
- Item format is strictly one uppercase alphabetic character.
- Invalid scan inputs throw exceptions in the Core library.
- Rules are injected into `Checkout` so pricing can vary per transaction context.

## TDD and Commit Process

The repository history is intentionally granular to show red/green/refactor progression:
- scaffold,
- failing tests,
- minimal implementations,
- validation hardening,
- console harness,
- documentation finalization.
