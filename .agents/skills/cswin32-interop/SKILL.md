---
name: cswin32-interop
description: 'Guides CsWin32 P/Invoke interop in a multi-targeted .NET library. Consult when replacing [DllImport] with source-generated PInvoke.* calls, working with the generated Windows.Win32 projections (HANDLE / HMODULE / HRESULT / BOOL and the typed enum/constant types), configuring NativeMethods.txt / NativeMethods.json, gating Windows-only code across target frameworks, or choosing between CsWin32 and [LibraryImport]. Paired with the cswin32-com skill for the struct-based COM layer.'
argument-hint: 'Describe the Windows API or interop code you are migrating or adding.'
license: MIT
compatibility: Requires the .NET SDK and the Microsoft.Windows.CsWin32 source generator; projects Windows APIs.
metadata:
  portability: portable
  applicability: dotnet
  binding: optional-overlay
  risk: local-write
  maturity: canary
  requires: none
  related: cswin32-com, dotnet-polyfills, scratch-buffer-strategy
---

# CsWin32 P/Invoke interop

If `overlay.md` exists beside this file, read it before acting; it contains
repository-specific bindings. This core remains usable without it.

[CsWin32](https://github.com/microsoft/CsWin32) is a source generator that
replaces hand-written `[DllImport]` declarations with generated `PInvoke.*`
calls and strongly-typed projections of Win32 handles, structs, enums, and
constants. You list the API, type, or constant names you need in
`NativeMethods.txt`; the generator emits them and pulls in their dependencies.

This skill is the general P/Invoke layer. The struct-based COM layer builds on
it - see the paired **cswin32-com** skill, whose vtable methods follow the same
[blittable-signatures](blittable-signatures.md) rules.

## Rules

1. **Replace `[DllImport]` with `PInvoke.*`.** Delete the old declaration and
   any hand-written struct/enum/constant it depended on - the generator emits
   projected equivalents. See [types-and-constants.md](types-and-constants.md).
2. **Use the generated types directly** (`HANDLE`, `HMODULE`, `HRESULT.S_OK`,
   `FILE_FLAGS_AND_ATTRIBUTES`, ...). Never keep a parallel local copy of a
   Win32 enum or struct CsWin32 already projects.
3. **Call the generated method directly** - no thin wrapper. When the generator
   runs in one project, its types reach friend assemblies via
   `InternalsVisibleTo`.
4. **Prefer CsWin32 for Windows APIs.** Reserve `[LibraryImport]` (or, on older
   targets, `[DllImport]`) for genuinely non-Windows native calls such as
   `libc`.
5. **Preserve the original error-handling contract when migrating.** Check the
   old declaration for `PreserveSig` / `SetLastError` / `BOOL` / `HRESULT`
   semantics and reproduce them: `PreserveSig=false` becomes
   `.ThrowOnFailure()`; `SetLastError=true` plus a failed `BOOL` becomes
   `throw new Win32Exception()`. Silently returning where the old code threw is
   a behavior change. The paired cswin32-com skill has the COM-side parity
   table.
6. **Keep signatures blittable.** CsWin32 is configured `allowMarshaling: false`,
   so every `[DllImport]` and every COM vtable method must be blittable. The
   full rule set is in [blittable-signatures.md](blittable-signatures.md).

## The Windows-interop compile gate

CsWin32 output is Windows-only. A cross-platform or source-buildable library
gates its Windows-interop code behind a repo-specific compile symbol (the
"Windows-interop gate") plus, on any code path that also has a non-Windows
branch, a runtime `IsWindows` check. A Windows-only library that still
multi-targets frameworks has **no** platform gate - the cross-cut there is
**target framework**, not platform. Either way,
[gating.md](gating.md) covers guard selection, the correct nesting order,
`CA1416`, and verifying the guarded build. Your overlay names the concrete gate
(or states there is none).

## Configuration

- **`NativeMethods.txt`** - one API / type / constant name per line; the
  generator projects each and pulls in its dependencies.
- **`NativeMethods.json`** - generator options. `allowMarshaling: false` (raw
  blittable signatures, no marshaller) and `useSafeHandles: false` are the
  interop-friendly defaults these skills assume.
- Run the generator in a single project and share via `InternalsVisibleTo`
  rather than adding CsWin32 to every project.

## Sibling pages

- [blittable-signatures.md](blittable-signatures.md) - the signature rules
  shared by `[DllImport]` and COM vtables (`HRESULT`, `T**` vs `out T*`,
  `void*`, `nint`, `PCWSTR` / `PWSTR`, typed enum parameters).
- [types-and-constants.md](types-and-constants.md) - using the generated
  handle / enum / constant projections, "grep the metadata before redefining",
  type conversions, FILETIME, and native integers.
- [gating.md](gating.md) - multi-TFM / multi-platform guards, `CA1416`, a
  stack-first scratch buffer, and verifying the guarded build.
