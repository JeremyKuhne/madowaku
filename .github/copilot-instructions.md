# CoPilot instructions for this repository

# Coding Standards
- Use modern C# features where appropriate.
- Avoid using `var`.
- Use target-typed `new` expressions where applicable.
- Lines should never end in whitespace or contain only whitespace.
- Use `nint` / `nuint` for native integer types, never `IntPtr` / `UIntPtr`.
- For enum flag manipulation, use the Touki enum extension methods
  (`AreFlagsSet`, `IsOnlyOneFlagSet`, `AreAnyFlagsSet`, `SetFlags`,
  `ClearFlags`). Do **not** use `Enum.HasFlag` &mdash; it boxes on
  net472 (`Enum.HasFlag` was made an intrinsic on modern .NET but the
  .NET Framework JIT still allocates per call).

# Testing
- Place tests in the 'madowaku.tests' project.
- Use descriptive test method names.
- Cover edge and negative cases.
- Do not add "Arrange, Act, Assert" comments in tests.

# General Guidance
- Include XML documentation for public APIs.
- Ensure code is cross-compatible with .NET 10 and .NET Framework 4.7.2.
- Adhere to the repository's license and copyright.

# Skills

Domain-specific guidance lives under [.agents/skills/](../.agents/skills/).
Tools that support the `SKILL.md` format (Copilot cloud agent, Copilot code
review, VS Code, Visual Studio, Claude Code) discover them via each skill's
frontmatter `description` and invoke them on demand. See
[.agents/skills/FORMAT.md](../.agents/skills/FORMAT.md) for the file format
and [.agents/skills/README.md](../.agents/skills/README.md) for the
inventory.

- [cswin32-interop](../.agents/skills/cswin32-interop/SKILL.md)
  &mdash; CsWin32 P/Invoke patterns, TFM gating, polyfill layout, the
  `ComWrappers` shim, and the `PInvoke` vs `PInvokeMadowaku` split.
- [cswin32-com](../.agents/skills/cswin32-com/SKILL.md)
  &mdash; struct-based COM interop (`ComScope<T>`, `IComIID` polyfill,
  manual COM structs, activation).