# Madowaku (窓枠): Win32 CsWin32 support for .NET and .NET Framework

[![Build](https://github.com/JeremyKuhne/madowaku/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JeremyKuhne/madowaku/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/JeremyKuhne/madowaku/branch/main/graph/badge.svg)](https://codecov.io/gh/JeremyKuhne/madowaku)
[![NuGet](https://img.shields.io/nuget/v/KlutzyNinja.Madowaku.svg)](https://www.nuget.org/packages/KlutzyNinja.Madowaku/)

Provides useful Win32 functionality based on both for .NET and .NET Framework applications.

## Supported architectures

Madowaku supports **x64** and **ARM64**, with separately compiled CsWin32 interop for each.
AnyCPU and x86 builds are not supported. Set the application's compiler target explicitly:

```xml
<PropertyGroup>
  <PlatformTarget>ARM64</PlatformTarget>
</PropertyGroup>
```

Use `x64` for an x64 application. The single `KlutzyNinja.Madowaku` package includes both
architectures for all three target frameworks; its build targets select the matching compiler
reference and runtime assembly. There is no architecture-neutral reference assembly.
ARM64 applications using the .NET Framework build require the ARM64 .NET Framework 4.8.1 runtime.

Madowaku (窓枠) is the Japanese word for "window frame". This library provides extensions on top of CsWin32
to make it easier to create Win32 applications in .NET and .NET Framework.

Some of the design goals include:

- Avoiding unnecessary allocations
- Avoiding code that prevents AOT compilation on .NET

## Features

- Direct COM support (no need for COM Interop)
- COM scoping mechanisms - see `ComScope`, `AgileComPointer`
- Extended `VARIANT` and `SAFEARRAY` support
- Dynamic COM object creation with `ComClassFactory` (including directly from a dll)
- Pattern for implementing `IComIID` on .NET Framework (see `IRecordInfo`)

See [CONTRIBUTING.md](CONTRIBUTING) for more information on how to contribute to this project.
