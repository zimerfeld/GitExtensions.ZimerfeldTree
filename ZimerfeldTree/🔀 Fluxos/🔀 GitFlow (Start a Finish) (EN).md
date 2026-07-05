---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [fluxo, gitflow, start, finish, etapa2]
---

# Flow: 2 — GitFlow (Start → Finish)

The `GitFlowForm` window, opened by the **GitFlow** button or the context menu. Drives the cycle in **pure git** (without the `git-flow` binary).

![[ScreenshotGitFlow.png]]

## Steps

```
1. [GitFlow]  →  choose the type (feature / bugfix / release / hotfix / support)
        │  name (release/hotfix pre-fill yyyyMMddHHmm)
        │  optional: based on → derive from another feature (feature child of a feature)
        ▼
2. [Start]    →  git checkout -b <prefix><name> <base>
        │  feature/release ← develop · hotfix/support ← main · bugfix ← release/* (mandatory)
        │  based on → git commit --allow-empty (makes the hierarchy visible)
        ▼
3. [Publish]  →  git push --set-upstream <remote> <prefix><name>
   [Track]    →  git fetch + git checkout -b … --track <remote>/<ref>
   [Update]   →  git fetch + checkout + git merge <remote>/<parent>
        ▼
4. [Finish]   →  merge --no-ff into the target + branch -d (+ push --delete if remote exists)
        │  feature/bugfix: target develop / parent release
        │  hotfix/release: checkout main + merge + git tag + checkout develop + merge
        │  release: full automatic flow + push of main/develop/tag + expands TAGS
```

## Key rules

- A **bugfix** only exists **bound to a release** — Start blocked without a release; it stays nested under it.
- **Feature child of a feature** via *based on:* → *finish* **cascades** up to `develop`. See [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of a feature]].
- **Merge error (conflict):** the plugin stops and shows the result; resolve with `git merge --abort` or resolve + `git commit`.

> Full Start→pure git map (all types) in [[👁️ Visão Geral (EN)|Overview]].

## Related

- [[🌲 Abrir e navegar a árvore (EN)|Open and navigate the tree]]
- [[🔀 GitFlowForm (EN)|GitFlowForm]]
- [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[⚙️ git flow - chaves de config (CLI) (EN)|git flow - config keys (CLI)]]
