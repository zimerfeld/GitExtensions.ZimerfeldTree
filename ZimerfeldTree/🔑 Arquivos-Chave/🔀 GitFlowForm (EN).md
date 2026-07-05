---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, gitflow, modal]
arquivo: src/GitExtensions.ZimerfeldTree/GitFlowForm.cs
---

# GitFlowForm.cs

Modal window that drives the `git flow` commands using **native git only**. ~758 lines. Title: `ZimerfeldTree - GitFlow`.

**Path:** `src/GitExtensions.ZimerfeldTree/GitFlowForm.cs`

---

## Role

Does not depend on the `git-flow` binary being installed — each button fires a sequence of pure `git`. Covers the five types: **feature, bugfix, release, hotfix, support**, with operations **start / publish / track / update / finish**. See [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]] and [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]].

## Specific rules

- A **bugfix** can only exist **bound to a release** — `DoStart` blocks if there is no release; the base records a *based-on override* and the bugfix is nested under the release.
- **based on:** allows a **feature that is child of a feature**; in that case it also runs `git commit --allow-empty -m "chore: start <ref>"` so the hierarchy becomes visible. See [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of a feature]].
- **Default name** for release/hotfix pre-filled with `yyyyMMddHHmm`.
- **Finish release** runs the full automatic flow (merge into main + tag + merge into develop + push everything). See the Start→pure git map in [[👁️ Visão Geral (EN)|Overview]].

## Initialize button

Applies the default `gitflow.*` keys all at once — see [[⚙️ git flow - chaves de config (CLI) (EN)|git flow - config keys (CLI)]].

## Related

- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]
- [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]]
- [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of a feature]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[🔀 GitFlow (Start a Finish) (EN)|GitFlow (Start to Finish)]]
