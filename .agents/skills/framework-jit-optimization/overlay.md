---
core: framework-jit-optimization
core-pin: v0.11.0
---

# madowaku overlay - framework-jit-optimization

Repository-specific companion to the vendored
[framework-jit-optimization](SKILL.md) skill. The `SKILL.md` and its sibling
pages are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku bindings

- **The Framework target is `net472`** (tests and perf use `net481`); the main
  library also builds `net10.0` and `net10.0-windows10.0.22000.0`. Code must
  behave correctly on every target - see [AGENTS.md](../../../AGENTS.md).
- **net472-only code lives under**
  [madowaku/Framework/](../../../madowaku/Framework/), which is net472-scoped by
  construction - no `#if NETFRAMEWORK` inside it (see
  [interop.instructions.md](../../../.github/instructions/interop.instructions.md)).
- **Enum flags**: never use `Enum.HasFlag`; it boxes on net472. Use the Touki
  enum extension methods (`AreFlagsSet`, `IsOnlyOneFlagSet`, `AreAnyFlagsSet`,
  `SetFlags`, `ClearFlags`).

## Cross-references

- [`performance-testing`](../performance-testing/SKILL.md) - the harness that
  proves a Framework codegen win or regression on both JITs.
- [`scratch-buffer-strategy`](../scratch-buffer-strategy/SKILL.md) - the
  net481/net10 buffer-strategy crossovers.
- [`cswin32-interop`](../cswin32-interop/SKILL.md) - the interop surface and the
  net472 polyfill layout most hot paths cross.

## Updating

Pull upstream changes with `gh skill update framework-jit-optimization` (review
the diff, re-pin `core-pin`). Keep madowaku-specific additions here.
