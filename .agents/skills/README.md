# Skills catalog (`.agents/skills/`)

Inventory of skills available in this repo, what triggers each, and how to
disambiguate overlapping ones. For the file format and discovery rules,
see [FORMAT.md](./FORMAT.md).

Skill auto-invocation matches against the `description` field. When two
skills have overlapping verbs in their descriptions, the wrong one can
fire silently. The "Disambiguation" section below records every known
overlap.

## Inventory

| Skill | Trigger phrasing | Cross-references |
| ----- | ---------------- | ---------------- |
| [cswin32-interop](./cswin32-interop/SKILL.md) | "add a P/Invoke", "replace `[DllImport]`", "use CsWin32 for X", "what does `PInvoke` vs `PInvokeMadowaku` mean", net472 vs modern .NET CsWin32 gating, the `ComWrappers` polyfill, Touki vs local polyfills | `cswin32-com` |
| [cswin32-com](./cswin32-com/SKILL.md) | "use `ComScope<T>`", "wire up `IComIID`", "activate a COM object", "manually define an `ICLR*` / `IWbem*` / `IMetaData*` interface", `IID.Get<T>()`, `delegate* unmanaged` vtables, per-struct net472 polyfill | `cswin32-interop` |
| [performance-testing](./performance-testing/SKILL.md) | "add a benchmark", "run perf tests", "compare allocations", "BenchmarkDotNet", "why is net481 slower/faster" | `cswin32-interop`, `cswin32-com` |

## Disambiguation

### `cswin32-interop` vs `cswin32-com`

Both touch CsWin32. They are mutually exclusive by **surface**:

- **P/Invoke functions, Win32 structs/enums/constants, `HANDLE`/`HMODULE`/
  `HRESULT`/`BOOL`, TFM gating, the `ComWrappers` polyfill, the
  `PInvoke` vs `PInvokeMadowaku` split** &rarr; `cswin32-interop`.
- **COM interfaces &mdash; `ComScope<T>`, `IComIID` (per-struct net472
  polyfill), `IID.Get<T>()`, manual struct-based interfaces (CLR hosting,
  metadata, etc.), `delegate* unmanaged` vtables, `CoCreateInstance` /
  `CoGetClassObject`** &rarr; `cswin32-com`.

If a change adds both a new P/Invoke and a new COM type, run
`cswin32-interop` first to add the P/Invoke surface, then `cswin32-com`
for the COM activation and per-struct partial.

## Maintenance

Freshness is tracked from git history, not from a manual column. CI warns
(does not fail) when a skill directory has no commits in the last 90 days
&mdash; see
[.github/workflows/agent-files.yml](../../.github/workflows/agent-files.yml).

A stale warning means "re-read this skill end-to-end against the current
codebase." Confirm:

1. Every cross-reference resolves.
2. Every file path / type / API mentioned still exists.
3. Every claim about the codebase is still true.

The only way to clear the warning is to commit a change to the skill
directory &mdash; ideally the result of a real review pass, but at minimum
a whitespace touch with a commit message stating "verified still current."

When adding a new skill, append a row to the inventory above in the same
change set; the skill is not "shipped" until the catalog reflects it.
