---
core: cswin32-interop
core-pin: pending-promotion
---

# madowaku overlay - cswin32-interop

Repository-specific companion to the [cswin32-interop](SKILL.md) core. The
`SKILL.md` and its sibling pages are a **portable core** reconciled from
madowaku's and dotnet/msbuild's CsWin32 skills, authored here so it can be
promoted to the shared commons. It is **not yet vendored**, so it carries no
`github-*` provenance and `core-pin` is `pending-promotion`; the promotion change
will vendor it and add a real pin. Keep the core generic - everything
madowaku-specific lives here.

## The Windows-interop gate: there isn't one

madowaku is a **Windows-only** library, so there is no `FEATURE_WINDOWSINTEROP`
symbol, no source build, and no `IsWindows` runtime split. The cross-cut is
purely **TFM**: code must compile on `net472`, `net10.0`, and
`net10.0-windows10.0.22000.0` (see [AGENTS.md](../../../AGENTS.md)). Use `#if NET`
only for members that need a .NET 7+ feature; net472-only code lives under
[madowaku/Framework/](../../../madowaku/Framework/), which is net472-scoped by
`DefaultItemExcludes`, so no `#if NETFRAMEWORK` goes inside it. `CA1416` does not
arise the way it does in a cross-platform repo.

## Two PInvoke classes (Touki plus this repo)

CsWin32 generates per assembly. Touki already runs it to produce
`Windows.Win32.PInvoke`; madowaku runs it again to produce
`Windows.Win32.PInvokeMadowaku` (the `className` in
[NativeMethods.json](../../../madowaku/NativeMethods.json)). Both live in the
`Windows.Win32` namespace and are visible here. Before adding a name to
[NativeMethods.txt](../../../madowaku/NativeMethods.txt):

1. Look for the API / constant / enum on `PInvoke` (Touki).
2. Then on `PInvokeMadowaku` (this repo, from an earlier change).
3. Only if neither has it, add it and let CsWin32 project it into
   `PInvokeMadowaku`; verify the generated file under
   `artifacts/obj/madowaku/.../generated/Microsoft.Windows.CsWin32/...`.

The split exists only until CsWin32 supports extension-type merging
([microsoft/CsWin32#1477](https://github.com/microsoft/CsWin32/issues/1477)).

## Polyfills: Touki first

BCL polyfills come from the `KlutzyNinja.Touki` package, not this repo. If a
modern `System.*` API is missing on net472, add it upstream in Touki, not here.
The narrow exception is **Windows-only** shims (Win32 / COM / CsWin32), which live
under [madowaku/Framework/](../../../madowaku/Framework/), the folder mirror of
the BCL namespace being polyfilled. Source preference: a Microsoft package, then
`KlutzyNinja.Touki`, then a hand-rolled Windows-only shim under `Framework/` as a
last resort. The core's stack-first buffer is Touki's `BufferScope<T>` (from the
`KlutzyNinja.Touki` package).

## ComWrappers / CCW polyfill (net472)

CsWin32 unconditionally emits `Windows.Win32.ComHelpers.UnwrapCCW<T1, T2>`, which
references `ComWrappers.ComInterfaceDispatch` - a .NET 5+ type. madowaku ships a
Windows-only shim at
[madowaku/Framework/System/Runtime/InteropServices/ComWrappers.cs](../../../madowaku/Framework/System/Runtime/InteropServices/ComWrappers.cs)
whose `GetInstance<T>` always returns `null`, so `UnwrapCCW` on net472 yields
`COR_E_OBJECTDISPOSED`. Do not expose CCW-based managed-to-COM scenarios on
net472; gate that surface `#if NET`. See the
[cswin32-com](../cswin32-com/overlay.md) overlay.

## Concrete bindings

- **Enum flags:** use the Touki enum extensions (`AreFlagsSet`,
  `IsOnlyOneFlagSet`, `AreAnyFlagsSet`, `SetFlags`, `ClearFlags`) - never
  `Enum.HasFlag` (it boxes on net472).
- **`HMODULE`** has helpers at
  [madowaku/Windows/Win32/Foundation/HMODULE.cs](../../../madowaku/Windows/Win32/Foundation/HMODULE.cs).
- **Verification:** `dotnet build -c Release` and `dotnet test -c Release` across
  all TFMs before pushing - net472 RyuJIT and Release-mode inlining surface bugs
  Debug does not.

## Cross-references

- [`cswin32-com`](../cswin32-com/SKILL.md) - the struct-based COM layer.
- [`dotnet-polyfills`](../dotnet-polyfills/SKILL.md) - the downlevel polyfill
  stack behind the "Touki first" policy.
