---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquitetura, classes, design, i18n, threading, gitextensions]
versao: 1.0.358
---

# Architecture

## Class diagram

```
GitExtensions (host)
    │
    │  MEF (System.ComponentModel.Composition)
    ▼
ZimerfeldTreePlugin        ← [Export(IGitPlugin)] : GitPluginBase
    │  Execute()  → opens/brings-to-front the singleton window
    │  captures _commands (Register/Unregister)
    ▼
BranchHierarchyForm (main window)   ← Sizable, non-modal, singleton
    │  cboRepo (independent working dir)
    │  RefreshTreeAsync(...) → Task.Run (progress overlay)
    │  buttons: Pull / Push / Commit / Delete / GitFlow / Restore
    │        │                       │
    │        ▼                       ▼
    │  GitFlowForm (modal)      RestoreForm (modal)
    │  start/publish/track/     10 tabs: restore/revert/
    │  update/finish            reset/reflog/rebase…
    │        │                       │
    ▼        ▼                       ▼
BranchHierarchyService     ← git executor + output parser + hierarchy assembly
    │  RunGit(args) → (StdOut, StdErr, ExitCode)
    ▼
git (PATH)   ·   models: BranchInfo / BranchType (BranchNode.cs)
                 icons:  NodeIcons (tree) · PluginIcon (window)
```

## The classes

### `ZimerfeldTreePlugin` — entry point
Inherits from `GitPluginBase`, exported via MEF as `IGitPlugin`. **`Execute`** opens (or brings to front) the **singleton window** `BranchHierarchyForm`; **`Register`/`Unregister`** capture/clear `_commands` (`IGitUICommands`) used to open the host's native Commit/Push/Pull dialogs. See [[🌳 ZimerfeldTreePlugin (EN)|ZimerfeldTreePlugin]].

### `BranchHierarchyForm` — the main window
WinForms `Sizable`, non-modal, `CenterScreen`, singleton per session. A tree in 3 fixed sections (**LOCAL / REMOTES / TAGS**), real-time filtering, buttons above the tree, a progress overlay on first load, a live Commit counter (FileSystemWatcher). The constructor does **no** git — everything is read in the background triggered by `Shown`. See [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]] and [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]].

### `GitFlowForm` — the GitFlow window
Modal. Drives start/publish/track/update/finish for feature, bugfix, release, hotfix and support using **native git only** (does not depend on the `git-flow` binary). Allows a **flexible hierarchy** (feature child of a feature via *based on:*). See [[🔀 GitFlowForm (EN)|GitFlowForm]] and [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]].

### `RestoreForm` — the Restore window
Modal, ~980 px, 10 tabs from the safest to the most destructive (Emergency Plan → Restore File/Tree/Tag → Cherry-Pick → Revert → Reset → New Branch/Tag → Reflog → Discard Local → Rebase). See [[⏪ RestoreForm (EN)|RestoreForm]] and [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

### `BranchHierarchyService` — git executor + hierarchy
Runs `git` in subprocesses (stdout/stderr redirected) and **parses** the output. Builds the commit graph with **a single `git log --all`** and resolves parents by BFS → **O(commits)** instead of O(N²×subprocess). Contains the logic of the GitFlow commands (pure git). See [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]].

### Models and icons
- `BranchNode.cs` — `BranchInfo` (branch data) + `BranchType` enum (Local/Remote/Tag). See [[🌿 BranchNode (EN)|BranchNode]].
- `NodeIcons.cs` — 16×16 GDI+ icons + embedded PNGs (tree ImageList). See [[🎨 NodeIcons (EN)|NodeIcons]].
- `PluginIcon.cs` — the "Tree of Life" icon of the plugin/window (`Resources/ico.png`), loaded once and cached. See [[🖼️ PluginIcon (EN)|PluginIcon]].

## Decoupling from the host

> [!important] The window picks the repository via its own `cboRepo`
> The working directory comes from the `cboRepo` combo (populated from the GitExtensions settings XML), independent of the active repository in the host. `Register` keeps `_commands` only to be able to open the native dialogs (Commit/Push/Pull) **in the selected working dir**. See [[🪟 Janela não-modal singleton (EN)|Non-modal singleton window]].

## Localization (i18n)

English / Portuguese, chosen **per window** and remembered. The main window uses the global `I18n.SetLanguage` (persisted in `ZimerfeldTree.language.json`); GitFlow and Restore have their own selector persisted in their settings files. It translates **controls/labels**, never the data.

## Threading

> The window **opens instantly**: the constructor does **zero git work**. The first load runs behind the `Shown` event (`FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)`).

- **`RefreshTreeAsync`** — collects branches/tags/hierarchy on a `Task.Run` background thread and applies it to the UI; progress overlay (0→100%, 8 steps) on the first open and on explicit refreshes.
- **Live Commit counter** — `FileSystemWatcher` with a 600 ms debounce → a single `git status` in the background, without rebuilding the tree. Ignores changes in `.git`.
- **Remote check on open** — a `git fetch` of the upstream runs in the background after `Shown` (offline-safe), correcting the Pull/Push counters.

## Related

- [[🌳 ZimerfeldTreePlugin (EN)|ZimerfeldTreePlugin]]
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]
- [[🪟 Janela não-modal singleton (EN)|Non-modal singleton window]]
- [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]]
- [[📦 Dependências (EN)|Dependencies]]
- [[👁️ Visão Geral (EN)|Overview]]
