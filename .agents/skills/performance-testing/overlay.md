---
core: performance-testing
core-pin: v0.11.0
---

# madowaku overlay - performance-testing

Repository-specific companion to the vendored [performance-testing](SKILL.md)
skill. The `SKILL.md` and its sibling pages (`authoring.md`, `running.md`,
`interpreting-requests.md`, `interpreting-results.md`, `reading-codegen.md`,
`investigation-workflow.md`) are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift. Everything madowaku-specific lives here.

> **Pinned to the commons v0.11.0 tag.** Pull later upstream changes with
> `gh skill update performance-testing`, review the diff, then re-pin `core-pin`.

## Concrete bindings for the core's placeholders

- **Perf project**: the core's `<root>.perf` is
  [madowaku.perf](../../../madowaku.perf/madowaku.perf.csproj), namespace
  `madowaku.perf`. It multi-targets `net10.0-windows10.0.22000.0` and `net481`
  and references both [madowaku](../../../madowaku/madowaku.csproj) and
  [madowaku.tests](../../../madowaku.tests/madowaku.tests.csproj) so test
  fixtures can seed benchmark setup.
- **Target frameworks**: `<tfm>` is `net10.0-windows10.0.22000.0` or `net481`.
  Always pass `-f`, because the project is multi-targeted.
- **Globals**:
  [madowaku.perf/GlobalUsings.cs](../../../madowaku.perf/GlobalUsings.cs) already
  imports `BenchmarkDotNet.Attributes` and `BenchmarkDotNet.Jobs`. Do not
  re-import them.
- **Coding style**: follow [AGENTS.md](../../../AGENTS.md) (explicit types, no
  `var`, target-typed `new()`, collection expressions, indented XML docs).
- **Example benchmark**:
  [madowaku.perf/VariantConversionPerf.cs](../../../madowaku.perf/VariantConversionPerf.cs)
  shows the `[MemoryDiagnoser]` layout and realistic `VARIANT` conversion cases
  drawn from the tests.

## Running

```pwsh
# modern .NET
dotnet run -c Release -f net10.0-windows10.0.22000.0 --project madowaku.perf -- --filter *VariantConversionPerf*
# .NET Framework
dotnet run -c Release -f net481 --project madowaku.perf -- --filter *VariantConversionPerf*
```

`-c Release` is mandatory. Compare both TFMs before claiming a win, since the
net481 and modern-.NET JITs differ significantly.

## Reproducible investigations

Use [investigation-workflow.md](investigation-workflow.md) when a performance
question spans phases, compares an exact external source revision, evaluates
multiple candidates, or must preserve a dirty-worktree experiment. Keep its
ledger and reconstruction artifacts local unless the user explicitly approves
publishing them and the source bundle contains no secret or unauthorized data.

Madowaku does not currently vendor a repository-specific trace-analyzer skill.
Use BenchmarkDotNet diagnosers and [reading-codegen.md](reading-codegen.md) for
local drill-down. Do not claim source-line profile evidence without first wiring
and validating an appropriate trace tool.

## madowaku specifics

- Benchmarks that touch code under
  [madowaku/Framework/](../../../madowaku/Framework/) (net472-only polyfills)
  must guard those references with `#if NETFRAMEWORK`.
- Prefer benchmark data derived from existing `madowaku.tests` cases (for
  example, `VARIANT` conversion inputs) so the numbers stay realistic.

## Cross-references (the core names these skills generically)

- [`framework-jit-optimization`](../framework-jit-optimization/SKILL.md) - the
  net481-versus-net10 codegen differences these benchmarks exist to validate.
- [`scratch-buffer-strategy`](../scratch-buffer-strategy/SKILL.md) - zeroed
  `stackalloc` versus `ArrayPool` versus Touki `BufferScope<T>` crossovers.
- [`il-copy-inspection`](../il-copy-inspection/SKILL.md) - reading IL to find the
  struct copies and boxing a benchmark surfaced.
- [`cswin32-com`](../cswin32-com/SKILL.md) and
  [`cswin32-interop`](../cswin32-interop/SKILL.md) - the interop surface most
  madowaku hot paths run through.
- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) - gates a perf claim
  that drives a code change on a `madowaku.perf` benchmark (or an explicit "not
  measured" note).
