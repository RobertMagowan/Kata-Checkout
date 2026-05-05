# Assumptions

1. Pricing and offer rules are repository-backed and versioned.
2. Cart state is runtime/in-memory in this increment and is not persisted.
3. Pricing version identifiers are GUID-based.
4. A cart pins pricing version at creation time and keeps it until expiry.
5. Cart expiry uses a 30-minute sliding TTL.
6. Core checkout remains stateful and non-idempotent (`Scan` increments per call).
7. Core validation remains strict; normalization occurs at API boundary.
8. No auth and no idempotency-key dedupe are included in this increment.
9. New carts use the latest active pricing version.
10. Active carts are not auto-migrated when pricing versions change.
11. Version mismatch between request and pinned cart version returns `409 Conflict`.
12. `POST /api/carts/{cartId}/clear` is version-agnostic and bodyless.
13. Failed validation/write operations do not refresh cart sliding TTL.
14. Cart capacity is bounded (`MaxCarts`) and create requests fail with `429` when capacity is exhausted.
15. Expired carts are evicted by background sweep and on-demand checks.
