---
core: manage-skills
core-pin: v0.11.0
---

# madowaku overlay - manage-skills

Repository-specific companion to the vendored [manage-skills](SKILL.md) skill.
The `SKILL.md`, its sibling pages (`find.md`, `build.md`, `update.md`), the
bundled `scripts/Validate-Skills.ps1`, and `assets/overlay.md.tmpl` are a
**pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku bindings

- **The commons is `JeremyKuhne/agent-skills`; madowaku pins all vendored cores
  to `v0.11.0`.**
- **The catalog is** [.agents/skills/README.md](../README.md); add or update its
  inventory row and disambiguation in the same change as any vendor, tweak, or
  new skill.
- **Born-local (never upstreamed):**
  [`publish-release`](../publish-release/SKILL.md) is specific to madowaku's
  structure and carries no `github-*` provenance; leave it out of the commons.
- **CsWin32 domain cores:**
  [`cswin32-interop`](../cswin32-interop/SKILL.md) and
  [`cswin32-com`](../cswin32-com/SKILL.md) are now vendored commons cores with
  madowaku overlays. The COM core declares the interop core as a hard
  dependency; install and update them at the same pin.
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
