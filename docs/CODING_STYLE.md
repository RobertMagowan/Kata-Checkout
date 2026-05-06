# Coding Style

This repository uses a specific C# formatting and readability style.  
When adding or modifying code, follow these rules.

## Line Endings And Whitespace

- Use `CRLF` line endings.
- Use 4 spaces for indentation.
- Do not use tabs.
- Keep one blank line between logical blocks and members.

## Namespace And Using Layout

- Use file-scoped namespaces:
  - `namespace CheckoutKata.Core.Services;`
- Place `using` statements after the file-scoped namespace.
- In `CheckoutKata.Core.*`, prefer local shorthand usings when appropriate:
  - `using Interfaces;`
  - `using Models;`

## Braces And Wrapping

- Use Allman braces for types, methods, and control flow.
- Wrap long parameter lists and arguments vertically.
- Keep continuation indentation aligned for readability.

## Validation And Guard Clauses

- Validate null arguments first with `ArgumentNullException.ThrowIfNull(...)`.
- Follow with domain validation (`ArgumentException`, etc.).
- Compact single-line guard clauses are preferred when clear.

Example:

```csharp
if (rule.UnitPrice <= 0) throw new ArgumentException("Unit price must be greater than zero.", nameof(rule));
```

## Core Coding Conventions

- Prefer `sealed` for concrete classes that are not intended for inheritance.
- Use explicit `checked(...)` arithmetic for pricing/count operations.
- Use deterministic comparers explicitly (`StringComparer.Ordinal`).
- Keep methods small and intention-revealing.
- Prefer explicit flow over terse/clever expression forms when readability is reduced.

## Test Conventions (NUnit)

- Use stacked class categories:
  - `[Category("Core")]`
  - `[Category("Pricing")]`
- Name tests in `Method_WithCondition_ExpectedOutcome` style.
- Keep Arrange/Act/Assert sections separated by blank lines.
- Put test helper methods at the bottom of each test file.
- Use collection expressions (`[...]`) for compact test data setup where it improves readability.

## Consistency Rule

- Match the style of the file you are editing.
- Do not reformat unrelated code in the same file unless explicitly requested.

