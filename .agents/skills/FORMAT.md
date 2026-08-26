# Agent Skills format reference

Format details for files under `.agents/skills/`. For the catalog of skills
currently present in this repo, see [README.md](./README.md).

## Layout

```text
.agents/skills/
  <skill-name>/
    SKILL.md           # required
    <other resources>  # scripts, templates, examples (optional)
```

The directory name **must** match the `name` field in `SKILL.md` exactly,
or the skill silently fails to load.

## `SKILL.md` format

```markdown
---
name: skill-name                 # lowercase, digits, hyphens; max 64 chars; matches dir name
description: One-sentence summary of what the skill does and when to use it.
---

# Body

Detailed instructions, procedures, examples. Reference sibling files with
relative Markdown links, e.g. `[template](./template.cs)`.
```

Optional frontmatter fields: `argument-hint`, `user-invocable`,
`disable-model-invocation`, `context` (`inline` or `fork`).

Vendored cores also carry string-valued `metadata`. `requires` names hard skill
dependencies that must be installed at the same pin; `related` names optional
handoffs. A born-local skill may use the same relationship fields when it
depends on another project skill.

## Human-facing prose contract

A skill that normally creates durable human-facing prose or remotely publishes
text must declare
[`technical-writing`](./technical-writing/SKILL.md) in `metadata.requires`.
Use it while drafting or revising the candidate and run pre-publication mode
after the text and its evidence stabilize. The owning skill retains evidence
collection, domain validation, approval, and the remote action.

Routine progress updates and session-only summaries do not create this
dependency. An optional handoff in `metadata.related` is sufficient when a
skill can complete its workflow without producing or publishing prose.

## Personal profile contract

Personal profile source, writing samples, identity, evidence, migration output,
and installed runtime packages never belong under this public project skill
root. Keep canonical personal source in approved private storage and install a
reviewed complete copy only at user scope. Public skills and fixtures must stay
generic and contain no identifying profile detail.

## Discovery

Vendor-neutral location for [Agent Skills](https://agentskills.io/).
Discovered by GitHub Copilot (VS Code, CLI, cloud agent) and Claude Code.
The `description` field is the trigger surface — phrase it around the
verbs and nouns a user would say when they need the skill.

## Validation

Wrapped prose inside a list item must stay on the paragraph's starting column.
CommonMark accepts lazy or deeper indentation and standard markdownlint does not
enforce consistent prose alignment; the bundled skill validator does.

Run the repository validators after changing a skill or its overlay:

```pwsh
pwsh .agents/skills/manage-skills/scripts/Validate-Skills.ps1 <skill-directory>
pwsh tools/Validate-AgentFiles.ps1
```
