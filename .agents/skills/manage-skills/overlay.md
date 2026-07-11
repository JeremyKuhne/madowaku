---
core: manage-skills
core-pin: v0.10.0
---

# madowaku overlay - manage-skills

Repository-specific companion to the vendored [manage-skills](SKILL.md) skill.
The `SKILL.md`, its sibling pages (`find.md`, `build.md`, `update.md`), the
bundled `scripts/Validate-Skills.ps1`, and `assets/overlay.md.tmpl` are a
**pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku bindings

- **The commons is `JeremyKuhne/agent-skills`; madowaku pins to `v0.10.0`**
  (the one exception is `github-actions-cost-optimization`, which postdates that
  tag and is pinned to a commit SHA - see its overlay).
- **The catalog is** [.agents/skills/README.md](../README.md); add or update its
  inventory row and disambiguation in the same change as any vendor, tweak, or
  new skill.
- **Born-local (never upstreamed):**
  [`publish-release`](../publish-release/SKILL.md) is specific to madowaku's
  structure and carries no `github-*` provenance; leave it out of the commons.
- **Portable cores pending promotion:**
  [`cswin32-interop`](../cswin32-interop/SKILL.md) and
  [`cswin32-com`](../cswin32-com/SKILL.md) are generic cores reconciled from
  madowaku's and dotnet/msbuild's CsWin32 skills, each with a madowaku
  `overlay.md`. They carry portfolio `metadata` but no provenance yet
  (`core-pin: pending-promotion`); a separate change upstreams them to the
  commons and re-vendors them here with a pin.
- **Frontmatter validation**: run the bundled
  [scripts/Validate-Skills.ps1](scripts/Validate-Skills.ps1) on a skill
  directory. The repo's own agent-file checks live in
  [tools/Validate-AgentFiles.ps1](../../../tools/Validate-AgentFiles.ps1) and run
  in [.github/workflows/agent-files.yml](../../../.github/workflows/agent-files.yml).
- **`gh skill` is optional.** When it is unavailable, vendor by copying the
  pinned core and stamping `metadata.github-*` provenance by hand, then author
  the overlay from `assets/overlay.md.tmpl`.

## Cross-references

- [`agent-files-review`](../agent-files-review/SKILL.md) - validates the syntax
  and conventions of the file you land; run it after any vendor or overlay edit.

## Updating

Pull upstream changes with `gh skill update manage-skills` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
