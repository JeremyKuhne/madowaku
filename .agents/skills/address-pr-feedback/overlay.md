---
core: address-pr-feedback
core-pin: v0.11.0
---

# madowaku overlay - address-pr-feedback

Repository-specific companion to the vendored [address-pr-feedback](SKILL.md)
skill. The `SKILL.md` is a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in its frontmatter). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku bindings

- **The publish boundary still applies.** "Address the review comments", "fix the
  comments", and "fix the CI failure" are **edit-only**; they never authorize a
  commit or push. Make the changes, describe them, and stop - see
  [AGENTS.md - Working with the user on changes](../../../AGENTS.md#working-with-the-user-on-changes).
- **Run `dotnet test -c Release` before declaring a fix done.** Release-mode
  inlining surfaces bugs Debug does not (the `Unsafe.As`-on-a-parameter foot-gun
  on net481 RyuJIT is a known one).
- **Do not stack "make CI green" follow-up commits.** Fix, then wait for an
  explicit publishing verb.

## Cross-references

- [`create-pr`](../create-pr/SKILL.md) - the pre-PR counterpart with the same
  publish gate.
- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) - the checklist for the
  edits you push in a follow-up round.

## Updating

Pull upstream changes with `gh skill update address-pr-feedback` (review the
diff, re-pin `core-pin`). Keep madowaku-specific additions here.
