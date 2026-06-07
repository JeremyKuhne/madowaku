---
name: cswin32-com
description: 'Guides struct-based COM interop in madowaku using CsWin32 patterns. Consult when working with ComScope<T>, IComIID, IID.Get<T>(), delegate* unmanaged vtables, CoCreateInstance / CoGetClassObject, the per-struct IComIID net472 polyfill, or manually defining COM interfaces not in Win32 metadata (e.g. CLR hosting ICLRMetaHost, ICLRRuntimeInfo).'
argument-hint: 'Describe the COM interface or activation pattern you are working with.'
---

# CsWin32 COM Interop Guide (madowaku)

Struct-based COM interop using CsWin32 patterns &mdash; AOT-compatible, no
`[ComImport]` and no built-in CLR marshalling. Adapted from
[dotnet/msbuild &mdash; cswin32-com](https://github.com/dotnet/msbuild/blob/main/.github/skills/cswin32-com/SKILL.md);
trimmed to madowaku's single-library, Windows-only layout.

For the broader CsWin32 rules (TFM gating, polyfill layout, `ComWrappers`
shim) see the [`cswin32-interop`](../cswin32-interop/SKILL.md) skill.

## Workflow

1. Determine if the interface is in Win32 metadata.
   - **Yes** &mdash; add the name to
     [NativeMethods.txt](../../../madowaku/NativeMethods.txt). CsWin32 will
     generate it.
   - **No** (e.g. CLR hosting interfaces like `ICLRMetaHost`,
     `ICLRRuntimeInfo`) &mdash; define a manual struct, see below.
2. Use `ComScope<T>` for lifetime: `using ComScope<T> scope = new();`.
3. Activate via `PInvoke.CoCreateInstance(...)` (Touki) or
   `PInvokeMadowaku.CoCreateInstance(...)` (this repo) with `IID.Get<T>()`,
   or via a class factory helper. Check Touki's `PInvoke` for the API first
   &mdash; see "Two PInvoke classes" in the
   [`cswin32-interop`](../cswin32-interop/SKILL.md) skill.
4. Call methods via `scope.Pointer->Method(...)`. Pass `ComScope<T>` directly
   as a `T**` / `void**` output parameter (implicit conversion).
5. Gate manual structs with `#if NET` when they rely on static-abstract
   `IComIID`. CsWin32-generated COM types work on net472 via the per-struct
   polyfill (next section).

## `IComIID` polyfill (net472)

CsWin32 emits the `IComIID` interface (with a static-abstract `Guid`) and
attaches it to every generated COM struct **only on .NET 7+**. On net472:

- The `IComIID` interface itself is missing &mdash; we ship an instance-based
  version at
  [madowaku/Framework/Windows/Win32/IComIID.cs](../../../madowaku/Framework/Windows/Win32/IComIID.cs).
- Generated COM structs do not have `IComIID` in their base list &mdash; we
  ship per-struct partials in
  [madowaku/Framework/Windows/Win32/System/Com/](../../../madowaku/Framework/Windows/Win32/System/Com/)
  that add it. `IUnknown.cs` is the canonical example:

  ```csharp
  namespace Windows.Win32.System.Com;

  public unsafe partial struct IUnknown : IComIID
  {
      readonly ref readonly Guid IComIID.Guid
      {
          [MethodImpl(MethodImplOptions.AggressiveInlining)]
          get => ref Unsafe.AsRef(in IID_Guid); // CsWin32-emitted field, always present
      }
  }
  ```

When you add a CsWin32-generated COM type that you want to use through
`ComScope<T>` on net472, add a partial to that folder. Both files live under
`Framework/`, so they compile **only** for net472 &mdash; do not add `#if`
guards inside; the project file does the gating.

> Pattern source:
> [winforms IDataObject.cs](https://github.com/dotnet/winforms/blob/main/src/System.Private.Windows.Core/src/Framework/Windows/Win32/System/Com/IDataObject.cs).

## COM interfaces in Win32 metadata

Add the interface name to `NativeMethods.txt` &rarr; CsWin32 generates it
&rarr; use `ComScope<T>`:

```csharp
using ComScope<IClassFactory> factory = new();
HRESULT hr = PInvokeMadowaku.CoGetClassObject(
    &clsid,
    CLSCTX.CLSCTX_INPROC_SERVER,
    null,
    IID.Get<IClassFactory>(),
    factory);
if (hr.Failed)
{
    return hr;
}

factory.Pointer->CreateInstance(...);
```

## Manual COM structs (not in Win32 metadata)

For interfaces not in Win32 metadata (CLR hosting, debugging, custom
out-of-band COM APIs), define struct-based implementations under
`madowaku/DotNet/...` or a peer folder, in their own files.

```csharp
[SupportedOSPlatform("windows6.1")]
internal unsafe struct ICLRMetaHost : IComIID
{
    public static Guid Guid { get; } = new(0x9280188D, ...);

#if NET
    static ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ReadOnlySpan<byte> data = [ /* 16 GUID bytes */ ];
            return ref Unsafe.As<byte, Guid>(ref MemoryMarshal.GetReference(data));
        }
    }
#endif

    private readonly void** _lpVtbl;

    // IUnknown (vtable 0-2) + interface methods at correct indices.
    public HRESULT GetRuntime(char* version, Guid* iid, void** runtimeInfo)
    {
        fixed (ICLRMetaHost* pThis = &this)
        {
            return ((delegate* unmanaged[Stdcall]<ICLRMetaHost*, char*, Guid*, void**, HRESULT>)
                _lpVtbl[3])(pThis, version, iid, runtimeInfo);
        }
    }

    public static Guid CLSID { get; } = new(0x9280188D, ...);
}
```

Requirements:

- `delegate* unmanaged[Stdcall]` &mdash; works on net472 too (C# 9 / IL
  `calli`).
- Static-abstract `IComIID` member &mdash; .NET 7+ only; gate that block with
  `#if NET`. The instance-based polyfill is intentionally **not** attached to
  manual structs because it forces an interface dispatch on a hot path.
  Manual structs that need to participate on net472 must implement the
  instance member explicitly, mirroring the per-struct partials for
  generated types.
- Exact vtable indices &mdash; unused slots can be omitted as long as the
  ones you call are correct.
- `char*` with `fixed` for `BSTR` / wide-string parameters.
- `[CLSCompliant(false)]` is generally required on the file (or
  `<assembly: CLSCompliant(false)>` in `AssemblyInfo`); CsWin32 emits its
  generated COM types as non-CLS by default. See the existing
  [GlobalSuppressions.cs](../../../madowaku/GlobalSuppressions.cs).

## Activation

```csharp
// Via CoCreateInstance &mdash; use IID.Get<T>() for the IID.
Guid clsid = ICLRMetaHost.CLSID;
using ComScope<ICLRMetaHost> host = new();
HRESULT hr = PInvokeMadowaku.CoCreateInstance(
    &clsid,
    null,
    CLSCTX.CLSCTX_INPROC_SERVER,
    IID.Get<ICLRMetaHost>(),
    host);
```

Key points:

- Use `IID.Get<T>()` &mdash; do not take `&localGuid`. On net472 this resolves
  via the instance polyfill on a temporary `default(T)`; on .NET 7+ it goes
  through the static-abstract member.
- Initialize `ComScope<T>` with `new()`. It implicitly converts to `T**` /
  `void**` output parameters.

## Lifetime & access

- `ComScope<T>` is a `ref struct` &mdash; always use with `using`. It calls
  `Release()` on dispose.
- Access methods via `scope.Pointer->Method(...)`.
- Pass `ComScope<T>` directly as `T**` or `void**` output parameter.

## File organization

| Path | Contents |
| --- | --- |
| `madowaku/Windows/Win32/System/Com/` | Hand-written helpers around generated COM types (e.g. `SafeArrayScope.cs`). |
| `madowaku/Framework/Windows/Win32/IComIID.cs` | net472 instance-based `IComIID` polyfill. |
| `madowaku/Framework/Windows/Win32/System/Com/IUnknown.cs` | net472 partial attaching `IComIID` to `IUnknown`. |
| `madowaku/Framework/Windows/Win32/System/Com/...` | Additional per-struct partials, one per CsWin32-generated COM type used through `ComScope<T>`. |
| `madowaku/DotNet/Hosting/`, `madowaku/DotNet/Metadata/` | Manual COM structs for CLR hosting / metadata APIs. |

## Gotchas

- **CS0592** prevents `[SupportedOSPlatform]` on a `partial struct`. Put the
  attribute on individual methods instead.
- **Don't use `Enum.HasFlag` &mdash; it boxes on net472.** Use the Touki
  enum extensions (`AreFlagsSet`, `IsOnlyOneFlagSet`, `AreAnyFlagsSet`,
  `SetFlags`, `ClearFlags`) for flag tests in COM hot paths.
- **Anonymous unions** in CsWin32 output show up as nested `Anonymous`
  members. Inspect the generated `*.g.cs` for the exact path.
- **CCW (`UnwrapCCW`) is not supported on net472** &mdash; see the
  `ComWrappers` polyfill section in the
  [`cswin32-interop`](../cswin32-interop/SKILL.md) skill. Don't expose
  CCW-based managed-to-COM scenarios on net472; gate them with `#if NET`.
