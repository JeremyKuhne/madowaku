---
core: agent-files-review
core-pin: v0.11.0
---

# madowaku overlay - agent-files-review

Repository-specific companion to the vendored [agent-files-review](SKILL.md)
skill. The `SKILL.md` and its `frontmatter.md` page are a **pinned copy of the
portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku scaffold paths

- **The validator is**
  [tools/Validate-AgentFiles.ps1](../../../tools/Validate-AgentFiles.ps1). Run it
  plain to check frontmatter, whitespace, and the mirror; run it with `-Fix` to
  regenerate the [.github/copilot-instructions.md](../../../.github/copilot-instructions.md)
  mirror from [AGENTS.md](../../../AGENTS.md).
- **`AGENTS.md` is the single source of truth**; the copilot-instructions mirror
  is generated and must never be hand-edited.
- **Scanned locations**: [.github/instructions/](../../../.github/instructions/),
  [.github/prompts/](../../../.github/prompts/),
  [.github/agents/](../../../.github/agents/), and
  [.agents/](../../), plus [docs/agent-customization.md](../../../docs/agent-customization.md).
- **CI**:
  [.github/workflows/agent-files.yml](../../../.github/workflows/agent-files.yml)
  runs the validator, a 90-day skill-freshness warning, markdownlint
  ([.markdownlint.jsonc](../../../.markdownlint.jsonc)), and an offline lychee
  link check on `ubuntu-latest`.
- **SKILL names** must match `^[a-z0-9-]{1,64}$` and equal the parent directory.
  The vendored [frontmatter reference](frontmatter.md) describes a broader
  portable rule; madowaku enforces this stricter ASCII form.

## Cross-references

- [`manage-skills`](../manage-skills/SKILL.md) - the lifecycle skill for adding,
  vendoring, and updating skills (distinct from this file-syntax review).
- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) - its final audit runs
  the link and validator checks before a PR.

## Updating

Pull upstream changes with `gh skill update agent-files-review` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
