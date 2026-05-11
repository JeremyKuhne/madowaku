---
applyTo: "**/*.csproj, **/*.props, **/*.targets, **/Directory.Packages.props"
---

# MSBuild file conventions

Path-specific rules for `.csproj`, `.props`, `.targets`, and central
package management files. The general `AGENTS.md` rules still apply.

## Target frameworks

- The library multi-targets `$(DotNetCoreVersion)-$(WindowsPlatformVersion)`,
  `$(DotNetCoreVersion)`, and `$(DotNetFrameworkVersion)`. These are
  defined in [Directory.Build.props](../../Directory.Build.props):
  - `DotNetCoreVersion` = `net10.0`
  - `DotNetFrameworkVersion` = `net472`
  - `WindowsPlatformVersion` = `windows10.0.22000.0`
- The test project multi-targets the windowed modern TFM and `net481`.
- **Never hard-code `net10.0`, `net472`, or the windows platform suffix**
  in csproj `TargetFrameworks` lists. Reference the properties instead so
  the values stay in one place.

## Central package management

- Package versions live in
  [Directory.Packages.props](../../Directory.Packages.props). Individual
  csproj files list `<PackageReference Include="..."/>` **without** a
  `Version` attribute.
- When adding a new dependency, add the `PackageVersion` entry in
  `Directory.Packages.props` first, then reference it from the csproj.

## Conditionals

- Prefer property-based conditions over hard-coded TFM strings:
  `Condition="'$(TargetFramework)' == '$(DotNetFrameworkVersion)'"` over
  `Condition="'$(TargetFramework)' == 'net472'"`.
- Group net472-only items inside a single `ItemGroup` with a
  `Condition` on the group, not per-item conditions.

## Formatting

- 2-space indent for MSBuild XML (matches existing files).
- One blank line between top-level `PropertyGroup` / `ItemGroup`
  elements.
- Comment non-obvious settings inline with `<!-- ... -->`; mention
  *why*, not *what*.

## What not to do

- Do not add `<NoWarn>` entries to silence platform-compatibility
  warnings (CA1416) without a clear justification comment.
- Do not introduce per-project `nuget.config` or per-project package
  source overrides.
- Do not change `LangVersion`, `Nullable`, or `ImplicitUsings` defaults
  in `Directory.Build.props` without coordinating across all projects.
