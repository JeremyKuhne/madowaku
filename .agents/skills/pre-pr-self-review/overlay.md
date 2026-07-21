---
core: pre-pr-self-review
core-pin: v0.11.0
---

# madowaku overlay - pre-pr-self-review

Repository-specific companion to the vendored [pre-pr-self-review](SKILL.md)
skill. The `SKILL.md` is a **pinned copy of the portable core** from
[JeremyKuhne/agent-skills](https://github.com/JeremyKuhne/agent-skills) (see the
`metadata.github-*` provenance in its frontmatter). Do not hand-edit the core;
`gh skill update` would flag the drift.

> **Pinned to the commons v0.11.0 tag.**

## madowaku recurring mistakes to self-check

- **Every target must compile and behave.** madowaku builds `net10.0`,
  `net10.0-windows10.0.22000.0`, and `net472`; a change that only builds on one
  is not done. Guard modern-only APIs with `#if NET` and keep net472-only code in
  [madowaku/Framework/](../../../madowaku/Framework/).
- **`dotnet test -c Release`**, not just Debug - Release inlining surfaces the
  `Unsafe.As`-on-a-parameter net481 RyuJIT foot-gun.
- **No `Enum.HasFlag`** (boxes on net472); use the Touki enum extension methods.
- **`nint` / `nuint`, never `IntPtr` / `UIntPtr`**; `HANDLE` / `HRESULT` / `BOOL`
  and friends at interop boundaries.
- **Interop APIs go through the generated `PInvoke` surface**
  (`NativeMethods.txt`), not hand-written `[DllImport]`.
- **A perf claim needs a [madowaku.perf](../../../madowaku.perf/madowaku.perf.csproj)
  benchmark** (or an explicit "not measured" note).

## Cross-references

- [`performance-testing`](../performance-testing/SKILL.md) - to satisfy the perf
  evidence check.
- [`security-review`](../security-review/SKILL.md) - for the unsafe / interop
  surface the review touches.
- [`create-pr`](../create-pr/SKILL.md) - the workflow this checklist precedes.

## Updating

Pull upstream changes with `gh skill update pre-pr-self-review` (review the diff,
re-pin `core-pin`). Keep madowaku-specific additions here.
