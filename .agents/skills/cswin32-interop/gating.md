# Gating and guarded builds

Detail for [cswin32-interop](SKILL.md). CsWin32 output is Windows-only. How you
gate it depends on whether the library is cross-platform / source-buildable or
Windows-only but multi-targeted.

## The two cross-cuts

- **Platform.** A cross-platform or source-buildable library compiles its
  Windows-interop code only behind a repo-specific compile symbol (the
  "Windows-interop gate") and takes a runtime `IsWindows` check on any code path
  that also has a non-Windows branch. Both are required.
- **TFM.** A Windows-only library that multi-targets frameworks has no platform
  gate; the cross-cut is target framework (for example an older `net472` /
  `netstandard` leg versus a modern .NET leg). Guard only the members that
  depend on a newer language or runtime feature.

Your overlay names the concrete gate, or states there is none. Keep this core's
guidance gate-name-agnostic.

## Nest the guards in the right order

```csharp
#if WINDOWS_INTEROP_GATE
    if (IsWindows)
    {
        PInvoke.GetFileAttributesEx(fullPath, out WIN32_FILE_ATTRIBUTE_DATA data);
    }
#endif
    // cross-platform fallback
```

The compile gate is **outside**, the runtime check **inside**. The inverse -
`if (IsWindows) { #if ... #endif }` - leaves dead code (or a missing return
value) when the symbol is undefined. A file that is entirely Windows-only is
better excluded from the non-Windows build via `<Compile Remove>` than wrapped
in a whole-file `#if`.

## Guard selection

| Guard | Use for | Runtime OS check |
| --- | --- | --- |
| Windows-interop gate only | Multi-TFM Windows calls present on every framework | Yes |
| gate plus "modern-only" (`&& NET`) | Members needing .NET 7+ features: static-abstract interface members, some `delegate*` conventions, real `ComWrappers` | Yes |
| gate plus "not netstandard" | CsWin32 types unavailable on `netstandard` | Yes |
| framework-only (`#if !NET`) | Code that exists only for the old framework leg, usually a polyfill - inherently Windows | No |

Namespace `using` directives for gated types must live inside the same guard.

## CA1416 platform compatibility

For a cross-platform assembly, satisfy the analyzer semantically rather than
suppressing it:

- `if (IsWindows)` satisfies `[SupportedOSPlatform]`; `if (IsUnixLike)`
  satisfies `[UnsupportedOSPlatform("windows")]`. **Never** use `!IsWindows` -
  use an explicit `else if (IsUnixLike)`.
- Put a versioned `[SupportedOSPlatform("windows6.1")]` on methods that call
  CsWin32 APIs. `CS0592` forbids the attribute on a `partial struct` - put it on
  individual members instead.
- Reserve `#pragma warning disable CA1416` for static local functions, which the
  analyzer cannot flow into.

## A stack-first scratch buffer

Win32 string and buffer APIs want a caller-allocated span with a length retry. A
stack-first buffer with an `ArrayPool<T>` fallback keeps the common case
allocation-free:

```csharp
using BufferScope<char> buffer = new(stackalloc char[(int)PInvoke.MAX_PATH]);
int length = (int)PInvoke.GetShortPathName(path, buffer.AsSpan());
if (length > buffer.Length)
{
    buffer.EnsureCapacity(length);
    length = (int)PInvoke.GetShortPathName(path, buffer.AsSpan());
}
```

Always `using` it (it is a `ref struct`), never `stackalloc` more than about
1024 bytes, and check for a CsWin32 convenience overload (e.g.
`GetShortPathName(string, Span<char>)`) before writing a `fixed` block. The
`scratch-buffer-strategy` skill covers choosing the strategy; the concrete
buffer type and its location are repo-specific - see the overlay.

## Verify the guarded build

Anything referenced **only** inside the Windows-interop gate must itself be
inside it, or the build that undefines the symbol breaks - most often on
warnings-as-errors. Watch for `IDE0005` (unused `using`), `IDE0051` / `IDE0052`
(unused private members), `CA1823` (an unused private field, e.g. a constant
read only inside the guard), and `CS1587` (an XML doc comment left before a
now-gated member). The same applies to a polyfill behind `#if !NET` consumed
only from gated code. Build **both** configurations - symbol defined and
undefined - before pushing.
