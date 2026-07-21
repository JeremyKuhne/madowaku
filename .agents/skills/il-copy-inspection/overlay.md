---
core: il-copy-inspection
core-pin: v0.11.0
---

# madowaku overlay - il-copy-inspection

Repository-specific companion to the vendored [il-copy-inspection](SKILL.md)
skill. The `SKILL.md` and its `references/copy-opcodes.md` page are a **pinned
copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku bindings

- **The high-value targets are the interop value types.** madowaku is dense with
  large mutable structs passed by value at the COM boundary - `VARIANT`,
  `SAFEARRAY`, `ComScope<T>`, `BSTR`, `PWSTR` - where a hidden defensive copy or
  a box silently costs allocations and correctness.
- **Read IL on both targets** to compare copies emitted by the C# compiler. To
  determine whether the net481 RyuJIT retains or elides an emitted copy, inspect
  machine code with `DisassemblyDiagnoser`; IL alone cannot answer that.
- **Pair a finding with a benchmark** in
  [madowaku.perf](../../../madowaku.perf/madowaku.perf.csproj) before reshaping a
  hot type.

## Cross-references

- [`cswin32-com`](../cswin32-com/SKILL.md) - the struct-based COM interop that
  produces most of madowaku's copy-sensitive types.
- [`framework-jit-optimization`](../framework-jit-optimization/SKILL.md) - the
  machine-code layer that determines whether a JIT retained an emitted copy.
- [`performance-testing`](../performance-testing/SKILL.md) - the harness that
  quantifies the copy or box the IL reveals.

## Updating

Pull upstream changes with `gh skill update il-copy-inspection` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
