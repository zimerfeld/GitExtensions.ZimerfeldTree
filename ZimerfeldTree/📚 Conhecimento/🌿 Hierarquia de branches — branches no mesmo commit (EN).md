---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-06
tags: [conhecimento, git, hierarquia, branch, bfs, ancestralidade]
---

# Branch hierarchy — branches on the same commit

## Summary

When two branches point to the same commit, the ZimerfeldTree BFS algorithm cannot establish a parent-child relationship between them — because git does not record where a branch was created from, only which commit it points to.

## Details

The `FindParentInGraph` algorithm (in `BranchHierarchyService.cs`) works by BFS:

1. Takes the SHA of the tip of the branch being analyzed
2. Enqueues the **parents** of that commit (not the commit itself)
3. Walks up the graph; the first branch found = parent

When `branch A` and `branch B` share the same SHA:
- The BFS for A starts from the parents of the shared SHA
- B is **at the same starting level** — it is never found as an ancestor
- Result: no relationship is established, both appear as roots

## Example

```
# Problematic situation — both on the same commit
git log --oneline feature/gridsolo feature/mododebug
* c19d7dc  ← HEAD of BOTH branches

# Correct situation — gridsolo one commit ahead
* cea86c1  ← feature/gridsolo   (appears as a child of mododebug in the tree)
* c19d7dc  ← feature/mododebug
```

## When it happens

- Branch created (`git checkout -b nova-branch base`) with no commit afterwards
- Branch whose work was "absorbed" back (a fast-forward makes the base reach the child)

## Solution in ZimerfeldTree

When creating a branch with the **based on:** checkbox marked in the GitFlow → Start window, the plugin automatically runs:

```
git commit --allow-empty -m "chore: start <branch-name>"
```

This ensures the new branch immediately diverges from its base, making the hierarchy visible without needing a content commit.

## 🔗 Related

- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
