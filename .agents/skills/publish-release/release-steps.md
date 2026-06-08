# Tag, publish, and aftercare

Detail for the [publish-release](SKILL.md) skill. These steps run **only after**
the approval checkpoint in the core has passed.

## 1. Create and push the tag

Use an annotated tag with a short message:

```pwsh
git tag -a v0.1.0-alpha.4 -m "v0.1.0-alpha.4"
git push origin v0.1.0-alpha.4
```

Do **not** use lightweight tags &mdash; annotated tags carry the tagger and
date that show up in GitHub releases.

Pushing the tag triggers the publish workflow. Watch the run:

- <https://github.com/JeremyKuhne/madowaku/actions/workflows/publish.yml>

The workflow restores, builds Release, packs, and pushes every `.nupkg` it
finds in `./artifacts` to nuget.org with
`dotnet nuget push --skip-duplicate` using the `NUGET_API_KEY` repo secret
(an API key, not OIDC). Only `KlutzyNinja.Madowaku` packs &mdash; the test and
perf projects set `<IsPackable>false</IsPackable>`, so they produce no package.

> **The tag format is guarded.** [publish.yml](../../../.github/workflows/publish.yml)
> validates the tag against the SemVer-with-`v`-prefix regex (and rejects
> non-tag refs) before packing, so a malformed tag like `v.0.1.0-alpha.3`
> fails fast instead of silently publishing a MinVer height fallback. Still
> confirm the published version on nuget.org matches what you intended &mdash;
> the guard catches *malformed*, not *wrong-but-well-formed*. See the
> "Tag-format guard" section in [versioning.md](versioning.md).

If the workflow fails, treat it like any CI failure: do **not** delete and
re-push the tag without explicit user approval &mdash; that's destructive and
the nuget.org publish is irreversible. Fix forward with the next tag in the
stream.

### Re-running the publish for an existing tag (`workflow_dispatch`)

If a transient failure (NuGet outage, network blip) leaves a tag pushed but
not published, [publish.yml](../../../.github/workflows/publish.yml) also
accepts a manual `workflow_dispatch`. This workflow takes **no inputs** &mdash;
instead, in the Actions UI choose **Run workflow** and select the **tag** ref
(not a branch) from the ref dropdown. The job checks out that exact ref with
`fetch-depth: 0`, so MinVer derives the intended version from the tag.

Select the **tag**, not `main` or a branch: dispatching against a branch makes
MinVer compute a `*.<height>` fallback and would publish a wrong version.

## 2. Create the GitHub release

Once the workflow has succeeded and the package is visible on nuget.org,
create the matching GitHub release. This is what users actually read.

Find the prior release first (the GitHub `get_latest_release` tool or
`gh release view`) so the new notes can reference it. Then create via
`gh release create` (preferred when `gh` is available and authenticated):

```pwsh
# Drop the --prerelease flag for a stable release.
gh release create v0.1.0-alpha.4 `
  --title "v0.1.0-alpha.4" `
  --notes-file release-notes.md `
  --prerelease
```

If `gh` is not available or not authenticated, use the GitHub web UI
(Releases &rarr; Draft a new release &rarr; choose the existing tag), or the
GitHub MCP tools.

### Release notes template

````markdown
## Changes

<!-- One-sentence headline of the most important change. -->

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Breaking changes
<!-- Only present on Major bumps. AssemblyVersion changed from
     0.0.0.0 -> 1.0.0.0; consumers must rebuild. -->
- ...

## Compatibility

- Targets: `net10.0-windows10.0.22000.0`, `net10.0`, `net472`.
- AssemblyVersion: `<old>` -> `<new>` (note **changed** or **unchanged**).

## Install

```bash
dotnet add package KlutzyNinja.Madowaku --version 0.1.0-alpha.4
```

**Full changelog:** <https://github.com/JeremyKuhne/madowaku/compare/v0.1.0-alpha.2...v0.1.0-alpha.4>
````

Notes on the template:

- Use the `--prerelease` flag iff the SemVer has a prerelease label. GitHub
  displays prereleases differently (the "latest" indicator stays on the most
  recent stable). Skipping `--prerelease` on an alpha is a real bug.
- For the `compare/<prior>...<new>` link, use the prior **well-formed** tag.
  Because `v.0.1.0-alpha.3` is malformed, a compare against the alpha.2 tag is
  the meaningful diff until a clean tag exists in the stream.

## 3. Aftercare

- This package is not dog-fooded by another project in the repo &mdash; the
  test and perf projects reference it by project reference, and
  [Directory.Packages.props](../../../Directory.Packages.props) pins
  *dependencies* (Touki, CsWin32, etc.), not `KlutzyNinja.Madowaku` itself. So
  there is no consumer version to bump after a release.
- If you bumped `Major` (binary break), make sure the release notes call out
  the `AssemblyVersion` move (`0.0.0.0` &rarr; `1.0.0.0`) so downstream
  binders know to rebuild.
- The README's NuGet badge tracks nuget.org automatically; no manual edit is
  needed after publishing.
