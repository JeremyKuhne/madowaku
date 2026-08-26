---
core: technical-writing
core-pin: v0.16.1
---

# madowaku overlay - technical-writing

Repository-specific companion to the vendored [technical-writing](SKILL.md)
skill. The `SKILL.md`, its `artifact-patterns.md` page, and bundled references
are a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.16.1 tag.**

## madowaku bindings

- Durable human-facing artifacts include [README.md](../../../README.md),
  [CONTRIBUTING.md](../../../CONTRIBUTING.md), public XML documentation, pull
  request bodies, review-thread replies, and GitHub release notes.
- Ground status, compatibility, test counts, and completion claims in the
  current diff, command output, or GitHub state. Preserve uncertainty when the
  available evidence does not establish a claim.
- Public XML documentation must also follow
  [AGENTS.md - Comments and XML documentation](../../../AGENTS.md#comments-and-xml-documentation).
- A successful writing review never authorizes a commit, push, tag, release, or
  other remote action. The owning workflow keeps the publish boundary.

## Cross-references

- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) establishes the code,
  test, and diff facts before prose review.
- [`create-pr`](../create-pr/SKILL.md),
  [`address-pr-feedback`](../address-pr-feedback/SKILL.md), and
  [`publish-release`](../publish-release/SKILL.md) own the corresponding remote
  actions and approval gates.
- [`agent-files-review`](../agent-files-review/SKILL.md) owns customization
  behavior and Markdown correctness; this skill owns reader-facing prose.
- [`code-comprehension`](../code-comprehension/SKILL.md) owns source-code
  readability rather than human-facing artifacts.

## Updating

When the core is re-pinned, update `core-pin`, review these bindings against the
new core, and run the repository's strict skill validator and relative-link
check.
