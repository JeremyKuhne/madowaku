---
core: dotnet-polyfills
core-pin: v0.10.0
---

# madowaku overlay - dotnet-polyfills

Repository-specific companion to the vendored [dotnet-polyfills](SKILL.md) skill.
The `SKILL.md` and its `references/packages.md` page are a **pinned copy of the
portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku bindings

- **The downlevel target is `net472`.** A polyfill exists to let modern-.NET APIs
  compile and behave correctly there.
- **Hand-rolled polyfills live under**
  [madowaku/Framework/](../../../madowaku/Framework/), each declaring the BCL
  namespace it polyfills (for example
  `Framework/System/Runtime/InteropServices/ComWrappers.cs` ->
  `namespace System.Runtime.InteropServices;`). The folder is net472-only by
  construction, so no `#if NETFRAMEWORK` guards go inside it - see
  [interop.instructions.md](../../../.github/instructions/interop.instructions.md).
- **`KlutzyNinja.Touki` is an additive source.** madowaku depends on Touki;
  prefer its runtime polyfills and helpers before hand-rolling one.
- **Package versions** (`PolySharp`, `System.Memory`, `Microsoft.Bcl.*`) are
  centralized in
  [Directory.Packages.props](../../../Directory.Packages.props).

## Cross-references

- [`cswin32-interop`](../cswin32-interop/SKILL.md) - the CsWin32 interop rules and
  the `ComWrappers` shim, the largest polyfilled surface in madowaku.
- [`framework-jit-optimization`](../framework-jit-optimization/SKILL.md) - net472
  codegen consequences of a polyfilled path.

## Updating

Pull upstream changes with `gh skill update dotnet-polyfills` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
