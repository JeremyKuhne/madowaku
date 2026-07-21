---
core: code-comprehension
core-pin: v0.11.0
---

# madowaku overlay - code-comprehension

Repository-specific companion to the vendored [code-comprehension](SKILL.md)
skill. The `SKILL.md` and its bundled `references/research.md` are a **pinned
copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in `SKILL.md`). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku bindings

- **Local style is [AGENTS.md](../../../AGENTS.md)** and the path-specific
  [.github/instructions/](../../../.github/instructions/) rules. Judge
  readability against those conventions (explicit types, no `var`, indented XML
  docs, no end-of-line comments) rather than generic defaults.
- **Interop density is expected.** Pointer-heavy, `unsafe`, multi-targeted code
  is inherent here; weigh cognitive load against the interop patterns in
  [`cswin32-com`](../cswin32-com/SKILL.md) and
  [`cswin32-interop`](../cswin32-interop/SKILL.md), not against idiomatic managed
  code.

## Cross-references

- [`pre-pr-self-review`](../pre-pr-self-review/SKILL.md) - where a readability
  concern turns into a pre-PR action.

## Updating

Pull upstream changes with `gh skill update code-comprehension` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
