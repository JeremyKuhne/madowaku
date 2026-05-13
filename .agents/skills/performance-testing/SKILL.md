---
name: performance-testing
description: Author and run BenchmarkDotNet performance tests in the madowaku.perf project. Use when adding new benchmarks, running existing ones, comparing implementations, or evaluating allocations and memory usage for code in madowaku.
argument-hint: Describe the API or hot path you want to benchmark and whether you care most about time, allocations, or both.
---

# Performance testing in madowaku.perf

The [madowaku.perf](../../../madowaku.perf/madowaku.perf.csproj) project hosts
all [BenchmarkDotNet](https://benchmarkdotnet.org/) benchmarks for this repo.
It multi-targets modern .NET (`$(DotNetCoreVersion)-$(WindowsPlatformVersion)`)
and `net481`, so you can compare runtime behavior between the two JITs.

This skill is adapted from Touki's performance-testing skill and narrowed to
madowaku conventions.

## 1. Authoring a benchmark

### File and class layout

- One benchmark class per file, file named after the class.
- Namespace is always `madowaku.perf`.
- Class is `public` and has one or more `[Benchmark]` methods.
- Use the standard repo file header.
- Follow [AGENTS.md](../../../AGENTS.md) coding rules (explicit types, XML docs,
  no `var`, collection expressions where applicable).

### Globals already imported

[GlobalUsings.cs](../../../madowaku.perf/GlobalUsings.cs) already provides:

- `BenchmarkDotNet.Attributes`
- `BenchmarkDotNet.Jobs`

Do not re-import those in each benchmark file.

### Required attributes

Always annotate benchmark classes with `[MemoryDiagnoser]`.

When comparing alternatives, set one method to baseline:

```csharp
[Benchmark(Baseline = true)]
public int Current()
{
    return _value;
}

[Benchmark]
public int Candidate()
{
    return _value + 1;
}
```

### What benchmark methods must do

- Return a value derived from the measured work (not `void`).
- Keep setup outside the measured path using `[GlobalSetup]`.
- Avoid helper indirection between benchmark method and system-under-test
  unless you intentionally want that overhead measured.
- For mutating operations, return a digest or sampled value from the mutated
  data (`buffer[0]`, sum, xor, etc.) so dead-code elimination cannot erase work.

## 2. Running benchmarks

The benchmark entrypoint uses `BenchmarkSwitcher.FromAssembly(...)`, so all
CLI args after `--` are forwarded to BenchmarkDotNet.

### Target framework is required

Because `madowaku.perf` is multi-targeted, always pass `-f`.

```powershell
# modern .NET
 dotnet run -c Release -f net10.0-windows10.0.22000.0 --project madowaku.perf -- --filter *VariantConversionPerf*

# .NET Framework
 dotnet run -c Release -f net481 --project madowaku.perf -- --filter *VariantConversionPerf*
```

`-c Release` is mandatory for useful numbers.

### Useful switches

- `--job short`: quick smoke pass while iterating.
- `--memory`: force memory diagnoser for the run.
- `--exporters github`: emit GitHub-flavored markdown summary.
- `--disasm`: inspect generated assembly for codegen investigations.

## 3. Reading memory results

With `[MemoryDiagnoser]`, output includes:

- `Gen0`, `Gen1`, `Gen2`: collections per 1000 ops.
- `Allocated`: managed bytes per operation.

Guidance:

- `Allocated` should be `-` or `0 B` for allocation-free paths.
- Use `Ratio` with `Allocated` to avoid trading CPU gains for allocation
  regressions.
- Compare both TFMs before claiming an improvement. net481 and modern .NET can
  differ significantly in JIT behavior.

## 4. Workflow checklist

1. Add `<Name>.cs` under `madowaku.perf/` with `public class <Name>`.
2. Add `[MemoryDiagnoser]` and a clear baseline benchmark.
3. Ensure every benchmark returns a value tied to real work.
4. Build Release:

   ```powershell
   dotnet build -c Release madowaku.perf/madowaku.perf.csproj
   ```

5. Smoke-test with short job on both TFMs:

   ```powershell
   dotnet run -c Release -f net10.0-windows10.0.22000.0 --project madowaku.perf -- --job short --filter *<Name>*
   dotnet run -c Release -f net481 --project madowaku.perf -- --job short --filter *<Name>*
   ```

6. Run full benchmark if needed (remove `--job short`).
7. Capture summary from console or artifacts and include it with perf claims.

## 5. Repo-specific notes

- `madowaku.perf` references both `madowaku` and `madowaku.tests` so test-only
  fixtures can be reused in benchmark setup.
- If benchmarking code that only exists under `madowaku/Framework/`, guard
  references with `#if NETFRAMEWORK`.
- Keep benchmark data realistic and derived from existing tests where possible
  (for example, VARIANT conversion cases from `madowaku.tests`).
