---
applyTo: "madowaku/Windows/Win32/**/*.cs, madowaku/Framework/**/*.cs, madowaku/NativeMethods.txt, madowaku/NativeMethods.json"
---

# CsWin32 interop and net472 polyfill rules

Path-specific rules for hand-authored partials under `madowaku/Windows/Win32/`,
net472 polyfills under `madowaku/Framework/`, and the CsWin32 generator
inputs (`NativeMethods.txt`, `NativeMethods.json`). The general `AGENTS.md`
rules still apply.

For full patterns and recipes, consult:

- [`cswin32-interop`](../../.agents/skills/cswin32-interop/SKILL.md)
  &mdash; P/Invoke generation, TFM gating, `PInvoke` vs
  `PInvokeMadowaku`, the `ComWrappers` shim, polyfill layout.
- [`cswin32-com`](../../.agents/skills/cswin32-com/SKILL.md) &mdash;
  `ComScope<T>`, `IComIID`, `IID.Get<T>()`, `delegate* unmanaged`
  vtables, the per-struct net472 polyfill recipe.

## Non-negotiables

- **No hand-written `[DllImport]` for APIs available in Win32 metadata.**
  Add the function to `NativeMethods.txt` and call it through the
  generated `PInvoke` / `PInvokeMadowaku` surface instead.
- **Use `nint` / `nuint`**, never `IntPtr` / `UIntPtr`.
- **Use `HANDLE`, `HMODULE`, `HRESULT`, `BOOL` and friends** from
  `Windows.Win32.Foundation` rather than raw `int` / `nint` at interop
  boundaries.
- **No `Enum.HasFlag`** &mdash; it boxes on net472. Use the Touki
  enum extension methods (`AreFlagsSet`, `IsOnlyOneFlagSet`,
  `AreAnyFlagsSet`, `SetFlags`, `ClearFlags`).

## Polyfill layout (`madowaku/Framework/`)

- The folder is **net472-only by construction** &mdash; conditioned in
  the csproj. Do **not** use `#if NETFRAMEWORK` inside
  `madowaku/Framework/`; if it compiles in this folder, it's already
  net472.
- A polyfill declares the BCL namespace it's polyfilling. The folder
  hierarchy under `Framework/` mirrors the namespace:
  `Framework/System/Runtime/InteropServices/ComWrappers.cs` &rarr;
  `namespace System.Runtime.InteropServices;`.
- The per-struct `IComIID` polyfill lives at
  `madowaku/Framework/Windows/Win32/IComIID.cs` and is declared once;
  individual COM structs add their `IComIID` implementation in a partial
  next to the CsWin32-generated struct.

## TFM gating

- New types that are only meaningful on modern .NET (e.g. ones that take
  a `ReadOnlySpan<char>` parameter that net472 doesn't have a sane shape
  for) belong in source files conditioned by `#if NET` / `#if !NETFRAMEWORK`
  at the top of the file or member, **outside** `madowaku/Framework/`.
- Inside `madowaku/Framework/`, the file is already net472-scoped; no
  `#if NETFRAMEWORK` is required or permitted.

## CsWin32 generator inputs

- `NativeMethods.txt`: one symbol per line. Keep alphabetized within
  logical groups; add a comment header for each new grouping.
- `NativeMethods.json`: tweak generator behavior (`allowMarshaling`,
  custom mappings, COM interface generation mode). Document any
  non-default override with a short comment in the JSON or in the PR
  description.
- After editing either file, build to surface generator diagnostics
  before committing.
