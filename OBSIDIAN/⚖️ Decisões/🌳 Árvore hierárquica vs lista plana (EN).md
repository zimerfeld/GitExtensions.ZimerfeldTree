---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, ui, arvore, hierarquia]
status: aceita
criado: 2026-06-01
---

# ADR — Hierarchical branch tree instead of a flat list

## Context
GitExtensions (and most git clients) displays branches in a **flat list**. In repositories that adopt GitFlow, the name carries an implicit hierarchy (`feature/login`, `release/1.2`, `feature/login-oauth` deriving from `feature/login`) that the flat list hides, making it hard to see kinship and organization.

## Decision
Display the branches in a **hierarchical tree**, in 3 fixed sections (**LOCAL / REMOTES / TAGS**), combining **two dimensions**:
1. **Real ancestry** — kinship by commits / GitFlow relations (via the `git log --all` graph + BFS).
2. **Grouping by path** — the `/` separator in the name becomes a folder in the tree.

## Consequences
**Positive:**
- Immediate visual context of who derives from whom.
- Per-section `(N)` counters and status bar `Local: N | Remote: N | Tags: N`.
- Foundation for flexible GitFlow (feature child of a feature).

**Trade-offs:**
- The folder grouping is **by name (`/`)**, not by commit kinship — `master` and `develop` appear as siblings.
- A **real branch cannot be a parent node** of another (the ref would be a file **and** a directory). See [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]].

## 🔗 Related
- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]
- [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of a feature]]
- [[👁️ Visão Geral (EN)|Overview]]
