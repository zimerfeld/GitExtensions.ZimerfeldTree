---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, janela-principal, arvore]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs
---

# BranchHierarchyForm.cs

Main non-modal window — the hierarchical branch tree. **~2066 lines** (the bulk of the UI). Title bar: `ZimerfeldTree - BranchHierarchy`.

**Path:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs`

---

## Role

`Sizable` WinForms `Form`, non-modal, singleton per session, `CenterScreen`. Displays branches in 3 fixed sections — **LOCAL / REMOTES / TAGS** — with `(N)` counters, the current branch in bold, real-time filtering, and buttons above the tree. The constructor does **no** git; everything is read in the background after `Shown`.

## Functional blocks

| Block | Description |
|---|---|
| Tree | `TreeView` with real ancestry (commits/GitFlow) + grouping by path (`/`). ImageList from [[🎨 NodeIcons (EN)|NodeIcons]] |
| Async loading | `FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)` on a `Task.Run`; overlay 0→100%, 8 steps, Cancel button |
| Buttons | Pull / Push / Commit / Delete / GitFlow / Restore above the tree; counters `↓N` / `↑N` / `(N)`; act on HEAD |
| Live Commit counter | `FileSystemWatcher` on the working dir folder, 600 ms debounce → silent `git status`; ignores `.git` |
| Protected push | If the branch is behind the remote, it offers **automatic pull --rebase + push** |
| Working Directory | `cboRepo` read from `GitExtensions.settings` (XML) — independent of the host |
| Developer Mode | Protects `main`/`master`/`develop` (checkbox blocked) when off |
| Filter | Case-insensitive substring across all sections, preserving parent nodes |
| Persistence | Expand/collapse state per working dir (`ZimerfeldTree.treestate.json`); UI settings (`ZimerfeldTree.uisettings.json`) |
| Context menu | Embedded icons + header showing the checked-out branch; Pull/Push act on the clicked branch |

## Related

- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]] — the git logic behind it
- [[🔀 GitFlowForm (EN)|GitFlowForm]] · [[⏪ RestoreForm (EN)|RestoreForm]] — opened by the buttons
- [[🎨 NodeIcons (EN)|NodeIcons]]
- [[🏗️ Arquitetura (EN)|Architecture]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
