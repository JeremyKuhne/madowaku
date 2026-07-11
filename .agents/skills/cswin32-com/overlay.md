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

## madowaku is on the modern IComIID story

madowaku pins **CsWin32 0.3.298** (see
[Directory.Packages.props](../../../Directory.Packages.props)), which is **past
0.3.287**, so the generator emits `IComIID` on **all** TFMs and attaches it to
every generated COM struct automatically - the modern path in
[comiid-and-cls.md](comiid-and-cls.md). There is **no** hand-authored `IComIID`
polyfill: adding a generated COM type to
[NativeMethods.txt](../../../madowaku/NativeMethods.txt) carries `IComIID` through
`ComScope<T>` on net472 with nothing extra. `IID.Get<T>()`
([IID.cs](../../../madowaku/Windows/Win32/Foundation/IID.cs)) reads it via
`T.Guid` on modern .NET and `default(T).Guid` on net472.

## Activation and lifetime helpers (this repo)

- **Class factory:**
  [ComClassFactory](../../../madowaku/Windows/Win32/System/Com/ComClassFactory.cs)
  (activate via `CoGetClassObject` then `CreateInstance`).
- **Field-lifetime holder:** the core's GIT-backed holder is
  [AgileComPointer](../../../madowaku/Windows/Win32/System/Com/AgileComPointer.cs),
  backed by
  [GlobalInterfaceTable](../../../madowaku/Windows/Win32/System/Com/GlobalInterfaceTable.cs).
- **Activation call:** `PInvokeMadowaku.CoCreateInstance` /
  `PInvokeMadowaku.CoGetClassObject` - but check Touki's `PInvoke` first (see the
  [cswin32-interop](../cswin32-interop/overlay.md) overlay's "Two PInvoke
  classes").
- **`BSTR`** is extended with `IDisposable` at
  [madowaku/Windows/Win32/Foundation/BSTR.cs](../../../madowaku/Windows/Win32/Foundation/BSTR.cs);
  [SafeArrayScope](../../../madowaku/Windows/Win32/System/Com/SafeArrayScope.cs)
  and friends sit beside the COM helpers.

## Manual structs and CLS compliance

- **Manual COM structs** for interfaces not in Win32 metadata (e.g. CLR
  hosting / metadata such as `ICLRMetaHost`, `ICLRRuntimeInfo`) go in their own
  files. A manual struct that must work on net472 spells out **both** `IComIID`
  arms itself (static-abstract under `#if NET`, instance under `#else`) - the
  generator attaches `IComIID` to *generated* structs, not manual ones.
- **CLS compliance:** madowaku does not assert `[assembly: CLSCompliant(true)]`,
  so `CS3016` on generated COM wrappers never arises and no suppression is needed.
  (On 0.3.287+ the generator also auto-emits `[CLSCompliant(false)]` for a
  consumer that does assert CLS compliance.)

## CCW is not available on net472

The `ComWrappers` shim (see the [cswin32-interop](../cswin32-interop/overlay.md)
overlay) returns `null`, so CCW-based managed-to-COM (`UnwrapCCW`) yields
`COR_E_OBJECTDISPOSED` on net472. Gate any such surface `#if NET`. This also means
the core's CCW-based test-mocking bridge is a modern-.NET-only technique here.

## Cross-references

- [`cswin32-interop`](../cswin32-interop/SKILL.md) - the P/Invoke layer and
  blittable-signature rules.
- [`security-review`](../security-review/SKILL.md) - the unsafe / pointer COM
  surface this skill produces.
