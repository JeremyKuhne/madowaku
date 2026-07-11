---
name: cswin32-com
description: 'Guides struct-based COM interop using CsWin32 - AOT-compatible, no [ComImport] and no built-in CLR marshalling. Consult when working with ComScope, IID.Get, delegate* unmanaged vtables, CoCreateInstance or a class factory, IComIID across target frameworks, manually defining COM interfaces not in Win32 metadata (WMI, Fusion, CLR hosting/metadata), COM pointer lifetime in fields, or mocking struct-based COM in tests. Paired with the cswin32-interop skill for the general P/Invoke layer and blittable-signature rules.'
argument-hint: 'Describe the COM interface or activation pattern you are working with.'
license: MIT
compatibility: Requires the .NET SDK and the Microsoft.Windows.CsWin32 source generator; Windows COM APIs.
metadata:
  portability: portable
  applicability: dotnet
  binding: optional-overlay
  risk: local-write
  maturity: canary
  requires: none
  related: cswin32-interop, security-review, il-copy-inspection
---

# CsWin32 struct-based COM interop

If `overlay.md` exists beside this file, read it before acting; it contains
repository-specific bindings. This core remains usable without it.

Struct-based COM interop on top of CsWin32: raw `delegate*` vtable calls, no
`[ComImport]` and no built-in CLR marshalling, so it is AOT- and
trimming-friendly. This skill covers the COM-specific layer; the general P/Invoke
layer and the blittable-signature rules that vtable methods also follow live in
the paired **cswin32-interop** skill.

## Workflow

1. **In Win32 metadata?** If yes, add the interface name to `NativeMethods.txt`
   and CsWin32 generates it. If not (WMI, Fusion, Setup Configuration, CLR
   hosting / metadata), define a manual struct in its own file - see
   [manual-structs.md](manual-structs.md).
2. **Lifetime:** `using ComScope<T> scope = new();` for every transient COM
   pointer, and `using BSTR s = default;` for every COM `BSTR` out-param. Never
   write the pre-`ComScope` `T* p; try { ... } finally { p->Release(); }` shape;
   it leaks on every early return. See [lifetime.md](lifetime.md).
3. **Activate** via a class-factory helper or `CoCreateInstance` with
   `IID.Get<T>()` - not `&localGuid`. See [Activation](#activation).
4. **Call** via `scope.Pointer->Method(...)`. Pass `ComScope<T>` directly where
   the API expects `T**` / `void**`; the implicit operator takes the address.
5. **Match the caller's error contract.** If the top-level consumer treats COM
   failure as "absent", helpers return `default` / `false` rather than throwing a
   `COMException` that is immediately swallowed; otherwise call
   `.ThrowOnFailure()`. See the parity table in
   [migration-and-testing.md](migration-and-testing.md).
6. **Gate** by target framework only where a member needs a newer feature -
   [comiid-and-cls.md](comiid-and-cls.md) has the `IComIID` story that governs
   which TFMs `ComScope<T>` works on.

## Activation

```csharp
// Via CoCreateInstance - IID.Get<T>(), never &localGuid.
Guid clsid = SomeCoClass.CLSID;
using ComScope<ISomething> instance = new();
PInvoke.CoCreateInstance(
    &clsid, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.Get<ISomething>(), instance)
    .ThrowOnFailure();
instance.Pointer->DoThing(...);
```

- Use `IID.Get<T>()` for the IID - it reads the type's `IComIID.Guid`. Do not
  take `&localGuid`.
- Initialize `ComScope<T>` with `new()`; it implicitly converts to the `T**` /
  `void**` output parameter.
- A repo may also ship a class-factory helper (activate via `CoGetClassObject`
  then `CreateInstance`), the AOT-friendly path when a CLSID is registered - see
  the overlay.

## Sibling pages

- [lifetime.md](lifetime.md) - `ComScope<T>`, `BSTR`, the field-lifetime pointer
  holder, ownership transfer out of a helper, and the forbidden raw `T*` field.
- [manual-structs.md](manual-structs.md) - defining an interface not in Win32
  metadata: `delegate* unmanaged[Stdcall]` vtables, exact slot indices, the
  dual-target `IComIID` member, and strongly-typed handle/token wrappers.
- [comiid-and-cls.md](comiid-and-cls.md) - `IComIID` across target frameworks
  (modern auto-emit versus the down-level per-struct polyfill) and CLS
  compliance.
- [migration-and-testing.md](migration-and-testing.md) - the error-handling
  parity table when migrating off `[ComImport]`, and mocking struct-based COM in
  tests via a CCW bridge.
