---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
tags: [conhecimento, git, gitflow]
---

# git flow — config keys (CLI)

## Problem
GitExtensions writes gitflow settings in **its own internal format** (`gitflow.branch.develop.type=base`, etc.), but the **Plugins → Gitflow** plugin uses the **git flow CLI**, which expects different keys. Since the expected keys don't exist, the plugin thinks gitflow was never initialized and keeps showing **"Init Gitflow"**.

## Solution — add the keys in the standard format
```bash
git config gitflow.branch.master main
git config gitflow.branch.develop develop
git config gitflow.prefix.feature feature/
git config gitflow.prefix.bugfix bugfix/
git config gitflow.prefix.release release/
git config gitflow.prefix.hotfix hotfix/
git config gitflow.prefix.support support/
git config gitflow.prefix.versiontag ""
```

> Source: `GitFlowFix.txt` at the root of the [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]] repository.

---

## Maintenance — stale remote refs

### Problem
Branches deleted on the server keep showing up in the Git Extensions tree under `origin/feature/` because Git **does not automatically remove** the local tracking pointers.

### Manual cleanup
```bash
git remote prune origin
```

### Permanent rule (configured on 2026-06-02)
```bash
git config --global fetch.prune true
```
With this setting, every `git fetch` (including F5 in Git Extensions) automatically removes stale refs.

## 🔗 Related
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
