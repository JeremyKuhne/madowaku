# Contributing

## Building and testing

The solution defaults to the host's 64-bit architecture. Both platforms can be selected directly:

```shell
dotnet build -c Release -p:Platform=x64
dotnet build -c Release -p:Platform=ARM64
dotnet test -c Release -p:Platform=x64
```

Run tests on a matching native host. The library keeps its existing .NET 10, Windows-qualified
.NET 10, and .NET Framework 4.7.2 targets; tests run on Windows-qualified .NET 10 and .NET Framework 4.8.1.
Outputs and intermediate files are separated by platform and configuration.

```shell
dotnet pack madowaku/madowaku.csproj -c Release -o artifacts/packages
```

Packing builds both architectures for each target framework and produces one
`KlutzyNinja.Madowaku` package. Using `--no-build` requires both platform builds to exist.
The package contains architecture-specific assemblies under `runtimes/win-x64/lib/` and
`runtimes/win-arm64/lib/`, with MSBuild selection targets in `build/` and `buildTransitive/`.
This also selects the correct reference when Madowaku is an indirect dependency.

Use `tools/Test-ArchitecturePackage.ps1 -PackagePath <path-to-nupkg>` to verify all packaged
assembly architectures, compiler/runtime asset selection for all target frameworks, native
consumer execution, and rejection of AnyCPU consumers. It uses a temporary feed and package
cache and does not publish or change the global NuGet cache.

The validation also publishes RID-specific test applications for both architectures and
executes the one matching the host. ARM64 execution is covered by the ARM64 CI runner.

### Pull requests are welcome!

By submitting a PR you:

1. Confirm that you wrote the code (or otherwise have the right to contribute it), **and**
2. Agree that your contribution will be licensed under the MIT License that governs this project.

You retain copyright to your work; you simply grant Jeremy W. Kuhne and all downstream users a perpetual,
irrevocable MIT license to use, modify, and redistribute it.