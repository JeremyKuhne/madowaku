---
core: github-actions-cost-optimization
core-pin: 85325dbeb5d5c3e9be39b9f7725a25d971b77d8a
---

# madowaku overlay - github-actions-cost-optimization

Repository-specific companion to the vendored
[github-actions-cost-optimization](SKILL.md) skill. The `SKILL.md` and its
sibling pages are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to an unreleased commit.** This skill postdates the commons `v0.10.0`
> tag, so `core-pin` is the commit SHA
> `85325dbeb5d5c3e9be39b9f7725a25d971b77d8a` rather than a release tag. Re-pin to
> a tag once the commons ships one that includes this skill.

## madowaku bindings

- **The workflows to model** are
  [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml) (build and
  test), [.github/workflows/publish.yml](../../../.github/workflows/publish.yml)
  (packaging `KlutzyNinja.Madowaku`), and
  [.github/workflows/agent-files.yml](../../../.github/workflows/agent-files.yml)
  (agent-file validation, already path-filtered).
- **Do not weaken required validation to save minutes.** The matrix that builds
  `net10.0`, `net10.0-windows10.0.22000.0`, and `net472` exists for correctness;
  optimize triggers, caching, and artifact retention before dropping a leg.
- **Application runtime performance is out of scope here** - that is
  [`performance-testing`](../performance-testing/SKILL.md).

## Cross-references

- [`engineering-baseline`](../engineering-baseline/SKILL.md) - the CI domain this
  cost lens refines.
- [`security-review`](../security-review/SKILL.md) - keep supply-chain checks
  (pinned actions, least privilege) intact while trimming cost.

## Updating

Pull upstream changes with `gh skill update github-actions-cost-optimization`
(review the diff, re-pin `core-pin`) - ideally re-pinning to a release tag once
one includes this skill.
