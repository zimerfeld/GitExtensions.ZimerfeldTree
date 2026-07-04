---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-06-28 (a behind push now offers Pull-with-rebase-and-then-push via `DoPullRebaseThenPush`/`PullRebase`, instead of only blocking) | 2026-06-16 (Pull/Push icons on the buttons and menu; fetch of the current branch on open; Pull/Push menu acting on the clicked branch; a warning that blocks a behind push; header with the checked-out branch in the menu)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, zimerfeldtree]
fonte: src\GitExtensions.ZimerfeldTree\BranchHierarchyForm.cs
---

# ZimerfeldTree Interface — buttons and flows

> [!abstract] Summary
> **Non-modal** window (`BranchHierarchyForm`) that shows LOCAL / REMOTES / TAGS as a hierarchical tree and stays open next to GitExtensions. This document describes **each control** and **the exact step-by-step** of each action. For the `git flow` window see [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]. Project: [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]].

![[ScreenshotBranchHierarchy.png]]

## 🚪 How the window opens
- The **Plugins → ZimerfeldTree** menu calls `ZimerfeldTreePlugin.Execute`.
- The form is a **singleton** per GitExtensions session: if it already exists, it just updates the working dir and brings it to the front; otherwise it creates a new one.
- `Execute` returns `false` → GitExtensions does **not** refresh its own UI (the window manages its own state).
- The plugin subscribes to host events (`Register`): `PostBrowseInitialize` → repository switch; `PostCheckoutBranch` / `PostCheckoutRevision` → rebuilds the tree; `PostCommit` → rebuilds + focuses; `PostRepositoryChanged` → `OnExternalChange`. This keeps the tree automatically in sync.
- `OnExternalChange` calls `NotifyExternalRepoChanged()`: it refreshes on **genuine** external changes, but **ignores the echo** of our own `NotifyRepoChanged` (the `_suppressEcho` flag) — avoiding a redundant refresh / overlay flash.
- The **overlay only appears on the first display** (`VisibleChanged` guards `_initialLoadDone`): reactivating the window after closing GitFlow/Restore does **not** trigger the overlay (the tree is already up to date).
- **Remote check after opening** (`Shown` → `RefreshRemoteStatusAsync`): the initial load is **offline-safe** and shows the ahead/behind from the last fetch; once the window appears, a `git fetch` of the current branch's upstream runs **off the UI thread** (`FetchCurrentBranchUpstream` → `git fetch <remote> <branch>`), recomputes the tracking and corrects the Pull/Push buttons and the `Branch:` label. Best-effort: a network failure / no upstream is ignored.

## 🧭 Layout (top to bottom)
1. **Top panel** — a "Working Directory:" label, the repositories combo (`_cboRepo`), a "Branch: \<current\>" label.
2. **Filter panel** — a "Filter branches..." box (`_txtFilter`) + a **↺** button (`_btnRefresh`).
3. **Warn panel** (hidden by default) — a GitFlow warning + an **Organize as GitFlow / Restore real hierarchy** button (`_btnGitFlow`).
4. **GitFlow button panel** — **Pull**, **Push**, **Commit**, **GitFlow**, **Restore** (only appear if there is a current branch).
5. **Tree** (`TreeView`) — 3 fixed sections: **LOCAL (n)**, **REMOTES (n)**, **TAGS (n)**.
6. **Bottom panel** — a **Close** button (centered).
7. **Status strip** — `Local: n | Remote: n | Tags: n`.
8. **Loading overlay** — floats over everything during the load.

## 🔄 Tree loading (`RefreshTreeAsync`)
Triggered by: the **first** display of the window (`VisibleChanged`, only on the initial load), the ↺ button, the "Refresh" menu, a repo switch, the host's checkout/commit/repo-changed events (own echo suppressed), and after each local mutation.
Steps (with % in the overlay):
1. `10%` local branches — `git branch --format=%(refname:short)`
2. `30%` remote branches — `git branch -r --format=%(refname:short)`
3. `50%` tags — `git tag --sort=-version:refname`
4. `65%` local hierarchy — `BuildParentMap` (1× `git log --all --format="%H %P"` + BFS)
5. `80%` remote hierarchy — `BuildRemoteParentMap`
6. `92%` synchronization — `git for-each-ref ...%(upstream:track)` → fills in ahead/behind
7. `96%` pending changes — `git status --porcelain`, read **in the background** (off-UI-thread); the value is **reused** by `UpdateCommitActionTexts(pending)` for the `Commit (n)` counter, without a second `git status` on the UI thread
8. `100%` Done.
- Concurrent calls are **coalesced** (`_isRefreshing`); a refresh in progress is **cancelled** before another one starts.
- Errors become a `MessageBox`. Cancelling restores the UI without touching the existing tree.
- The current branch appears in **bold + highlight color**, with tracking indicators: `(↓M↑N)` (↓ behind / ↑ ahead) only when there is divergence.

## 🌲 Tree structure
- LOCAL and REMOTES combine **two axes**: vertical nesting by **real commit ancestry** (`parentMap`) + horizontal grouping by **`/` in the name** (e.g. `feature/teste` → `feature` folder → `teste` leaf).
- REMOTES is subdivided by remote (`origin`, ...).
- The expansion state is **persisted per repository** in `%APPDATA%\GitExtensions\ZimerfeldTree.treestate.json` (saved with a 500 ms debounce; restored on reopen). During filtering everything is expanded.

## ⚙️ Forced GitFlow mode (auto-organization)
- `GetGitFlowViolations()` checks the expected rules: `master/main` root; `develop` child of master; `feature/*` child of develop (same for each remote).
- If there are violations **and the user has not chosen manually yet**, the tree **auto-organizes** into the GitFlow layout (`BuildGitFlowParentMap` / `BuildGitFlowRemoteParentMap`: master=root, develop→master, feature/release→develop, hotfix→master).
- The warning panel shows the violation count or "showing GitFlow organization".

---

## 🖱️ Buttons and actions — step by step

### Repositories combo (`_cboRepo`)
1. `SelectedIndexChanged`: if it changed → sets `WorkingDir`, **re-enables** GitFlow auto-organization, and `RefreshTree()`.
- The list comes from `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings` (the repository history) + the current working dir.

### Filter box (`_txtFilter`)
1. `TextChanged` → `ApplyFilter`: rebuilds the 3 sections filtering by **substring** (case-insensitive) on the full name; expands everything while there is a filter.

### ↺ button (`_btnRefresh`)
1. Calls `RefreshTree()` → reloads everything with the overlay (see above).

### "Organize as GitFlow" / "Restore real hierarchy" button (`_btnGitFlow`)
1. Sets `_gitFlowUserToggled = true` (a manual choice turns off the auto-organization).
2. Inverts `_gitFlowForced`.
3. `RefreshTree()` → the tree is rebuilt in the chosen layout.

### Pull button (`_btnPull`) → `DoPull` — acts on **HEAD**
1. Disables the button.
2. In the background: `git pull --tags`.
3. On the UI: re-enables the button, `RefreshTree()`, `NotifyRepoChanged()` (notifies GitExtensions and returns focus to the window).
4. If it fails and there is a message → a "Pull failed" `MessageBox`.
- The button shows a **down-arrow icon** (`ctx-pull.png`, blue) **before the text**, replacing the old `↓` character. Label `Pull (M)` when the current branch is M commits behind.
- The top `Branch: <name>` label gets the `↓M` suffix when there are commits to pull (`UpdateBranchLabel`).

### Push button (`_btnPush`) → `DoPush` → `PushCurrent` — acts on **HEAD**
1. **Divergence guard** (`EnsureNotBehindBeforePush`): if the current branch is **behind** (`behind > 0`), it shows the warning "Your branch is N commit(s) behind the remote — you must integrate first. Pull (rebase) and then push?" — **Yes** runs `DoPullRebaseThenPush` (the `PullRebase` service → `git pull --rebase <remote> <branch>` in the background; success → `PushCurrent`; failure/conflict → `RefreshTree` + the `pullRebaseFailedTitle` error, push skipped), **No** cancels. The method always returns `false` when behind: the push, if any, is fired by the rebase continuation, not by the caller.
2. **Preferred:** opens the **native GitExtensions Push dialog in-process** (`StartPushDialog`, `pushOnShow: true` — fires the push automatically on open).
   - On close: `RefreshTree()` + `NotifyRepoChanged()` — **always**, regardless of the return value (`pushCompleted` is not reliable with `pushOnShow`).
3. **Fallback** (without `_openPushDialog`): launches `GitExtensions.exe push` as a new process (fire-and-forget — no refresh possible). A start error → `MessageBox`.
- The button shows an **up-arrow icon** (`ctx-push.png`, green) **before the text**, replacing the old `↑` character. Label `Push (N)` when the current branch is N commits ahead of the remote.

### Commit button (`_btnCommitDedicated`) → `DoCommit`
1. **Preferred:** opens the **native GitExtensions commit window in-process** (`_openCommitDialog` → `IGitUICommands.StartCommitDialog`). This keeps the Commit Template plugins visible (e.g. "Zimerfeld: Auto-summary").
   - Return `true` (a commit happened) → `RefreshTree()` + `NotifyRepoChanged()`.
   - Return `false` (closed without committing) → nothing.
   - Return `null` (unavailable) → falls to the fallback.
2. **Fallback:** `OpenCommitWindow()` fires a **new process** `GitExtensions.exe commit` (plugins do not load in this mode). Error → `MessageBox`.
- The label shows `Commit (n)` with the count of pending changes (`git status --porcelain`) via `UpdateCommitActionTexts`. The count is recomputed: on construction, **after `LoadRepositories`** (with the repo already selected), when the context menu opens, and on each refresh — in the latter **reusing** the value read in the background of `RefreshTreeAsync` (no extra `git status` on the UI thread).
- **Live counter (`FileSystemWatcher`):** a watcher over `_svc.WorkingDir` (`IncludeSubdirectories = true`) updates the `Commit (n)` **silently** as files are created/edited/deleted — with no tree rebuild and no overlay. Flow: `EnsureWorkingDirWatcher()` (called after `ApplyRepoData`, a no-op if the repo did not change) → `RestartWorkingDirWatcher()` creates the watcher → events (on a pool thread) go through `OnWorkingDirChanged`, which **ignores `.git`** via `IsUnderGitDir` (`.gitignore`/`.gitattributes` pass through) and does a `BeginInvoke` to the UI thread → `RestartCommitCountDebounce()` (a 600 ms debounce groups the burst of a single save) → `SilentRefreshCommitCountAsync()` runs only `GetPendingChangesCount()` in the background and calls `UpdateCommitActionTexts(pending)`. Ignoring `.git` avoids the **echo** of our own `git status` (which rewrites the index stat cache). `OnWorkingDirWatcherError` recreates the watcher on a buffer overflow. Cleanup: `StopWorkingDirWatcher()` + `Dispose` of the timer on `FormClosed`.

### GitFlow button (`_btnGitFlowDedicated`) → `DoGitFlow`

![[ScreenshotGitFlow.png]]

1. Creates `GitFlowForm` (modal) and positions it **side by side** with ZimerfeldTree, both centered (if the screen fits; otherwise centered over the window).
2. Subscribes to `RepoMutated`: on each mutation inside GitFlow, it schedules revealing the affected branch and calls `RefreshTree()` **behind the modal** (without stealing focus).
3. `ShowDialog` (blocks).
4. On close: recenters ZimerfeldTree; **only** calls `RefreshTree()` if there was a **release finish** (to focus the new tag) — otherwise it does **not** refresh, since `RepoMutated` already updated it live. It does **not** call `NotifyRepoChanged()` (does not bring the minimized GitExtensions to the front).
- Same flow as the "GitFlow…" menu item. Window details in [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]].

### Restore button (`_btnRestore`) → `DoRestore`

> Renamed from **Back a Version** (`_btnVoltar`) to **Restore** (`_btnRestore`). The fields and images of each of the 10 tabs are in [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

1. Creates `RestoreForm` (modal) and positions it **side by side** with BranchHierarchy, both centered — the same positioning as the GitFlow window.
2. Subscribes to `RepoMutated`: after each successful operation, it calls `RefreshTree()` **behind the modal** (without stealing focus).
3. `ShowDialog` (blocks).
4. On close: it does **not** refresh (`RepoMutated` already updated it live) and does **not** call `NotifyRepoChanged()`. The `RestoreForm` saves the fields via `FormClosing → SaveSettings`.

Three operations available in the Restore window:
- **Restore File** — `git checkout <hash> -- "<file>"`: recovers a specific file from a commit's state and stages it
- **Cherry-Pick** — `git cherry-pick <hash>` or the range `<old>..<recent>`: applies one or more commits onto the current branch
- **Reset Branch** — `git checkout <branch>` (if it is not the current one) + `git reset --mixed|--soft|--hard <hash>` + return to the original branch

The field values are persisted in `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`. See [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

### Delete button (`_btnExcluir`) → `DoDelete`
1. Dynamic text via `UpdateDeleteButtonText()`: `Delete` (0 checked) → `Delete (N)`. Updated on `AfterCheck` and on each rebuild.
2. `DoDelete`: targets = the checked checkboxes if any (`CheckedBranchNodes()`); otherwise the selected node.
   - 2+ → `DoDeleteMultiple` (a single confirmation listing the items, batch deletion, force on an unmerged local).
   - 1 → the individual flow. 0 → nothing.
3. **Protection:** main/master/develop are removed from the targets if `Developer Mode` is off (`IsProtectedBranch`); if nothing is left, it shows a "Protected branch" warning.

**Batch deletion flow (step by step):**

1. Checked items — the button shows `Delete (8)`:

![[ScreenshotBeforeDelete.png]]

2. A single confirmation listing the items, with a **Delete Remotely?** option:

![[ScreenshotConfirmDelete.png]]

3. A progress overlay during the deletion (a list of steps + an **Abort Operation** button):

![[ScreenshotDuringDelete.png]]

4. The tree rebuilt without the items and with updated counters:

![[ScreenshotAfterDelete.png]]

### "Developer Mode" checkbox (`_chkDeveloperMode`)
1. Next to `Show Debug` in the footer. The state is persisted in `ZimerfeldTree.uisettings.json` (`developerMode`) via `SaveUiSettings()`, loaded by `LoadDeveloperMode()`.
2. `Tree_BeforeCheck` blocks **checking** (not unchecking) main/master/develop when it is off.
3. On **turning it off**, `UncheckProtectedBranches()` unchecks the protected ones that were checked.

### Close button (`_btnClose`)
1. `Close()`. On close, the tree's expansion state is saved to disk (`FormClosed → SaveTreeState`).

### Cancel button (overlay, `_btnCancelRefresh`)
1. Disables itself, becomes "Cancelling…" and cancels the `CancellationTokenSource` of the ongoing refresh.

---

## 🌳 Tree interactions
- **Double-click** on a branch leaf → `DoCheckout`.
- **Enter** with a branch selected → `DoCheckout`.
- **Right-click** → selects the node under the cursor and opens the context menu.
- **Checkbox** on each branch/tag (leaf) for multi-selection. Sections/folders have the checkbox **hidden** (`ApplyCheckBoxVisibility` via `TVM_SETITEM`, after `EndUpdate`) and **blocked** (`Tree_BeforeCheck`). `_tree.CheckBoxes = true`.
- **Expand/collapse persistence:** `AfterExpand`/`AfterCollapse` write the stable path (`GetNodeStablePath`) into `_treeStateByRepo` → a 500 ms debounce + `FormClosed` → `treestate.json`. Restored in `RestoreTreeState`, reapplied on **`Shown`** (the native handle already exists; in the constructor without a handle it does not stick).

## 📋 Context menu (visibility depends on the node type)
Defined in `CtxMenu_Opening`: `branch` = local|remote; `local`/`remote`/`tag` specific ones. Orphan separators are hidden.

- **Overlay-style header** (`_miHeader`, a bold `ToolStripLabel` + a `_miHeaderSep` separator at the top): shows the checked-out branch (`Branch: <name>`) — the `ContextMenuStrip` is already a borderless floating window; the header sits at the top and the commands below. Visible both in single and multiple selection. (A deliberate choice instead of a separate borderless `Form`, which would be fragile — see the "Pragmatic over literal" memory.)
- **Pull/Push act on the clicked branch** (not on HEAD): the branch is checked out first and the counters reflect **its** behind/ahead. The toolbar buttons still act on HEAD.

| Item | Visible for | Action (step by step) |
|------|--------------|----------------------|
| **Pull (N)** | local branch | `DoPullForSelected`: **checks out the clicked branch** (`EnsureCurrentBranch`) and then `DoPull`. Icon `ctx-pull.png`. `N` = commits behind of **that** branch. |
| **Push (N)** | local branch | `DoPushForSelected`: checkout of the clicked branch + divergence guard (`EnsureNotBehindBeforePush`: if behind, offers Pull-with-rebase-and-then-push via `DoPullRebaseThenPush`) + `PushCurrent`. Icon `ctx-push.png`. `N` = commits ahead of **that** branch. |
| **Commit (n)** | always | Same as the Commit button → `DoCommit`. Shows the pending count. |
| **Checkout** | branch (local/remote) | `DoCheckout`: local → `git checkout "<name>"`; remote → `CheckoutRemoteAsLocal` = `git checkout -b "<local>" --track "<origin/...>"`. Success → `RefreshTree` + `NotifyRepoChanged`; error → `MessageBox`. |
| **New branch from here…** | local or tag | `DoNewBranch`: asks for a name (`InputDialog`) → `git checkout -b "<new>" "<ref>"`. Success → refresh + notify. |
| **Merge into current branch** | local | `DoMerge`: confirms → `git merge "<name>"`. Success → refresh; error → `MessageBox`. |
| **Rebase onto current branch** | local | `DoRebase`: confirms → `git rebase "<name>"`. Success → refresh; error → `MessageBox`. |
| **Rename…** | local | `DoRename`: asks for a new name → `git branch -m "<old>" "<new>"`. |
| **Delete…** | local/remote/tag | `DoDelete`: confirms; tag → `git tag -d` **+** `git push <remote> --delete <tag>` (removes local **and** remote; "remote ref does not exist" is treated as success); remote → `git push <remote> --delete <branch>`; local → `git branch -d`. If "not fully merged" → offers to **force** (`git branch -D`). |
| **GitFlow…** | branch | Same as the GitFlow button → `DoGitFlow`. |
| **Restore…** | current branch ≠ `develop` | Same as the Restore button → `DoRestore` (opens the Restore window). Does not depend on the clicked node — always acts on the checked-out branch. |
| **Expand all** | always | `node.ExpandAll()`. |
| **Collapse all** | always | `CollapseRecursive(node)`. |
| **Refresh** | always | `RefreshTree()`. |

## 🎨 Icons by node type (`NodeIcons`)
- LOCAL (monitor) · REMOTES (cloud) · TAGS (ribbon) · path folder (amber).
- `master`/`main` golden shield · `develop` (embedded PNG) · `feature/*` folder=branch, leaf=green leaf · `bugfix/*` ladybug · `release/*` package · `hotfix/*` warning · `support/*` gear.
- Several use an **embedded PNG** in `Resources\` with a GDI+ fallback.

## 🔗 Related
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF plugin for GitExtensions]]
- [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]]
