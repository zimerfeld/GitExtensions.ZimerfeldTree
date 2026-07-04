---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, git, subprocesso, service, hierarquia, gitflow]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs
---

# BranchHierarchyService.cs

Executor of `git` as a subprocess + output parser + assembly of the branch hierarchy. ~831 lines.

**Path:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs`

---

## Role

All the git logic of the main window and of GitFlow lives here. Runs `git` in a `ProcessStartInfo` (stdout/stderr redirected, UTF-8, no window), parses the output and returns [[🌿 BranchNode (EN)|BranchInfo]] models.

## Collection and hierarchy

- **In-memory commit graph:** a single `git log --all` builds the graph; parents resolved by **BFS** → **O(commits)** instead of O(N²×subprocess).
- LOCAL/REMOTES combine **real ancestry** (kinship by commits / GitFlow) **+ grouping by path** (`/`).
- `BuildGitFlowParentMap` uses the real ancestry to find the GitFlow parent (e.g., the release that is parent of a bugfix).
- Detects hierarchy **violations** (e.g., `violLocalBugfix`) that trigger the GitFlow auto-organization.

## GitFlow commands (pure git)

Implements the start / publish / track / update / finish sequences for the five types. The complete **Start→git** map is in [[👁️ Visão Geral (EN)|Overview]] and [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]].

## Known limitations

- **Two branches on the same commit** do not form a parent-child relationship (the BFS never finds one as the parent of the other) → both become roots; automatic solution = empty commit at Start via *based on*. See [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]].
- A **real branch cannot be a parent node** of another (the ref would be a file **and** a directory).

## Related

- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[🔀 GitFlowForm (EN)|GitFlowForm]] · [[⏪ RestoreForm (EN)|RestoreForm]]
- [[🌿 BranchNode (EN)|BranchNode]]
- [[🏗️ Arquitetura (EN)|Architecture]]
