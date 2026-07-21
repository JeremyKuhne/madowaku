# Choosing the version and composing the tag

Detail for the [publish-release](SKILL.md) skill. Covers establishing the prior
version, the prerelease channel decision, the `Major.Minor.Patch` bump, the
`AssemblyVersion` gotcha, the exact tag format, and the tag-format guard.

## 1. Establish the prior version

List the most recent five tags on the `v` prefix:

```pwsh
git tag --list 'v*' --sort=-creatordate | Select-Object -First 5
```

Cross-check against nuget.org so you don't accidentally re-publish a version
that's already live (publishing is idempotent thanks to `--skip-duplicate`,
but choosing a duplicate is almost always a mistake):

- <https://www.nuget.org/packages/KlutzyNinja.Madowaku>

Record the prior version (e.g. `0.1.0-alpha.3`).

> **Watch the tag list for the historical `v.` typo.** The most recent tag in
> this repo at the time of writing is `v.0.1.0-alpha.3` — note the stray
> `.` after `v`. MinVer (prefix `v`) cannot parse it, so it is **silently
> ignored** for versioning. When establishing the prior version, take the
> newest tag that is *well-formed* (here `v0.1.0-alpha.2`), not merely the
> newest tag. The publish workflow now rejects such tags — see the
> "Tag-format guard" section in section 4.

## 2. Decide the prerelease channel (alpha / beta / rc / stable)

Before picking numbers, decide what *kind* of release this is. **Always
prompt** the user with the current state explicit:

> "The last well-formed release was an **alpha** (`v0.1.0-alpha.2`). Should
> this also be an alpha release, or are you promoting to beta / rc / stable?"

Present these options (however your agent surface asks the user a multiple-
choice question), and mark the option matching the prior channel as the
recommended default:

- `Stay in alpha` — bug fixes / additive work during early development.
- `Promote to beta` — feature-complete for the upcoming Minor; only
  stabilization work expected.
- `Promote to rc` — release candidate; only blocker bug fixes.
- `Promote to stable` — drop the prerelease label entirely.
- `Use a different label` — free-form.

Channel rules of thumb:

- Don't *skip* channels casually. `alpha` → `beta` → `rc` →
  `stable` is the normal path. Going `alpha` → `stable` is allowed but
  should be deliberate.
- Once you ship a stable `Major.Minor.Patch`, the next prerelease must
  bump *something* (`0.1.0` → `0.1.1-alpha.1` or `0.2.0-alpha.1`); you
  cannot ship `0.1.0-alpha.2` after `0.1.0` stable.
- Do **not** mix channels backwards (no going from `beta` back to `alpha`
  for the same Major.Minor.Patch). If a beta turned out to need more
  churn, bump the underlying version: `0.2.0-beta.3` → `0.3.0-alpha.1`.

## 3. Decide `Major.Minor.Patch`

Apply [SemVer 2.0.0](https://semver.org) rules. The package is currently
pre-1.0, so the rules below describe the **target stable** semantics; while
the prior tag is itself a prerelease, the same bump table applies to the
underlying `Major.Minor.Patch` portion.

| Change shipped since the last tag | Bump |
| --- | --- |
| Binary breaking change to public API of `madowaku.dll` (removed/renamed type or member, signature change, return-type change, base-type change, broken inheritance, removed `[Obsolete]`'d API) | **Major** |
| Behavioral break that compiles but changes observable runtime contract (different exception type, different default, different ordering, different threading guarantee) | **Major** unless the user explicitly accepts shipping it as Minor with a release-note callout |
| Net-new public API, new overload, new optional parameter, new public type, new TFM | **Minor** |
| Bug fix only, no new public surface, no observable contract change for non-buggy callers | **Patch** |
| Internal-only refactor, perf, doc, comment, build, CI | **Patch** (or no release at all) |

For pre-1.0, the user may treat **Major** and **Minor** as both "Minor"
during alpha/beta. That is fine; just confirm the call out loud:

> "This change adds a public type but you're still in 0.1.x alpha — bump
> to 0.2.0-alpha.1 (treating it as Minor) or stay at 0.1.0-alpha.4?"

### When `AssemblyVersion` changes — and why it matters

MinVer's defaults (used as-is in this repo, no overrides) produce:

- `Version` / `PackageVersion` / `InformationalVersion` = full SemVer
  (e.g. `0.1.0-alpha.4`).
- `FileVersion` = `Major.Minor.Patch.0` (e.g. `0.1.0.0`).
- **`AssemblyVersion` = `Major.0.0.0`** (e.g. `0.0.0.0` for any `0.x.y`).

Only a **Major** bump changes `AssemblyVersion`. That has real consequences:

- A change in `AssemblyVersion` forces every consumer that has the assembly
  in their build graph to either rebuild against the new identity or use
  binding redirects (on .NET Framework). Strong-named assemblies make this
  stricter, and `madowaku.dll` is strong-named
  (`klutzyninja.snk`, see [madowaku/madowaku.csproj](../../../madowaku/madowaku.csproj)).
- A change in `Version`/`FileVersion` *without* `AssemblyVersion` changing
  is binary-compatible — consumers can drop the new DLL into an existing
  bin folder and it just works.

**Therefore:** any binary breaking change *must* bump `Major`, even during
0.x. Refusing to bump `Major` on a binary break — to "stay at 0.1.x"
— silently keeps `AssemblyVersion = 0.0.0.0` across an incompatible
boundary, which is a real foot-gun for downstream binders.

When `Major` does bump, also note in the release that `AssemblyVersion`
moved (`0.0.0.0` → `1.0.0.0`).

## 4. Compose the tag

Format:

```text
v<Major>.<Minor>.<Patch>[-<prerelease>]
```

Prerelease segment uses dot-separated identifiers, e.g. `alpha.4`,
`beta.2`, `rc.1`. Numeric identifiers are SemVer-sorted as numbers, so
`alpha.10` correctly sorts above `alpha.9` (no leading zeros).

Examples (good):

- `v0.1.0-alpha.4`
- `v0.2.0-beta.1`
- `v0.2.0-rc.1`
- `v0.2.0`
- `v1.0.0-rc.1`

Examples (malformed — do not create):

- `v.0.1.0-alpha.4` — stray `.` after `v`. This is the exact defect in
  the existing `v.0.1.0-alpha.3` tag; do not recreate it.
- `0.1.0-alpha.4` — missing `v` prefix (MinVer's prefix is `v`).
- `v0.1.0.alpha.4` — `.` instead of `-` before prerelease.
- `v0.1` — missing patch component (the workflow glob `v*.*.*` requires
  three dot-separated segments).
- `v01.02.03` — leading zeros in numeric identifiers.

A well-formed tag matches this SemVer-with-`v`-prefix shape:

```text
^v(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-[0-9A-Za-z.-]+)?$
```

### Tag-format guard

madowaku's [publish.yml](../../../.github/workflows/publish.yml) runs a
**Validate tag format** step as the first thing in the publish job, before
checkout. It rejects the run (fails fast, before any pack/push) when:

- The ref is not a tag (`github.ref` must start with `refs/tags/`) — this
  catches a `workflow_dispatch` accidentally launched from `main` or a branch,
  which would otherwise make MinVer compute a `*.<height>` fallback and publish
  a wrong version.
- The tag name does not match the SemVer-with-`v`-prefix regex above.

The trigger glob (`tags: ['v*.*.*']`) is *not* the guard — `*` matches a
stray dot, so a typo'd tag such as `v.0.1.0-alpha.3` still starts the workflow.
The validation step is what actually stops it: that tag fails the regex and the
job throws before packing.

Even with the guard in place:

- Still triple-check the tag string against the good examples above before
  pushing — the guard prevents a *malformed* publish, not a *wrong but
  well-formed* one (e.g. tagging `v0.2.0` when you meant `v0.1.1`).
- After the workflow runs, confirm the published version on nuget.org matches
  the version you intended.
