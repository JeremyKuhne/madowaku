# Manual COM structs

Detail for [cswin32-com](SKILL.md). For an interface not in Win32 metadata (WMI,
Fusion, Setup Configuration, CLR hosting / metadata), hand-write a struct that
lays out the vtable yourself. Put each interface in its own file, excluded from
any source-only or non-Windows build.

## Shape

```csharp
internal unsafe struct IAssemblyCache : IComIID
{
    public static readonly Guid IID_IAssemblyCache = new(0xE707DCDE, ...);

#if NET
    static ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef(in IID_IAssemblyCache);
    }
#else
    readonly ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef(in IID_IAssemblyCache);
    }
#endif

    private readonly void** _lpVtbl;

    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        fixed (IAssemblyCache* pThis = &this)
            return ((delegate* unmanaged[Stdcall]<IAssemblyCache*, Guid*, void**, HRESULT>)
                _lpVtbl[0])(pThis, riid, ppvObject);
    }
    // AddRef @1, Release @2, then interface methods at their slot indices.
}
```

## Rules

- **`delegate* unmanaged[Stdcall]`** for the function-pointer cast. IDL
  `STDMETHODCALLTYPE` is `__stdcall` on Win32; the wrong convention silently
  corrupts the stack. This works on the old framework leg too (it lowers to an IL
  `calli`).
- **Vtable slots are exact and IUnknown-relative:** slot 0 = `QueryInterface`,
  1 = `AddRef`, 2 = `Release`, 3+ = interface methods in IDL order. When the
  interface derives from another, **add the parent's method count** first (e.g. a
  `v2` method after three `IUnknown` and three `v1` methods is at slot 6). Unused
  slots may be omitted as long as the ones you call have the correct index.
- **`IComIID` has two shapes** - a static-abstract `static ref readonly Guid
  IComIID.Guid` on .NET 7+, and an instance `readonly ref readonly Guid
  IComIID.Guid` down-level. A dual-target manual struct must spell out **both**
  arms (`#if NET` / `#else`); a modern-only struct drops the `#else` and gates the
  whole file `#if NET`. CsWin32 handles this automatically for *generated*
  structs - see [comiid-and-cls.md](comiid-and-cls.md).
- **Use the generated `PCWSTR` / `PWSTR`** for wide-string parameters (add them
  to `NativeMethods.txt`); raw `char*` with `fixed` only where no typed
  equivalent exists.
- **Vtable methods are blittable** - the same rules as any `[DllImport]`; see the
  blittable-signatures page of the paired cswin32-interop skill (`HRESULT`
  return, `T**` not `out T*`, `void*`, typed enum parameters).
- **`CS0592`** forbids `[SupportedOSPlatform]` on a struct - put it on individual
  methods.

## Strongly-typed handle and token wrappers

When the native side `typedef`s a primitive into a family of "same shape,
different meaning" aliases (e.g. `typedef ULONG32 mdToken; typedef mdToken
mdAssembly; ...`), mirror the hierarchy with distinct `readonly struct` wrappers,
each holding the single underlying primitive - the same pattern as CsWin32's
`HANDLE` / `HWND` / `HMODULE`. It stays blittable, so `delegate*` casts and arrays
cost nothing at the boundary. Conversions follow the typedef hierarchy:
**implicit** widening to the base (always safe), **explicit** narrowing (opt-in,
because the C side cannot enforce the kind at the cast site). Check the native
header for the canonical validation primitives before writing `IsNil` / `IsValid`
- the encoding often hides in macros (for `mdToken`, `IsNilToken` tests the rid
half, `RidFromToken(tk) == 0`, not the whole value, and a per-type nil such as
`mdAssemblyNil = 0x20000000` is the table tag, not `0`).
