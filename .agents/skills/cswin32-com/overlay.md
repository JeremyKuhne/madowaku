---
core: cswin32-com
core-pin: pending-promotion
---

# madowaku overlay - cswin32-com

Repository-specific companion to the [cswin32-com](SKILL.md) core. The `SKILL.md`
and its sibling pages are a **portable core** reconciled from madowaku's and
dotnet/msbuild's CsWin32 skills, authored here so it can be promoted to the shared
commons. It is **not yet vendored** - no `github-*` provenance, and `core-pin` is
`pending-promotion` until the promotion change vendors it. Keep the core generic;
madowaku specifics live here.

## madowaku uses 0.3.287+ IComIID generation

madowaku pins **CsWin32 0.3.298** (see
[Directory.Packages.props](../../../Directory.Packages.props)), which is **past
0.3.287**, so the generator emits `IComIID` on all three madowaku library TFMs
and attaches it to every generated COM struct automatically - the 0.3.287+ path
in [comiid-and-cls.md](comiid-and-cls.md). There is **no** hand-authored
`IComIID` polyfill: adding a generated COM type to
[NativeMethods.txt](../../../madowaku/NativeMethods.txt) carries `IComIID` through
`ComScope<T>` on net472 with nothing extra. `IID.Get<T>()`
([IID.cs](../../../madowaku/Windows/Win32/Foundation/IID.cs)) reads it via
`T.Guid` on .NET 10 and `default(T).Guid` on net472.

Madowaku's `#if NET` branches select both .NET 10 legs, while its
`#if NETFRAMEWORK` branches select net472.

## Activation and lifetime helpers (this repo)

- **Owned-pointer scope and IID access:**
  [ComScope](../../../madowaku/Windows/Win32/System/Com/ComScope{T}.cs) owns one
  AddRef'd interface pointer and converts to `T**` / `void**` for output calls;
  [IID](../../../madowaku/Windows/Win32/Foundation/IID.cs) exposes the
  `Guid*`-returning `IID.Get<T>()` helper used by the core examples.
- **Class factory:**
  [ComClassFactory](../../../madowaku/Windows/Win32/System/Com/ComClassFactory.cs)
  (activate via `CoGetClassObject` then `CreateInstance`).
- **Cross-apartment field holder:** the core's GIT-backed strategy maps to
  [AgileComPointer](../../../madowaku/Windows/Win32/System/Com/AgileComPointer.cs),
  backed by
  [GlobalInterfaceTable](../../../madowaku/Windows/Win32/System/Com/GlobalInterfaceTable.cs).
- **Activation call:** `PInvoke.CoCreateInstance` /
  `PInvoke.CoGetClassObject` on madowaku's public generated surface (see the
  [cswin32-interop](../cswin32-interop/overlay.md) overlay's "One public PInvoke
  surface").
- **`BSTR`** is extended with `IDisposable` at
  [madowaku/Windows/Win32/Foundation/BSTR.cs](../../../madowaku/Windows/Win32/Foundation/BSTR.cs);
  [SafeArrayScope](../../../madowaku/Windows/Win32/System/Com/SafeArrayScope.cs)
  and friends sit beside the COM helpers.

## Owner-side CCW vtable hook

madowaku is the owner described by
[ccw-composition.md](ccw-composition.md). Its .NET 10 build implements the
`PopulateIUnknownImpl<TComInterface>` partial method declared by CsWin32's
generated `ComHelpers` in
[ComHelpers.cs](../../../madowaku/Windows/Win32/System/Com/ComHelpers.cs). The
implementation derives from `ComWrappers`, calls `GetIUnknownImpl`, and writes
the canonical `QueryInterface`, `AddRef`, and `Release` entry points into the
generated vtable.

Keep this hook in madowaku. A downstream extender's generated `IVTable` support
calls the owner assembly's `ComHelpers.PopulateIUnknown<T>()`; partial methods do
not span assemblies, so an implementation in the extender cannot satisfy the
owner declaration. The hook is `#if NET` because the net472 `ComWrappers` shim
cannot provide real CCW vtables. Shared imported COM structs and lifecycle
helpers likewise stay in madowaku; downstream packages add behavior with
extension blocks or use uniquely named implementation-only CCW provider types.

The thirtytwo migration exercised this boundary with a local `IDispatchCcw`
provider while runtime pointers remained madowaku's imported `IDispatch`. It
also exposed a recurring ownership rule: pointers returned by an extender's CCW
helper are AddRef'd caller references. Wrap them in madowaku `ComScope<T>` when
passing them to `Advise`, `SetClientSite`, or borrowed calls such as `DoVerb`.
For successful subscriptions, store the cookie and call `Unadvise` before
disposing the source pointer.

## Manual structs and CLS compliance

- **Manual COM structs** for interfaces not in Win32 metadata (e.g. CLR
  hosting / metadata such as `ICLRMetaHost`, `ICLRRuntimeInfo`) go in their own
  files. A manual struct that must work on net472 spells out **both** `IComIID`
  arms itself (static-abstract under `#if NET`, instance under `#else`) - the
  generator attaches `IComIID` to *generated* structs, not manual ones.
- **CLS compliance:** madowaku does not assert `[assembly: CLSCompliant(true)]`,
  so `CS3016` on its public generated COM wrappers does not arise and no
  suppression or per-type partial is needed. CsWin32 does not auto-annotate
  public wrappers; if madowaku adopts CLS compliance later, audit that public
  projection and mark intentionally non-CLS wrappers explicitly.

## `UnwrapCCW` is not available on net472

Classic COM-callable wrappers work on net472 - the core's test-mocking bridge
uses `Marshal.GetComInterfaceForObject`, which is fine here. What does **not**
work is CsWin32's `ComWrappers`-based `UnwrapCCW`: the `ComWrappers` shim (see the
[cswin32-interop](../cswin32-interop/overlay.md) overlay) returns `null`, so
`UnwrapCCW` yields `COR_E_OBJECTDISPOSED` on net472. Gate any surface that relies
on `UnwrapCCW` with `#if NET`.

## Cross-references

- [`cswin32-interop`](../cswin32-interop/SKILL.md) - the P/Invoke layer and
  blittable-signature rules.
- [`security-review`](../security-review/SKILL.md) - the unsafe / pointer COM
  surface this skill produces.
