---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, git]
status: aceita
criado: 2026-06-05
---

# ADR — GitFlow executed in pure git (without the git-flow binary)

## Context
There were two ways to implement the GitFlow commands: (a) depend on the external **`git-flow`** binary installed on the machine, or (b) reproduce each operation with **native git**. The external binary adds an install dependency, has port variations (`git-flow-avh` vs. classic) and its own config keys that GitExtensions does not always write in the expected format.

## Decision
Implement **all** the GitFlow commands (start / publish / track / update / finish, for feature/bugfix/release/hotfix/support) as **native git sequences** inside [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]. The `git-flow` binary does **not** need to be installed.

## Consequences
**Positive:**
- Zero external dependency beyond Git for Windows itself.
- Full control over each step → enables **flexible GitFlow** (feature child of a feature) and the automatic *finish release* (merge main + tag + merge develop + push everything).
- A visible log of each git command executed.

**Trade-offs:**
- The plugin reimplements the semantics of git-flow and must keep them correct.
- The `gitflow.*` config keys exist only for visual/CLI compatibility — see [[⚙️ git flow - chaves de config (CLI) (EN)|git flow - config keys (CLI)]].

## 🔗 Related
- [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of a feature]]
- [[🔀 GitFlowForm (EN)|GitFlowForm]]
- [[👁️ Visão Geral (EN)|Overview]]
