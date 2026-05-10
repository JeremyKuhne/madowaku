---
name: cswin32-interop
description: 'Guides CsWin32 P/Invoke interop in madowaku. Consult when working with PInvoke / PInvokeMadowaku classes, the Windows.Win32 namespaces, HANDLE / HMODULE / HRESULT / BOOL types, TFM gating (net472 vs modern .NET), the ComWrappers polyfill, polyfill layout (Touki vs local), or replacing [DllImport] with CsWin32-generated calls.'
argument-hint: 'Describe the Win32 API or interop code you are migrating or adding.'
---

# CsWin32 Interop Guide (madowaku)

[CsWin32](https://github.com/microsoft/CsWin32) replaces `[DllImport]` with
source-generated `PInvoke.*` calls. madowaku is a Windows-only library. The
cross-cut here is **TFM**, not platform: code must compile on `net472`,
`net10.0`, and `net10.0-windows10.0.22000.0`.

Adapted from
[dotnet/msbuild &mdash; cswin32-interop](https://github.com/dotnet/msbuild/blob/main/.github/skills/cswin32-interop/SKILL.md);
trimmed to the cases that actually arise in madowaku and reconciled with our
single-library, no-source-build layout.

## Rules

1. Replace `[DllImport]` with `PInvoke.*` / `PInvokeMadowaku.*`. Delete old
   declarations and hand-written structs/enums/constants.
2. Use CsWin32 types directly (`HANDLE`, `HMODULE`, `HRESULT.S_OK`,
   `FILE_FLAGS_AND_ATTRIBUTES`, etc.). Never declare a parallel local copy
   of a Win32 enum/struct that CsWin32 already projects.
3. Call the generated method directly &mdash; no thin wrappers.
4. **Check `PInvoke` (Touki) first, then `PInvokeMadowaku` (this repo)
   before adding anything to [NativeMethods.txt](../../../madowaku/NativeMethods.txt).**
   See "Two PInvoke classes" below.
5. Use `[LibraryImport]` (with `#if NET`) only for non-Win32 native calls
   (e.g. `libc`); on net472 fall back to `[DllImport]` on the same partial
   method. Win32 calls always go through CsWin32.

## Two PInvoke classes

CsWin32 generates per-assembly. Touki already runs CsWin32 to produce its
own `Windows.Win32.PInvoke` class with the APIs Touki needs internally, and
madowaku runs CsWin32 again to produce `Windows.Win32.PInvokeMadowaku`
(configured via `className` in
[NativeMethods.json](../../../madowaku/NativeMethods.json)). Both classes
live in the same `Windows.Win32` namespace and are both visible to callers
in this repo.

Order of operations before adding to `NativeMethods.txt`:

1. Look for the API on `Windows.Win32.PInvoke` (Touki).
2. If not present, look for it on `Windows.Win32.PInvokeMadowaku` (already
   added to our `NativeMethods.txt` in some earlier change).
3. Only if neither has it, add the name to
   [NativeMethods.txt](../../../madowaku/NativeMethods.txt) and let CsWin32
   project it into `PInvokeMadowaku`. Verify the generated file under
   `artifacts/obj/madowaku/.../generated/Microsoft.Windows.CsWin32/...` for
   the exact projected shape.

Same rule applies to **constants, enums, and Win32 structs** &mdash; they're
emitted next to the API that pulls them in. A constant like `S_OK` or an
enum like `CLSCTX` may already be projected by Touki; reuse it instead of
re-listing.

This split exists today only because CsWin32 does not yet support extension
type syntax to merge per-assembly partials &mdash; see
[microsoft/CsWin32#1477](https://github.com/microsoft/CsWin32/issues/1477).
Once CsWin32 ships that, both classes will collapse into one `PInvoke` and
this rule simplifies.

## TFM gating

madowaku targets `net472` + modern .NET. The relevant guards are:

| Guard | Use for |
| --- | --- |
| (none) | Code that compiles on every TFM. Default. Most CsWin32 surface lives here. |
| `#if NET` | Members that depend on .NET 7+ language/runtime features (static abstract interface members, `delegate*` `unmanaged` calling conventions only available there, real `ComWrappers`, `[GeneratedComInterface]`, etc.). |
| `#if NETFRAMEWORK` | Code that exists **only** for net472 (rare &mdash; usually a polyfill). Lives under `madowaku/Framework/`, which is already net472-only via `DefaultItemExcludes`, so an explicit `#if` inside that tree is redundant. Don't add it. |

Namespace imports for net472-only types must live inside the same guard.

## Polyfill layout

**.NET BCL polyfills come from Touki, not from this repo.** If a modern
`System.*` / `System.Buffers.*` / `System.Runtime.*` API is missing on net472,
the fix is to add the polyfill upstream in
[KlutzyNinja.Touki](https://github.com/JeremyKuhne/touki) (under
`touki/Framework/Polyfills/<BclNamespace>/`) and bump the package reference
here. Do **not** hand-roll cross-platform BCL polyfills in this repo just
because Touki doesn't ship one yet &mdash; file an issue / PR there instead.

The narrow exception is **Windows-only polyfills**: types that exist only
because of a Windows-specific surface (Win32 / COM / Registry / CsWin32
shims) and have no business in a general-purpose cross-platform polyfill
package. Those live here, under `madowaku/Framework/<BclNamespace>/...`,
compiled only for `$(DotNetFrameworkVersion)` via `DefaultItemExcludes` in
[madowaku.csproj](../../../madowaku/madowaku.csproj). The folder name mirrors
the BCL namespace being polyfilled (e.g.
`Framework/System/Runtime/InteropServices` for the CsWin32 `ComWrappers`
shim).

Touki-specific code that is **not** a polyfill of a modern .NET API does not
belong under `Framework/` and should not appear in this repo at all.

Source preference order:

1. Microsoft-shipped package (`System.Memory`, `Microsoft.Bcl.Memory`,
   `Microsoft.IO.Redist`, `Microsoft.Bcl.HashCode`).
2. `KlutzyNinja.Touki` &mdash; for both the helpers it ships
   (`Touki.Interop.HandleRef<T>`, `Touki.Text.*`, etc.) and any cross-platform
   BCL polyfill that fits there.
3. PolySharp source-gen for compiler attributes (this repo doesn't use it
   today; pull in only with a real caller).
4. Hand-rolled polyfill in `madowaku/Framework/...` &mdash; **last resort, and
   only when the polyfill is Windows-specific** (e.g. CsWin32 / COM / Registry
   shims). Cross-platform BCL gaps go upstream to Touki.

## ComWrappers polyfill (net472)

CsWin32 unconditionally emits `Windows.Win32.ComHelpers.UnwrapCCW<T1, T2>`
which references `System.Runtime.InteropServices.ComWrappers.ComInterfaceDispatch`.
That type exists only on .NET 5+. Because this shim exists **only** to
satisfy a Windows COM source generator &mdash; nobody on a non-Windows host
would consume it &mdash; it qualifies as a Windows-only polyfill and lives in
this repo at
[madowaku/Framework/System/Runtime/InteropServices/ComWrappers.cs](../../../madowaku/Framework/System/Runtime/InteropServices/ComWrappers.cs)
rather than upstream in Touki. The shim's `GetInstance<T>` always returns
`null`; callers using `UnwrapCCW` on net472 will see `COR_E_OBJECTDISPOSED`.
Don't expose CCW-based managed-to-COM scenarios on net472 &mdash; if you need
that, gate the public surface with `#if NET`.

## Type Conversions

- `HANDLE` &harr; `nint`: `(HANDLE)ptr` / `(nint)h.Value`. Sentinels:
  `HANDLE.Null`, `HANDLE.INVALID_HANDLE_VALUE`.
- `HMODULE` likewise; ours has helpers in
  [HMODULE.cs](../../../madowaku/Windows/Win32/Foundation/HMODULE.cs).
- `BOOL` is a struct &mdash; use the implicit conversion to `bool`, never
  raw `int`/`uint`.
- `HRESULT` &mdash; check `Failed` / `Succeeded`; cast to `int` only at the
  boundary with non-CsWin32 APIs (e.g. `new Win32Exception(hr.Value)`).
- Enum flags: use the Touki enum extensions (`AreFlagsSet`,
  `IsOnlyOneFlagSet`, `AreAnyFlagsSet`, `SetFlags`, `ClearFlags`). Never
  call `Enum.HasFlag` &mdash; it boxes on net472.

## Generated COM types and `ComScope<T>`

`ComScope<T>` (from CsWin32) is a `ref struct` for COM lifetime. It works on
**every** TFM, including net472, **only because** of the per-struct `IComIID`
polyfill described in the
[`cswin32-com`](../cswin32-com/SKILL.md) skill. Read that before adding a
COM type to `NativeMethods.txt`.

## Native integers

Always use `nint` / `nuint` for native integer types, never `IntPtr` /
`UIntPtr`. `nint` does **not** implement `IEquatable<nint>` on net472, so
generic constraints like `where T : unmanaged, IEquatable<T>` cannot be
instantiated with `nint` for cross-TFM call sites &mdash; pick a concrete
value type or split the path with `#if NET`.

## Verification

Always run a release build across all TFMs before pushing &mdash; net472
RyuJIT and Release-mode inlining surface bugs Debug doesn't.

```pwsh
dotnet build -c Release
dotnet test  -c Release
```
