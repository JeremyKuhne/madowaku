# Lifetime and access

Detail for [cswin32-com](SKILL.md). Struct-based COM has no runtime to release
references for you; scope every pointer.

## Where a COM pointer may live

A raw `T*` (a CsWin32 COM struct pointer) is allowed only as a local in an
`unsafe` method, a parameter, or a field of a `ref struct`. **A raw `T*` field on
a `class` or non-`ref` `struct` is forbidden** - it is an apartment-agility
hazard and leaks its reference if the owner is finalized without disposing. Use
the field-lifetime holder below instead.

## ComScope<T> - transient pointers

A `ref struct`, always `using`'d, that calls `Release` on dispose. **The default
for everything that does not outlive the current method.** It implicitly converts
to `T**` and `void**`, so an out-param writes straight into the scope:

```csharp
using ComScope<ISomething> scope = new();
Guid iid = IID.Get<ISomething>();
factory.Pointer->QueryInterface(&iid, scope).ThrowOnFailure();
scope.Pointer->DoThing(...);
```

- Access methods via `scope.Pointer->Method(...)`; null-check with
  `scope.IsNull`.
- Applies to *every* COM out-param - `CoCreateInstance`, `QueryInterface`,
  `IEnumXxx::Next`, a factory method, an app-local `STDAPI Get*` (declare the
  `[DllImport]` with `T** pp`, not `out T*`, so the implicit operator binds).

## Ownership transfer out of a helper

A helper that acquires a pointer can return `ComScope<T>` directly; intermediate
pointers stay in their own `using` scopes and release on the helper's `return`. A
`default` `ComScope<T>` is null and its `Dispose` is a no-op, so callers need no
extra null guard:

```csharp
private static ComScope<ISomething> Acquire()
{
    ComScope<ISomething> result = new();
    Guid clsid = SomeCoClass.CLSID;
    HRESULT hr = PInvoke.CoCreateInstance(
        &clsid, null, CLSCTX.CLSCTX_INPROC_SERVER, IID.Get<ISomething>(), result);
    return hr.Succeeded ? result : default;   // ownership flows to the caller
}
```

## BSTR - COM strings

A COM `BSTR` out-param is owned by the caller and must be freed with
`SysFreeString` (equivalently `Marshal.FreeBSTR`). CsWin32 generates `BSTR` as a
plain value - it does **not** implement `IDisposable` - so free it explicitly:

```csharp
BSTR version = default;
try
{
    instance.Pointer->GetVersion(&version).ThrowOnFailure();
    string managed = version.ToString();
}
finally
{
    Marshal.FreeBSTR((nint)version.Value);
}
```

A repo may extend `BSTR` with `IDisposable` (a `Dispose` that calls
`SysFreeString` / `FreeBSTR` and clears the field), enabling the same scoped
shape as `ComScope<T>` - `using BSTR version = default;` - the recommended
convenience where available. Because such a `Dispose` writes through the storage
in place, the `using` must be on the storage location, not a method-returned
copy.

## A COM pointer that outlives a method

When a COM pointer must be stored in a class field (the only legal way to keep
one on a non-`ref` type), use a finalizable managed holder that registers the
pointer in the Global Interface Table (thread-agile) and releases it on dispose,
with the finalizer as a safety net. Access it by round-tripping a short-lived
`ComScope<T>` back out of the holder for the duration of a call; hoist one scope
to the top of a method when several calls share it. When the source pointer is
already owned by a `ComScope<T>` that will release, register **without** taking
ownership (the GIT adds its own reference); take ownership only when no other code
path will release. The concrete holder type is repo-specific - see the overlay.
