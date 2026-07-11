---
core: engineering-baseline
core-pin: v0.10.0
---

# madowaku overlay - engineering-baseline

Repository-specific companion to the vendored [engineering-baseline](SKILL.md)
skill. The `SKILL.md`, its sibling pages (`assess.md`, `baseline.md`,
`scaffold.md`), the bundled `references/best-practices.md`, and the `scripts/`
scaffolding tree are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku bindings

- **madowaku is an established multi-target library, not a greenfield.** Use this
  skill for the *assess* path (audit the nine domains and report gaps), not the
  scaffold path.
- **Existing baseline the audit should recognize**: CI in
  [.github/workflows/dotnet.yml](../../../.github/workflows/dotnet.yml), packaging
  and publish in [.github/workflows/publish.yml](../../../.github/workflows/publish.yml)
  (`KlutzyNinja.Madowaku`), agent-file validation in
  [.github/workflows/agent-files.yml](../../../.github/workflows/agent-files.yml),
  central package management in
  [Directory.Packages.props](../../../Directory.Packages.props), and governance in
  [CONTRIBUTING.md](../../../CONTRIBUTING.md) and [LICENSE](../../../LICENSE).
- **Remote or irreversible actions** (branch protection, release settings,
  publishing) are proposed for explicit approval, never run silently - this
  matches [AGENTS.md](../../../AGENTS.md#working-with-the-user-on-changes).

## Cross-references

- [`manage-skills`](../manage-skills/SKILL.md) - the agent-enablement domain the
  audit scores.
- [`security-review`](../security-review/SKILL.md) - the supply-chain and
  unsafe-input domain.
- [`create-pr`](../create-pr/SKILL.md) - to land remediation under the publish
  gate.
- [`github-actions-cost-optimization`](../github-actions-cost-optimization/SKILL.md)
  - the CI-cost lens on the workflows above.

## Updating

Pull upstream changes with `gh skill update engineering-baseline` (review the
diff, re-pin `core-pin`). Keep madowaku-specific additions here.
