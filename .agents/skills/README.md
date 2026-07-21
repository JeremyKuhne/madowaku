# Skills catalog (`.agents/skills/`)

Inventory of skills available in this repo, what triggers each, and how to
disambiguate overlapping ones. For the file format and discovery rules,
see [FORMAT.md](./FORMAT.md).

Skill auto-invocation matches against the `description` field. When two
skills have overlapping verbs in their descriptions, the wrong one can
fire silently. The "Disambiguation" section below records every known
overlap.

## Inventory

Each skill is one of two kinds: **born-local** (specific to madowaku; no
upstream or provenance), or **vendored** from the shared commons
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) at a
pinned ref (also with an `overlay.md`). See
[the vendoring model](#vendoring-model) below.

### Born-local

| Skill | Trigger phrasing |
| ----- | ---------------- |
| [publish-release](./publish-release/SKILL.md) | "publish a new version", "release alpha.N", "ship a beta", "cut a release", "promote alpha to beta", "tag and publish" |

### Vendored (commons `v0.11.0`)

| Skill | Trigger phrasing |
| ----- | ---------------- |
| [cswin32-interop](./cswin32-interop/SKILL.md) | "replace `[DllImport]` with CsWin32", generated `PInvoke.*` / Win32 types, "which package owns this helper?", split a public owner from downstream extenders, native allocator ownership, byte/element sizes, `NativeMethods.*`, blittable signatures, compile-time / runtime / analyzer guards |
| [cswin32-com](./cswin32-com/SKILL.md) | struct-based COM with `ComScope`, caller-owned CCW references, `Advise` / `Unadvise`, activation, raw vtables, manual COM structs, `IComIID` across .NET 10 and .NET Framework, cross-assembly CCWs, mocking struct COM; requires `cswin32-interop` at the same pin |
| [performance-testing](./performance-testing/SKILL.md) | "add a benchmark", "run perf tests", "compare allocations", `BenchmarkDotNet`, multi-phase or exact-oracle investigations, dirty-source reproducibility, "why is net481 slower/faster", "how long / how much memory" |
| [framework-jit-optimization](./framework-jit-optimization/SKILL.md) | optimize a `net481` / .NET Framework hot path, specialize a generic for primitives, diagnose a Framework micro-benchmark regression |
| [scratch-buffer-strategy](./scratch-buffer-strategy/SKILL.md) | zeroed `stackalloc` vs `[SkipLocalsInit]` vs `BufferScope<T>` vs `ArrayPool`, "should I rent or stackalloc?", net481/net10 size crossovers |
| [dotnet-polyfills](./dotnet-polyfills/SKILL.md) | "use a modern .NET API on .NET Framework", PolySharp / `System.Memory` / `Microsoft.Bcl.*`, "which package supplies this type downlevel", "is this already polyfilled" |
| [il-copy-inspection](./il-copy-inspection/SKILL.md) | "find struct copies", "is this a defensive copy", "check for boxing in IL", "did the compiler emit a copy here" |
| [security-review](./security-review/SKILL.md) | "do a security review", "check for ReDoS / DoS", "audit untrusted input"; any `unsafe` / `Unsafe.*` / `MemoryMarshal.*` / `Marshal.*` use |
| [code-comprehension](./code-comprehension/SKILL.md) | "review this for readability", "is this too complex", "reduce nesting / cognitive load", reasonable method length / parameter count / nesting depth |
| [pre-pr-self-review](./pre-pr-self-review/SKILL.md) | self-review before opening a PR, "review my draft", after a reviewer flags issues that should have been caught |
| [create-pr](./create-pr/SKILL.md) | "make a PR", "open a pull request", "push and PR", publish in-progress work for review |
| [address-pr-feedback](./address-pr-feedback/SKILL.md) | "address the review", "fix the comments", "address Copilot's feedback", "fix the CI failure", post-PR follow-up |
| [manage-skills](./manage-skills/SKILL.md) | "find a skill for X", "build a skill" / "create a skill", "update the skill", reconcile a local skill change against the commons vs a repo overlay |
| [agent-files-review](./agent-files-review/SKILL.md) | review or validate changes to `AGENTS.md`, `*.instructions.md`, `*.prompt.md`, `*.agent.md`, `SKILL.md`, "fix the agent-files CI failure" |
| [engineering-baseline](./engineering-baseline/SKILL.md) | "ensure this repo follows modern engineering best practices", "audit this repository", "bring this repo up to standard" |
| [github-actions-cost-optimization](./github-actions-cost-optimization/SKILL.md) | "reduce CI cost / spend / minutes", optimize GitHub Actions triggers, matrices, runners, caches, artifacts |

## Vendoring model

Skills are authored once as a portable **core** in the commons and shared from
there. madowaku holds a pinned, provenance-stamped **copy** of each vendored
core plus a thin repo-specific **overlay**:

- **Vendored core** — the `SKILL.md` (and its sibling pages) carry
  `metadata.github-*` provenance (source repo, ref, tree SHA). It is a mirror of
  upstream; **do not hand-edit it**, or `gh skill update` will flag the drift.
- **Overlay** — `overlay.md` beside the core holds madowaku paths, project
  names, and cross-references to other skills. Its frontmatter records the
  `core-pin` it was reviewed against.
- **Born-local** — `publish-release` is specific to madowaku's structure
  (the `KlutzyNinja.Madowaku` release-tag flow), carries no provenance, and is
  never upstreamed.

All 16 vendored cores are pinned to the commons `v0.11.0` tag. The
[manage-skills](./manage-skills/SKILL.md) skill drives find / build / update,
and [agent-files-review](./agent-files-review/SKILL.md) validates the resulting
files.

## Disambiguation

### `cswin32-interop` vs `cswin32-com`

Both touch CsWin32. They are mutually exclusive by **surface**:

- **P/Invoke functions, Win32 structs/enums/constants, `HANDLE`/`HMODULE`/
  `HRESULT`/`BOOL`, platform and TFM guards, the `ComWrappers` polyfill, the
  public `PInvoke` owner/extender surface** → `cswin32-interop`.
- **COM interfaces — `ComScope<T>`, `IComIID` (emitted on .NET 10 and
  .NET Framework), `IID.Get<T>()`, manual struct-based interfaces (Setup
  Configuration, private / third-party APIs, etc.), `delegate* unmanaged`
  vtables, `CoCreateInstance` /
  `CoGetClassObject`** → `cswin32-com`.

If a change adds both a new P/Invoke and a new COM type, run
`cswin32-interop` first to add the P/Invoke surface, then `cswin32-com`
for the COM activation and per-struct partial.

### PR and release workflows: `create-pr` vs `address-pr-feedback` vs `publish-release`

- **Open a new PR for in-progress work** → `create-pr`.
- **Iterate on an existing PR** (review comments, Copilot feedback, CI fixes)
  → `address-pr-feedback`.
- **Cut a `KlutzyNinja.Madowaku` release tag** (a tag, not a PR) →
  `publish-release` (born-local). All three honor the same publish boundary in
  [AGENTS.md](../../AGENTS.md).

### Performance cluster

- **Measure** (author / run a BenchmarkDotNet benchmark, read results) →
  `performance-testing`.
- **Tune net472/net481 codegen** (specialization, unrolling, BCL delegation)
  → `framework-jit-optimization`.
- **Choose a scratch buffer** (`stackalloc` vs `ArrayPool` vs `BufferScope<T>`)
  → `scratch-buffer-strategy`.
- **Read IL for hidden struct copies or boxing** → `il-copy-inspection`.

### Skill and agent-file meta: `manage-skills` vs `agent-files-review`

- **Catalog lifecycle** (discover, add, vendor, sync a skill) →
  `manage-skills`.
- **Validate one agent file's syntax and conventions** (frontmatter, mirror,
  whitespace) → `agent-files-review`.

### `engineering-baseline` vs `github-actions-cost-optimization`

- **Whole-repo nine-domain audit** (foundation, build, test, publish,
  versioning, CI, supply chain, governance, agent enablement) →
  `engineering-baseline`.
- **CI runner cost specifically** (minutes, matrices, caches, artifacts) →
  `github-actions-cost-optimization`.

## Maintenance

Freshness is tracked from git history, not from a manual column. CI warns
(does not fail) when a skill directory has no commits in the last 90 days
— see
[.github/workflows/agent-files.yml](../../.github/workflows/agent-files.yml).

A stale warning means "re-read this skill end-to-end against the current
codebase." Confirm:

1. Every cross-reference resolves.
2. Every file path / type / API mentioned still exists.
3. Every claim about the codebase is still true.

The only way to clear the warning is to commit a change to the skill
directory — ideally the result of a real review pass, but at minimum
a whitespace touch with a commit message stating "verified still current."

When adding a new skill, append a row to the inventory above in the same
change set; the skill is not "shipped" until the catalog reflects it.
