---
applyTo: "madowaku.tests/**/*.cs"
---

# Test conventions (`madowaku.tests/`)

Path-specific rules for the MSTest test project. The general `AGENTS.md`
rules (coding style, whitespace, file header) still apply.

## Framework

- **MSTest** with the **Microsoft Testing Platform** runner
  (`EnableMSTestRunner` is set in the csproj). Run tests with
  `dotnet test`; do not invoke `vstest` directly.
- Tests run with **method-level parallelism**. The assembly-level
  `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]`
  attribute lives in `AssemblyInfo.cs`. Do not rely on shared mutable
  static state between test methods; if a test cannot run in parallel,
  mark it `[DoNotParallelize]`.
- Target frameworks: `net10.0-windows10.0.22000.0` and `net481`. All
  tests must build and pass on both.
- `Microsoft.VisualStudio.TestTools.UnitTesting` is available via a
  project-level `<Using>`, so `[TestClass]` / `[TestMethod]` need no
  explicit `using`.
- **AwesomeAssertions** (a FluentAssertions fork) is globally used via
  `global using AwesomeAssertions;` in `GlobalUsings.cs`. Assert with
  `.Should().*` only; do not use MSTest's native `Assert.*`. Use
  `FluentActions.Invoking(() => ...).Should().Throw<T>()` for exception
  assertions.

## Naming and structure

- Name tests `MethodName_StateUnderTest_ExpectedBehavior` (e.g.
  `IntConversion_RoundTrip`, `EmptyVariant_HasExpectedProperties`).
- Mirror the source layout: a test for `madowaku/Windows/Win32/Foo.cs`
  lives at `madowaku.tests/Windows/Win32/FooTests.cs` with namespace
  matching the production type.
- One `[TestClass] public class FooTests` per type under test. Multiple
  `[TestMethod]`s per class is fine.
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

- Do not introduce a separate test SDK (xUnit, NUnit) alongside MSTest.
- Do not use MSTest's native `Assert.*`; use AwesomeAssertions
  `.Should().*` throughout.
- Do not gate tests on a specific machine, locale, or installed SDK
  without a clear `[Ignore("...")]` reason.
- Do not use `Thread.Sleep` for synchronization; use the appropriate
  `Task`-based primitives.
