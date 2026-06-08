---
name: publish-release
description: Publish a new version of the `KlutzyNinja.Madowaku` NuGet package by cutting a release tag. Use when asked to "publish a new version", "release alpha.N", "ship a beta", "cut a release", "promote alpha to beta", or "tag and publish". Walks through choosing the right `Major.Minor.Patch` bump, deciding whether to stay in `alpha` / `beta` / `rc` / stable, composing the `v*` tag, pushing it, and creating the matching GitHub release. Flags when an `AssemblyVersion`-changing Major bump is required (binary breaking changes vs additive/bugfix work).
argument-hint: 'Describe the release you want to cut (channel and/or version bump) if known.'
---

# Publish a release

This repo ships a single NuGet package from one tag stream:

| Package | Tag prefix | Workflow |
| ------- | ---------- | -------- |
| `KlutzyNinja.Madowaku` | `v` (e.g. `v0.1.0-alpha.4`) | [.github/workflows/publish.yml](../../../.github/workflows/publish.yml) |

[MinVer](https://github.com/adamralph/minver) derives every version artifact
(`Version`, `PackageVersion`, `AssemblyVersion`, `FileVersion`,
`InformationalVersion`) from the tag at HEAD. **The tag *is* the version.**
MinVer is referenced directly in
[madowaku/madowaku.csproj](../../../madowaku/madowaku.csproj) with
`<MinVerTagPrefix>v</MinVerTagPrefix>`; there is no `Directory.Build.targets`
and no per-project override (only one package ships).

The publish workflow fires when a `v*.*.*` tag is pushed.
[publish.yml](../../../.github/workflows/publish.yml) validates the tag against
a SemVer-with-`v`-prefix regex (and rejects non-tag refs) as its first step, so
a malformed tag such as `v.0.1.0-alpha.3` fails fast before any pack/push. The
`v*.*.*` trigger glob is *not* the guard &mdash; it matches the stray dot too;
the validation step is what stops a typo. See the "Tag-format guard" section in
[versioning.md](versioning.md).

**Approval scope.** "Publish a release" authorizes preparing the tag and
release notes. It does **not** authorize the tag push. The
**Approval checkpoint** below is the gate. See
[AGENTS.md](../../../AGENTS.md) &sect; "Working with the user on changes" for
the canonical rule.

## Steps overview

1. **Inspect repo state** (below).
2. Establish the prior version &mdash; see [versioning.md](versioning.md).
3. Decide the prerelease channel (alpha / beta / rc / stable) &mdash; [versioning.md](versioning.md).
4. Decide `Major.Minor.Patch` and check the `AssemblyVersion` gotcha &mdash; [versioning.md](versioning.md).
5. Compose and validate the tag &mdash; [versioning.md](versioning.md).
6. **Approval checkpoint** (below) &mdash; stop and wait for an explicit publish verb.
7. Create and push the tag, then watch the workflow &mdash; see [release-steps.md](release-steps.md).
8. Create the GitHub release from the notes template &mdash; [release-steps.md](release-steps.md).
9. Aftercare &mdash; [release-steps.md](release-steps.md).

## 1. Inspect repo state

Read-only checks:

- `git remote -v` &mdash; confirm `origin` points at `JeremyKuhne/madowaku`
  (the push target).
- `git rev-parse --abbrev-ref HEAD` &mdash; must be `main` or a release branch.
  Refuse to tag from an arbitrary feature branch unless the user explicitly
  says so.
- `git status --porcelain` &mdash; must be clean. If dirty, stop and ask.
- `git log origin/main..HEAD` and `git log HEAD..origin/main` &mdash; must both
  be empty. The tag must point at the published `origin/main` tip (or whatever
  the user confirmed in the previous bullet).

There is only one package, so no "which package" prompt is needed. Then work
through steps 2&ndash;5 in [versioning.md](versioning.md) to land on a
validated tag.

## Approval checkpoint

**Stop here.** Show the user:

- The chosen tag (e.g. `v0.1.0-alpha.4`).
- The prior tag and what bumped (e.g. "alpha.3 &rarr; alpha.4, no Major/Minor
  change").
- The commit the tag will point at (`git rev-parse HEAD`, short SHA + subject).
- A short summary of the changes since the prior tag
  (`git log --oneline <prior-tag>..HEAD`).
- The expected published `AssemblyVersion` before/after.

Wait for an explicit publishing verb (`tag`, `push the tag`, `ship it`,
`publish`). Do **not** infer approval from the original "publish a release"
request &mdash; that authorized the *preparation*, not the push. See
[AGENTS.md](../../../AGENTS.md).

After approval, follow [release-steps.md](release-steps.md) to push the tag,
watch the workflow, and create the GitHub release.

## Sub-pages

- [versioning.md](versioning.md) &mdash; establishing the prior version, the
  prerelease channel decision, the `Major.Minor.Patch` bump table, the
  `AssemblyVersion` gotcha, the exact tag format, and the tag-format guard.
- [release-steps.md](release-steps.md) &mdash; creating and pushing the
  annotated tag, `workflow_dispatch` recovery, the GitHub release notes
  template, and aftercare.

## Cross-references

- [AGENTS.md](../../../AGENTS.md) &sect; "Working with the user on changes"
  &mdash; the publish-boundary rule governing the approval checkpoint.
- [madowaku/madowaku.csproj](../../../madowaku/madowaku.csproj) &mdash; MinVer
  wiring and package metadata.
- [.github/workflows/publish.yml](../../../.github/workflows/publish.yml)
  &mdash; the publish pipeline itself.
