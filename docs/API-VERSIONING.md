# API Pricing Versioning

## Why Versioned Pricing

Checkout totals must remain stable during an active checkout session even if offers change in the background.

## Lifecycle

1. Client creates cart: `POST /api/carts`.
2. Service resolves the latest active pricing version from EF Core InMemory pricing store.
3. Cart is created with pinned `pricingVersionId`.
4. All subsequent cart operations use pinned pricing version.
5. Cart expires after 30 minutes of inactivity (sliding TTL).
6. Expired carts are removed by periodic background sweep and on-demand checks.

## Version Mismatch Behavior

- Write/read calls may include a client `pricingVersionId`.
- If provided and it differs from cart's pinned version, API returns `409 Conflict`.
- Response is `ProblemDetails` with expected and requested version ids.
- `clear` is intentionally version-agnostic: `POST /api/carts/{cartId}/clear` (no request body required).

## Rule Updates During Active Checkout

- Creating and activating new pricing versions affects **new carts only**.
- Existing carts continue using pinned version until expiry.

## Error Semantics

- `400 Bad Request`: invalid input.
- `404 Not Found`: unknown cart, expired cart, or unknown pricing version lookup.
- `409 Conflict`: pricing version mismatch with pinned cart version.
- `429 Too Many Requests`: cart capacity reached.

## Cart Activity Semantics

- Successful read/write operations refresh sliding TTL.
- Failed validation/write operations do **not** refresh cart TTL.
