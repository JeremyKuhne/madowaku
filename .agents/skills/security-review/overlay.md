---
core: security-review
core-pin: v0.10.0
---

# madowaku overlay - security-review

Repository-specific companion to the vendored [security-review](SKILL.md) skill.
The `SKILL.md` and its sibling pages (`checklist.md`, `principles.md`,
`unsafe-apis.md`, `reporting.md`) are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku bindings

- **The whole library is an unsafe-interop surface.** Nearly every type crosses
  a native boundary, so the core's `unsafe` / `Unsafe.*` / `MemoryMarshal.*` /
  `Marshal.*` triggers fire broadly. Pay closest attention to raw pointer
  lifetime, `BSTR` / `PWSTR` ownership, `SAFEARRAY` bounds, and `VARIANT` union
  reinterpretation.
- **COM reference counting is a safety property.** A missing or double
  `Release` through `ComScope<T>` / `IUnknown` is a correctness *and* a resource
  bug; review activation and disposal paths.
- **Cross-target behavior counts.** A pattern that is safe on modern .NET may
  behave differently on `net472`; confirm both.

## Cross-references

- [`cswin32-com`](../cswin32-com/SKILL.md) and
  [`cswin32-interop`](../cswin32-interop/SKILL.md) - the interop patterns whose
  misuse the review is guarding against.
- [`performance-testing`](../performance-testing/SKILL.md) - for a ReDoS or
  algorithmic-complexity concern, quantify it with a benchmark.

## Updating

Pull upstream changes with `gh skill update security-review` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
