# Checkout Kata

A `.NET 10` implementation of the supermarket checkout kata using:
- test-first development (TDD),
- clean code,
- SOLID/SRP-aligned design,
- lightweight clean architecture,
- version-pinned pricing for dual Blazor/API clients.

## Problem Summary

Items are scanned one at a time. Each Item has a unit price, and some Items have special prices (for example, `3 for 130`).  
The checkout must:
- accept Items in any order,
- apply special prices as many times as eligible,
- compute the correct total.

Default rules used in tests:
- Item `A`: 50, special `3 for 130`
- Item `B`: 30, special `2 for 45`
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

- `CheckoutKata.Application`
  - `CheckoutKataDbContext` + EF Core InMemory-backed pricing version storage.
  - `ICheckoutSessionService` cart lifecycle orchestration with:
    - pinned `pricingVersionId`,
    - in-memory runtime cart state,
    - sliding TTL expiry.
  - `ICheckoutSessionMaintenance` for cart eviction and active-cart metrics.
  - Folder layout:
    - `Services/Carts` (checkout session service)
    - `Models/Carts` (session options + cart snapshot)
    - `Models/Pricing` (pricing version models)
    - `Interfaces/Carts` (cart service contracts)
    - `Persistence` (EF Core context + pricing entities/mappers)
    - `Exceptions` (application-specific error types)

- `CheckoutKata.Api`
  - Controller-based REST API for cart creation, scanning, clearing, and snapshots.
  - Boundary normalization (`Trim + ToUpperInvariant`) before domain scan operations.
  - Standardized `ProblemDetails` error responses (`400`, `404`, `409`, `429`).
  - Background cart eviction hosted service (sweep interval configurable via session options).

- `CheckoutKata.Tests`
  - Unit, application, and API integration coverage.

- `CheckoutKata.Console`
  - Interactive REPL for manual verification.

## Build and Test

From repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

Run API:

```powershell
dotnet run --project .\CheckoutKata.Api\CheckoutKata.Api.csproj
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
- Core item format is strictly one uppercase alphabetic character.
- Invalid scan inputs throw exceptions in the Core library.
- Pricing rules are versioned and pinned to cart at creation time.
- New pricing versions affect new carts only; active carts keep pinned version.
- Cart runtime state is in-memory and expires after sliding 30 minutes.
- Failed validation/write operations do not refresh cart TTL.
- Cart clear operation is bodyless and version-agnostic.
- Cart count is capacity-limited to guard in-memory growth.
- Pricing version storage uses EF Core InMemory and is configured via `Checkout:PricingStore:DatabaseName` in API appsettings.

Additional docs:
- `docs/ASSUMPTIONS.md`
- `docs/API-VERSIONING.md`
- `docs/CODING_STYLE.md`

## TDD and Commit Process

The repository history is intentionally granular to show red/green/refactor progression:
- scaffold,
- failing tests,
- minimal implementations,
- validation hardening,
- console harness,
- documentation finalization.
