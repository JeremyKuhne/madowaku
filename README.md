# Madowaku (窓枠): Win32 CsWin32 support for .NET and .NET Framework

[![Build](https://github.com/JeremyKuhne/madowaku/actions/workflows/dotnet.yml/badge.svg)](https://github.com/JeremyKuhne/madowaku/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/KlutzyNinja.Madowaku.svg)](https://www.nuget.org/packages/KlutzyNinja.Madowaku/)

Provides useful Win32 functionality based on both for .NET and .NET Framework applications.

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
