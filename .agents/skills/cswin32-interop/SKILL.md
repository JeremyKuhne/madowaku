---
name: cswin32-interop
description: 'Guides CsWin32 P/Invoke interop in a multi-targeted .NET library. Consult when replacing [DllImport] with source-generated PInvoke.* calls, working with generated Windows.Win32 projections (HANDLE / HMODULE / HRESULT / BOOL and typed enum/constant types), configuring NativeMethods.txt / NativeMethods.json, composing a public PInvoke across foundation/owner/extender packages with extensionReceiver, deciding which support library owns a helper, auditing native allocation and byte/element lengths, gating Windows-only code across target frameworks, or choosing between CsWin32 and [LibraryImport]. Paired with the cswin32-com skill for the struct-based COM layer.'
license: MIT
compatibility: Requires the .NET SDK and the Microsoft.Windows.CsWin32 source generator; projects Windows APIs.
metadata:
  portability: portable
  applicability: dotnet
  binding: optional-overlay
  risk: local-write
  maturity: canary
  requires: none
  related: cswin32-com, dotnet-polyfills, scratch-buffer-strategy, security-review
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
3. **Call the generated method directly** - no thin wrapper. Within one
  ownership layer, generate once and share internal types with friend
  assemblies. When a downstream package intentionally extends a public owner,
  use the [owner/extender composition](composition.md) model instead.
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
7. **Audit ownership and size units.** Generated wrappers do not decide which
  allocator frees an output, whether a COM reference remains caller-owned, or
  whether a length is bytes or elements. Record and test those contracts using
  [ownership-and-units.md](ownership-and-units.md).

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
- Run the generator in a single project within an ownership layer and share via
  `InternalsVisibleTo` rather than adding CsWin32 to every implementation
  project. A package that extends another package's public projection is a
  separate layer: configure `className` / `extensionReceiver` as described in
  [composition.md](composition.md).

### Source generator versus build task

The Roslyn source generator is the default. Setting
`CsWin32RunAsBuildTask=true` replaces it with an MSBuild task that writes `.cs`
files under the intermediate output directory and adds them to `Compile`; the
source generator stands down. Treat this as an opt-in for a specific build
ordering, generated-file, or tooling requirement, not as a general upgrade.

Before keeping build-task mode, clean and build every TFM, compare the emitted
public API, and measure clean-build time. It can be materially slower. It also
does not make an `allowMarshaling: false` project more AOT-safe by itself, so do
not enable `DisableRuntimeMarshalling` merely because generation moved to the
task.

## Sibling pages

- [blittable-signatures.md](blittable-signatures.md) - the signature rules
  shared by `[DllImport]` and COM vtables (`HRESULT`, `T**` vs `out T*`,
  `void*`, `nint`, `PCWSTR` / `PWSTR`, typed enum parameters).
- [types-and-constants.md](types-and-constants.md) - using the generated
  handle / enum / constant projections, "grep the metadata before redefining",
  type conversions, FILETIME, and native integers.
- [composition.md](composition.md) - composing one public `PInvoke` across an
  owner package and downstream extender, including constants, XML docs, and
  package verification.
- [library-layering.md](library-layering.md) - placing generic utilities,
  shared Win32 support, domain extensions, and applications in a directed
  package stack, with migration and release ordering.
- [ownership-and-units.md](ownership-and-units.md) - matching native allocators
  and deallocators, caller-owned COM references, failure cleanup, and
  byte-versus-element conversions.
- [gating.md](gating.md) - multi-TFM / multi-platform guards, `CA1416`, a
  stack-first scratch buffer, and verifying the guarded build.
