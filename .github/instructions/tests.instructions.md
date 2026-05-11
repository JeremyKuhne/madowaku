---
applyTo: "madowaku.tests/**/*.cs"
---

# Test conventions (`madowaku.tests/`)

Path-specific rules for the xUnit v3 test project. The general `AGENTS.md`
rules (coding style, whitespace, file header) still apply.

## Framework

- xUnit v3 with the **Microsoft Testing Platform** runner
  (`UseMicrosoftTestingPlatformRunner` is set in the csproj). Run tests
  with `dotnet test`; do not invoke `vstest` directly.
- Target frameworks: `net10.0-windows10.0.22000.0` and `net481`. All
  tests must build and pass on both.
- `Xunit` is available via a project-level `<Using>`; `using Xunit;` is
  unnecessary.
- `FluentAssertions` is referenced. Either `Assert.*` (xUnit) or
  `.Should().*` (FluentAssertions) is acceptable; pick one style per
  file and stay consistent within it.

## Naming and structure

- Name tests `MethodName_StateUnderTest_ExpectedBehavior` (e.g.
  `IntConversion_RoundTrip`, `EmptyVariant_HasExpectedProperties`).
- Mirror the source layout: a test for `madowaku/Windows/Win32/Foo.cs`
  lives at `madowaku.tests/Windows/Win32/FooTests.cs` with namespace
  matching the production type.
- One `public class FooTests` per type under test. Multiple `[Fact]`s
  per class is fine.
- Do **not** add "Arrange / Act / Assert" comments. The structure should
  be obvious from the code.

## Coverage expectations

- Cover edge cases and negative paths, not just the happy path.
- For every public API change, add or update at least one test that
  exercises the new shape.
- For cross-TFM bugs, add a regression test that would fail on the
  affected TFM specifically.

## Cross-TFM gotchas

- **`nint` does not implement `IEquatable<nint>` on net472/net481.**
  Generic type parameters constrained to `unmanaged, IEquatable<T>`
  cannot be instantiated with `nint` in cross-TFM tests. Use `int` or
  another concrete value type.
- **Release-mode inlining changes codegen.** Run
  `dotnet test -c Release` before declaring a fix done; `Unsafe.As` on a
  method parameter is a known foot-gun on net481 RyuJIT.
- **`[ExcludeFromCodeCoverage(Justification = ...)]` is .NET 5+ only.**
  Not available on net481. Use bare `[ExcludeFromCodeCoverage]` with the
  rationale in a `//` comment above the attribute.

## What not to do

- Do not introduce a separate test SDK (MSTest, NUnit) alongside xUnit.
- Do not gate tests on a specific machine, locale, or installed SDK
  without a clear `[Fact(Skip = "...")]` reason.
- Do not use `Thread.Sleep` for synchronization; use the appropriate
  xUnit / `Task`-based primitives.
