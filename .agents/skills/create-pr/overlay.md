---
core: create-pr
core-pin: v0.10.0
---

# madowaku overlay - create-pr

Repository-specific companion to the vendored [create-pr](SKILL.md) skill. The
`SKILL.md` and its `host-adapters.md` page are a **pinned copy of the portable
core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.10.0 tag.**

## madowaku publish boundary

- **The approval rule the core points at is**
  [AGENTS.md - Working with the user on changes](../../../AGENTS.md#working-with-the-user-on-changes),
  the canonical source of the commit/push publish boundary and the phrasings that
  are *not* approval ("open a PR", "address the review", "fix the CI failure",
  a bare "go ahead").
- **Never `git commit`, `git push`, or open a PR without an explicit publishing
  verb in the user's most recent message.** Selecting a task is not authorizing
  its publish.
- **Mechanical backstop**:
  [.vscode/settings.json](../../../.vscode/settings.json) denies `git commit`,
  `git push`, and `gh pr create|merge|close|edit` in agent mode (each needs an
  explicit Allow), and `main` is branch-protected. Treat these as backstops, not
  a license to skip the conversation.
- **`origin/main` is the target**; madowaku is the canonical repo.

## Cross-references

- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) - the checklist to walk
  before running this workflow.
- [`address-pr-feedback`](../address-pr-feedback/SKILL.md) - the post-PR workflow
  for subsequent edit rounds.
- [`publish-release`](../publish-release/SKILL.md) - the separate,
  madowaku-specific flow for cutting a `KlutzyNinja.Madowaku` release tag (not a
  PR).

## Updating

Pull upstream changes with `gh skill update create-pr` (review the diff, re-pin
`core-pin`). Keep madowaku-specific additions here.
