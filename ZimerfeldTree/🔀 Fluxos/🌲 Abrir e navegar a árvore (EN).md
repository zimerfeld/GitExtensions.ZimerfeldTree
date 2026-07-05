---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [fluxo, arvore, abertura, overlay, etapa1]
---

# Flow: 1 — Open and navigate the tree

Opening the main window (`BranchHierarchyForm`) and navigating the branch hierarchy.

![[ScreenshotBranchHierarchy.png]]

## Steps

```
1. Plugins → ZimerfeldTree   →  ZimerfeldTreePlugin.Execute()
        │  opens/brings-to-front the singleton window (no git in the constructor)
        ▼
2. Shown  →  FirstLoadAsync → RefreshTreeAsync(showOverlay:true)  (Task.Run)
        │  overlay 0→100% · 8 steps · Cancel button · form blocked
        │  1 single `git log --all` → commit graph → parents by BFS (O(commits))
        ▼
3. Tree populated in 3 sections: LOCAL (N) · REMOTES (N) · TAGS (N)
        │  current branch in bold · status bar Local/Remote/Tags
        │  overlay closes as soon as the tree appears (first open, no delay)
        ▼
4. git fetch of the upstream in the background  →  corrects counters ↓N / ↑N
        ▼
5. Navigate: real-time filter · expand/collapse (persisted state)
        │  Pull / Push / Commit / Delete / GitFlow / Restore buttons (act on HEAD)
        │  context menu acts on the clicked branch
```

## Details

- **Working Directory** comes from `cboRepo` (read from `GitExtensions.settings`), independent of the host. Switching repos re-points the Commit counter's `FileSystemWatcher`.
- **Overlay** only on the first display and explicit refreshes — it does not reappear when returning from GitFlow/Restore (the tree is already updated live).
- **Live Commit counter** updates the Commit button's `(N)` silently (600 ms debounce, ignores `.git`).

## Related

- [[🔀 GitFlow (Start a Finish) (EN)|GitFlow (Start to Finish)]]
- [[⏪ Restore (voltar no tempo) (EN)|Restore (go back in time)]]
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
