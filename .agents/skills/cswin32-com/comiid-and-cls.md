# IComIID across target frameworks, and CLS compliance

Detail for [cswin32-com](SKILL.md). `IComIID` is the interface CsWin32 uses to
recover a COM type's IID generically - `IID.Get<T>()` reads a type's
`IComIID.Guid`. Its shape differs by target framework, and how much you author by
hand depends on the CsWin32 version.

## Modern CsWin32 (0.3.287 and later)

The generator emits `IComIID` and attaches it to **every** generated COM struct
on **all** target frameworks - a static-abstract `static ref readonly Guid Guid`
on .NET 7+, an instance `ref readonly Guid Guid` on the old framework leg or
`netstandard2.0`. Nothing to hand-author: adding a generated COM type to
`NativeMethods.txt` carries `IComIID` through `ComScope<T>` automatically on every
TFM. `IID.Get<T>()` reads it via `T.Guid` on modern .NET and `default(T).Guid` on
the old leg.

Only **manual** structs implement `IComIID` by hand, matching the per-TFM shape
(see [manual-structs.md](manual-structs.md)).

## Down-level CsWin32 (before 0.3.287) or netstandard

Older generators emit the static-abstract `IComIID` and attach it to generated
structs **only on .NET 7+**. On the old framework leg you supply the missing
pieces by hand, both gated `#if !NET` so they vanish on modern .NET:

- The `IComIID` interface itself (an instance-based `ref readonly Guid Guid`).
- One `partial struct` per generated COM type used through `ComScope<T>`, adding
  `IComIID` to its base list and returning the CsWin32-emitted `IID_Guid` field:

```csharp
namespace Windows.Win32.System.Com;

internal partial struct IRunningObjectTable : IComIID
{
    readonly ref readonly Guid IComIID.Guid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef(in IID_Guid);   // generator-emitted, always present
    }
}
```

Add a partial for each new generated COM type you scope. A **manual** struct is
not given the instance polyfill (it would force an interface dispatch on a hot
path); a manual struct that must work down-level spells out the instance member
itself, as in [manual-structs.md](manual-structs.md). Whether a repo is on the
modern or the down-level story - and where the polyfill files live - is
repo-specific; **see the overlay.** Upgrading the generator past 0.3.287 lets you
delete the whole polyfill.

## CLS compliance

A CLS-compliant assembly (`[assembly: CLSCompliant(true)]`) trips `CS3016` on
generated COM wrappers that carry CCW thunks (the `[UnmanagedCallersOnly(...)]`
array argument). Modern CsWin32 (0.3.287+) auto-emits `[CLSCompliant(false)]` on
those wrappers and adds `CS3019` / `CS3021` to its own generated-file
`#pragma warning disable` list, so the assembly builds clean with no consumer
action. On an older generator you supply a hand-authored `[CLSCompliant(false)]`
partial per generated COM type, or an assembly-level suppression - see the
overlay. Do not add per-type partials or a blanket `NoWarn` on a modern
generator. Reference:
[dotnet/roslyn#68526](https://github.com/dotnet/roslyn/issues/68526).
