---
core: scratch-buffer-strategy
core-pin: v0.10.0
---

# madowaku overlay - scratch-buffer-strategy

Repository-specific companion to the vendored [scratch-buffer-strategy](SKILL.md)
skill. The `SKILL.md` and its bundled
`references/arraypool-performance.md` page are a **pinned copy of the portable
core** from [JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills)
(see the `metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku bindings

- **Buffer helper**: the core's `BufferScope<T>` is the Touki
  (`KlutzyNinja.Touki`) type; madowaku depends on Touki, so prefer it over a raw
  `ArrayPool<T>` rental for scratch buffers.
- **Crossovers apply across `net472` and modern .NET.** madowaku builds `net472`
  in addition to `net10.0` / `net10.0-windows10.0.22000.0`, and the size
  crossovers in `references/arraypool-performance.md` are measured on those JITs.
- **Validate a choice** with a [madowaku.perf](../../../madowaku.perf/madowaku.perf.csproj)
  benchmark before committing to a strategy on a hot path.

## Cross-references

- [`performance-testing`](../performance-testing/SKILL.md) - the harness that
  confirms a stackalloc / rental / `BufferScope<T>` crossover.
- [`framework-jit-optimization`](../framework-jit-optimization/SKILL.md) - the
  net472 codegen behavior behind the crossovers.

## Updating

Pull upstream changes with `gh skill update scratch-buffer-strategy` (review the
diff, re-pin `core-pin`). Keep madowaku-specific additions here.
