# Assumptions

The following assumptions reflect the current repository state (`CheckoutKata.Core`, `CheckoutKata.Console`, `CheckoutKata.Tests`).

1. Solution scope is core library + console adapter + unit tests only (no API/Application projects).
2. Monetary values are integer units (no decimal currency modeling in this kata implementation).
3. Core checkout is stateful and non-idempotent (`Scan` adds one item each call).
4. Core item validation is strict and throws on invalid inputs.
5. Item format in core is a single uppercase alphabetic character (`char.IsLetter` + `char.IsUpper`).
6. Core does not perform boundary normalization (trim/uppercase); callers must pass valid item values.
7. Pricing rules are validated at checkout construction and then used for the checkout lifetime.
8. Duplicate item pricing rules are rejected.
9. Unit price must be greater than zero.
10. `NForXDiscountPolicy` requires quantity > 1 and price > 0.
11. `PercentOffDiscountPolicy` requires percentage in range 1..100.
12. When multiple discount policies are present for an item, only one policy is applied: the one producing the lowest item total.
13. Discount policies are not stacked.
14. If a configured discount policy would produce a worse price than base unit pricing, base pricing is retained.
15. Arithmetic is overflow-checked (`checked`) and may throw `OverflowException`.
16. `Clear` behavior is idempotent (calling it multiple times keeps basket empty and total at zero).
17. Checkout bag charges are separate from scanned item SKUs and are not eligible for item discount policies.
18. Manual bag selection is represented by `ManualBagQuantityPolicy`, which implements `IManualBagQuantityPolicy`.
19. `Checkout` exposes its full command/read surface through `ICheckoutSession`.
20. `BagAwareCheckout` exposes bag count through `IBagCountReader`, item/bag cost reads through `ICheckoutCostBreakdown`, and the full surface through `IBagAwareCheckout`.
21. `BagAwareCheckout` decorates `ICheckoutSession` rather than depending on the concrete `Checkout` type.
22. `BagAwareCheckout.Clear` clears scanned items only; bag quantity lifecycle behavior belongs to the injected quantity policy.
23. Console checkout bags are hardcoded at 10 monetary units each and calculated as one bag per 10 scanned items.
24. Core types are not designed for concurrent mutation from multiple threads on the same checkout instance.
25. Console pricing rules are loaded from `pricing-rules.json` at startup.
26. Console deserializer currently supports policy types `n_for_x` and `percent_off`.
27. Console policy type matching is case-insensitive after normalization.
28. Console policy parameters are flattened per policy object (`quantity`, `price`, `percentage`) and are deserialized using web defaults (case-insensitive property matching).
29. Test coverage gate targets `CheckoutKata.Core` with minimum 95% line coverage when `EnforceCoverageGate=true`.
