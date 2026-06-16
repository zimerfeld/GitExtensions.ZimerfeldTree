# GitExtensions.ZimerfeldTree

![Icone](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/icon-128.png)

- Help keep this project always updated 💜

[![GitHub Sponsor](https://img.shields.io/badge/Sponsor-zimerfeld-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white)](https://github.com/sponsors/zimerfeld) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; [![Ko-fi](https://img.shields.io/badge/Ko--fi-Buy%20me%20a%20coffee-FF5E2B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/C0D621FCGD)

**Version:** 1.0.324  
**Updated:** 2026-06-16

A [GitExtensions](https://gitextensions.github.io/) plugin that displays branches **hierarchically** in a tree view, including child branches.

![ZimerfeldTree - BranchHierarchy](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBranchHierarchy.png)

[English](README.en-US.md) | [Português](README.pt-BR.md)

[...More information](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree "More information about GitExtensions.ZimerfeldTree package")

---

## Features

### Hierarchical branch view

- Non-modal window that stays open alongside GitExtensions. The title bar is **`ZimerfeldTree - BranchHierarchy`**; helper windows are `ZimerfeldTree - GitFlow` and `ZimerfeldTree - Restore`.
- Tree split into three fixed sections: **LOCAL**, **REMOTES**, and **TAGS**.
- **LOCAL** and **REMOTES** combine real commit ancestry / GitFlow organization with path grouping by `/`. For example, `feature/test` appears as `feature` -> `test`; when `feature/*` is a child of `develop`, the tree shows `develop` -> `feature` -> `test`.
- **TAGS** are grouped by `/` without ancestry calculation.
- Empty sections show `(no local branch found)`.
- The window opens centered on screen, has a fixed size, and exposes the standard Windows minimize and close buttons. Maximize is disabled.
- The window is independent from GitExtensions; minimizing GitExtensions does not affect BranchHierarchy.
- Asynchronous loading: the window appears immediately, then shows a centered progress panel while repository data is read in the background.
- Optimized hierarchy building: parent detection uses a single `git log --all` to build the commit graph in memory and resolves parents with BFS, avoiding the old O(N^2 x subprocess) bottleneck.
- Explicit reload overlay: the progress panel appears on first open and during explicit reloads or mutations such as refresh, checkout, new branch, merge, rename, delete, GitFlow, Restore, Pull/Push/Commit, genuine GitExtensions repository changes, and repository switches.
- Read-only step list: the overlay accumulates the running steps and keeps the final "Completed." message visible for one second.
- Cancel button in the overlay: cancels loading between git steps while preserving the previous tree data.
- The form is blocked during loading and re-enabled when loading finishes or is canceled.
- A centered **Close** button is available at the bottom of the window; shortcut: **Esc**.

### Working Directory and Branch selector

- The top **Working Directory:** row contains a fixed label, a selection-only ComboBox populated from the GitExtensions dashboard history, and a `Branch: <name>` label for the checked-out branch.
- Selecting another repository reloads the tree for that working directory.
- The repository list is refreshed whenever GitExtensions switches repositories.
- The current branch is highlighted with bold text and the system selection color.
- Tree sections show counters: `LOCAL (N)`, `REMOTES (N)`, `TAGS (N)`.
- The bottom status bar shows `Local: N | Remote: N | Tags: N`.

### Real-time filter

- The search field filters branches across all sections at the same time.
- Parent nodes are preserved when they contain matching children.

### Pull / Push / Commit / GitFlow / Restore buttons

Shown above the tree when a branch is checked out:

- **Pull** / **Pull ↓N** runs `git pull --tags`, fetching tracked branch commits and all remote tags. The button shows a blue **down-arrow icon** (it replaces the old `↓` character); `↓N` is the number of remote commits not yet pulled.
- **Push** / **Push ↑N** opens the native GitExtensions Push dialog. The button shows a green **up-arrow icon** (replaces the old `↑` character); `↑N` is the number of local commits not yet pushed.
  - When the checked-out branch is **behind** the remote (`↓N > 0`), Push is **blocked** by a warning ("your branch is N commit(s) behind — pull first") that offers to Pull right away, preventing the `non-fast-forward` rejection.
- **Commit** / **Commit (N)** opens the native GitExtensions Commit window. `(N)` is shown only when there are pending changes.
- On window open, a background `git fetch` of the current branch's upstream refreshes the counts off the UI thread (the window stays fast/offline-safe), and the `Branch: <name>` label gains a `↓N` suffix when there are commits to pull.
- After Push, Pull, or Commit, the tree refreshes automatically and the button counters are recalculated.
- **GitFlow** opens the GitFlow operation window.
- **Restore** opens the Restore window with three history recovery operations.
- **Delete** / **Delete (N)** deletes the selected branches or tags.
- Button icons reuse the same embedded icons used by the context menu (Pull/Push use the new down/up-arrow icons).

### Multi-select and branch deletion

- Each local branch, remote branch, and tag has a checkbox to the left of its name. Section nodes and path folders are not selectable.
- Check one or more items for batch deletion.
- The **Delete** button changes dynamically: `Delete` when nothing is checked, `Delete (N)` when items are checked.
- With two or more checked items, deletion runs as a batch with a single confirmation dialog.
- With one checked item, that item is deleted.
- With no checked items, the selected tree node is deleted.
- For a local branch that is not fully merged, forced deletion is offered.
- Tags are deleted locally with `git tag -d` and remotely with `git push <remote> --delete <tag>`.
- The context menu follows the same selection rules.
- After deletion, the tree is rebuilt and checkboxes are cleared.

Complete batch deletion flow:

**1. Before - checked items** (button shows `Delete (8)`):

![Before deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBeforeDelete.png)

**2. Single confirmation** listing every item, with the **Delete Remotely?** option:

![Confirm deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotConfirmDelete.png)

**3. During deletion** - progress overlay with the step list and **Abort Operation** button:

![During deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotDuringDelete.png)

**4. After** - the tree is rebuilt without the deleted items and with updated counters:

![After deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotAfterDelete.png)

#### Main branch protection and Developer Mode

- **main**, **master**, and **develop** branches, local or remote, are protected by default and cannot be selected or deleted.
- The **Developer Mode** checkbox at the bottom of the window unlocks selection and deletion for those branches.
- Turning Developer Mode off automatically unchecks any protected branch.
- Developer Mode is persisted in `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json` together with **Show Debug**.

### Automatic focus after Commit

- After closing the GitExtensions Commit window, BranchHierarchy automatically regains focus.
- The tree is refreshed and the window is brought to the front.

### "Show Debug" checkbox

The **Show Debug** checkbox enables identification tooltips for plugin controls:

- Tooltip line 1: `TYPE: <control type>`.
- Tooltip line 2: `ID: <internal name>`.
- The window tooltip shows `TYPE: BranchHierarchyForm` and `Handle: 0x<HWND>`.
- GitFlowForm also shows its type and handle when Show Debug is enabled.
- Works in BranchHierarchy and GitFlow windows.
- The setting is persisted in `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`.

### Developer Mode checkbox

The **Developer Mode** checkbox controls protection for main branches:

- **Off by default:** **main**, **master**, and **develop** are protected.
- **On:** those specific branches can be selected and deleted.
- Turning it off clears any protected branch that was selected.
- The setting is persisted with **Show Debug**.

### Tree state persistence

- Expansion and collapse state is saved per working directory, including root nodes, branches, and path folders.
- Nodes are identified by a stable path such as `LOCAL|master|develop|feature`.
- State is saved when nodes expand/collapse, with a 500 ms debounce, and when the window closes.
- First open restores the previous tree state after repository data is loaded.
- New repositories start with LOCAL fully expanded and REMOTES/TAGS collapsed except for their roots.
- While filtering, all nodes are expanded automatically to show matches.

### Automatic GitFlow organization

- The plugin checks whether the real ancestry follows GitFlow rules: `master`/`main` at the root, `develop` under `master`, and `feature/*`, `release/*`, and `hotfix/*` under the expected parents.
- When the hierarchy does not match GitFlow, the plugin automatically applies GitFlow organization in the tree and shows a warning.
- The warning button can switch back to the real git ancestry.
- Manual choice is respected until the repository changes or the window is reopened.

### Automatic refresh

- The tree reloads automatically when the checked-out branch changes, when GitExtensions switches repositories, or when a repository is initialized/reopened.
- The **Refresh** button triggers a manual reload.

### Context menu

Each item has an embedded 16 x 16 icon generated from `Resources/ctx-*.png`:

| Icon                                                                                                                                                           | Item                       | Available for                                                                        |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------- | ------------------------------------------------------------------------------------ |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-pull.png" width="16" height="16">       | Pull (N)                   | Local branch - `N` = commits behind; checks out the clicked branch first, then pulls |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-push.png" width="16" height="16">       | Push (N)                   | Local branch - `N` = commits ahead; checks out the clicked branch first, then pushes (blocked when behind) |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-commit.png" width="16" height="16">     | Commit (N)                 | Always - opens the GitExtensions Commit window; `N` is the number of pending changes |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-checkout.png" width="16" height="16">   | Checkout                   | Local, remote, tag                                                                   |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-new-branch.png" width="16" height="16"> | New branch from here...    | Local, tag                                                                           |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-merge.png" width="16" height="16">      | Merge into current branch  | Local                                                                                |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rebase.png" width="16" height="16">     | Rebase onto current branch | Local                                                                                |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rename.png" width="16" height="16">     | Rename...                  | Local                                                                                |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-delete.png" width="16" height="16">     | Delete...                  | Local, remote, tag                                                                   |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-gitflow.png" width="16" height="16">    | GitFlow...                 | Branch (local/remote/tag)                                                            |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-restore.png" width="16" height="16">    | Restore...                 | When current branch is not `develop`                                                 |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-expand.png" width="16" height="16">     | Expand all                 | Always                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-collapse.png" width="16" height="16">   | Collapse all               | Always                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-refresh.png" width="16" height="16">    | Refresh                    | Always                                                                               |

The context-menu **Pull/Push act on the branch you right-clicked** (not on HEAD): the clicked branch is checked out first, then pulled/pushed, and their `(N)` counters show that branch's own behind/ahead. Push on a behind branch is blocked by the same "pull first" warning as the button. They appear only for local branches and sit just before **Commit**. The popup also shows, at the very top, a **header with the currently checked-out branch** (`Branch: <name>`).

The **Commit** item recalculates the pending working tree count every time the menu opens. It opens the native Commit window in the already running GitExtensions process when possible, so Commit Template plugins are already loaded. If the repository shown in BranchHierarchy differs from the active GitExtensions repository, it opens through a new process as a fallback.

### GitFlow Initialize button

The **GitFlow Initialize** button applies the default GitFlow configuration keys to the current repository:

| Key                         | Default value |
| --------------------------- | ------------- |
| `gitflow.branch.main`       | `main`        |
| `gitflow.branch.develop`    | `develop`     |
| `gitflow.prefix.feature`    | `feature/`    |
| `gitflow.prefix.bugfix`     | `bugfix/`     |
| `gitflow.prefix.release`    | `release/`    |
| `gitflow.prefix.hotfix`     | `hotfix/`     |
| `gitflow.prefix.support`    | `support/`    |
| `gitflow.prefix.versiontag` | _(empty)_     |

This is equivalent to running `git config <key> <value>` for each row.

## Project structure

```text
ZimerfeldTree/
|-- src/
|   `-- GitExtensions.ZimerfeldTree/
|       |-- ZimerfeldTreePlugin.cs             # MEF entry point (IGitPlugin)
|       |-- BranchHierarchyForm.cs             # Main branch hierarchy window
|       |-- GitFlowForm.cs                     # GitFlow operation window
|       |-- RestoreForm.cs                     # Restore window
|       |-- BranchHierarchyService.cs          # Git logic
|       |-- BranchNode.cs                      # Models
|       |-- NodeIcons.cs                       # Tree icons
|       |-- PluginIcon.cs                      # Plugin/window icon
|       |-- Resources/                         # Embedded PNGs
|       |-- GitExtensions.ZimerfeldTree.csproj
|       `-- GitExtensions.ZimerfeldTree.nuspec # NuGet package metadata
|-- build.ps1                                  # Build, versioning, and deploy script
|-- README.md                                  # Language selector
|-- README.pt-BR.md                            # Portuguese documentation
`-- README.en-US.md                            # English documentation
```

### GitFlow window

![GitFlow window](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotGitFlow.png)

- Closing the GitFlow window re-centers BranchHierarchy and does not trigger an unnecessary refresh, except after finishing a release, when the tree reloads once to focus the new tag.
- After a successful **Start**, the "Manage existing branches" panel is preselected with the new branch.
- After any GitFlow action succeeds, the BranchHierarchy tree refreshes immediately while focus stays in GitFlow.
- The affected branch is checked out and revealed in the LOCAL tree section.

### Start and Finish rules per type

The diagram summarizes, for each branch type, the **base used on Start**, the **branch created**, and the **merge target on Finish**:

![Start and Finish rules per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotStartFinish.png)

- **feature** — starts from `develop` (or another `feature/*`, optional); finishes into `develop` or the based-on parent
- **bugfix** — starts from a `release/*` (required pick); finishes into `develop` or the parent
- **release** — starts from `develop` (fixed base); finishes into `main` (`merge --no-ff` + tag) and `develop`, pushing main/develop/tag
- **hotfix** — starts from `main` (fixed base); finishes into `main` (`merge --no-ff` + tag) and `develop`
- **support** — starts from a production **tag** (required pick); finishes into `main` only, with no tag and without touching `develop`
- Common to every Finish: optional fetch, deletion of the local and remote branch (unless **Keep**), and re-linking of children in the tree

The full `git` command flow for each type, from Start to Finish:

![Full Start to Finish flow per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotFlowPerType.png)

### Hierarchy: how the node is positioned in the tree

Git stores only each branch's tip commit, not its origin. To nest the new branch under its base, Start uses one of these mechanisms:

![Hierarchy: empty commit and based-on override](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotHierarchyBasedOn.png)

- **empty commit** (base = develop/main, based-on checked) — `git commit --allow-empty` makes the tip diverge; real ancestry nests the node
- **based-on override** (base = custom `feature/*`, based-on checked) — writes `.git/zimerfeld-basedon.json` (purely visual link, clean history)
- **GitFlow / path rule** (no based-on) — plain `checkout -b`; the node sits at the base tip and is grouped by the GitFlow rule + prefix
- On Finish, `RebaseBasedOnOnFinish` removes the finished branch's link and re-points its children to the merge target, keeping the tree connected

### GitFlow window - base branch on Start

The **Start branch** panel includes a **based on:** option:

- When disabled, the plugin uses the standard base branch for the selected GitFlow type.
- When enabled, the new branch starts from the selected branch.
- This is useful for branches that should be visually nested under another branch.

### GitFlow window - "Manage existing branches"

The plugin executes native git commands directly and does **not** require the `git-flow` binary.

| Action  | Behavior                                                             |
| ------- | -------------------------------------------------------------------- |
| Publish | Pushes the selected local branch and sets upstream tracking          |
| Track   | Creates a local tracking branch from the selected remote branch      |
| Update  | Pulls updates for the selected branch                                |
| Finish  | Merges the selected branch into the configured target and removes it |

Operational behavior:

- The GitFlow window keeps focus after every command.
- BranchHierarchy refreshes in the background after successful operations.
- The affected branch, or the resulting branch after finish, is selected in the tree.
- When opening the window, if the checked-out branch matches a GitFlow type, the type and branch dropdowns are preselected.

#### Error handling

When a git command fails, the output is displayed in the window and a warning is shown. Missing destination branches point the user to the existing branches and `gitflow.branch.*` configuration. Merge conflicts leave the repository in merging state and must be resolved manually with `git merge --abort` or by resolving conflicts and committing.

### Restore window

![Restore window](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestore.png)

Opens from **Restore** and provides three operations to recover states from git history.

#### Restore File

- Restores a specific file from a selected commit.
- Lets the user choose the commit and file path.
- Applies the restored file to the working tree without changing branch history.

#### Cherry-Pick

- Applies a selected commit onto the current branch.
- Useful for recovering one specific change without resetting the branch.
- Git conflict handling remains the native git behavior.

#### Reset Branch

- Resets a branch to a selected commit.
- If the selected branch is not the current one, the plugin checks it out, applies the reset, and returns to the original branch automatically.
- Intended for deliberate history recovery operations.

#### Window behavior

- The Restore window is modal and positioned next to BranchHierarchy, with both windows centered on screen.
- After successful operations, BranchHierarchy refreshes in the background without stealing focus from Restore.
- The last used values are persisted and restored on the next open.
- The **About Restore** link explains each operation.

### Restore window - general behavior

![Restore window](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestore.png)

- Opens when clicking **Restore** in BranchHierarchy.
- Keeps focus while BranchHierarchy refreshes in the background.
- Persists recent field values in `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`.

### Icons

The plugin icon is used in:

- The GitExtensions **Plugins** menu.
- The plugin window title bar and Windows taskbar.

The Tree of Life icon is the embedded 16 x 16 PNG [`Resources/ico.png`](src/GitExtensions.ZimerfeldTree/Resources/ico.png), loaded once by [`PluginIcon.cs`](src/GitExtensions.ZimerfeldTree/PluginIcon.cs) and cached for the process lifetime.

### Icons by branch type

Each tree node receives a 16 x 16 icon. GitFlow types have dedicated icons:

| Icon                                                                                                                                                    | Node type            | Description           |
| ------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------- | --------------------- |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/master.png" width="16" height="16">  | `master` / `main`    | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/develop.png" width="16" height="16"> | `develop`            | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/feature.png" width="16" height="16"> | `feature` folder     | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/folha.png" width="16" height="16">   | `feature/*` children | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/bugfix.png" width="16" height="16">  | `bugfix/*`           | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/release.png" width="16" height="16"> | `release/*`          | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/hotfix.png" width="16" height="16">  | `hotfix/*`           | Embedded custom image |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/support.png" width="16" height="16"> | `support/*`          | Embedded custom image |

#### Custom icons

| Type                    | Resource                      | Visual                    |
| ----------------------- | ----------------------------- | ------------------------- |
| **LOCAL** section       | `Resources/local.png`         | steel-blue monitor        |
| **REMOTES** section     | `Resources/remotes.png`       | dark-blue cloud           |
| **TAGS** section        | `Resources/tags.png`          | purple tag/ribbon         |
| remote group (`origin`) | `Resources/origin.png`        | blue cloud                |
| remote branch           | `Resources/remote-branch.png` | green fork                |
| tag                     | `Resources/tag.png`           | teal tag                  |
| `master` / `main`       | `Resources/master.png`        | golden shield             |
| `develop`               | `Resources/develop.png`       | crossed wrench and hammer |
| `feature` folder        | `Resources/feature.png`       | branch sprout             |
| `feature/*`             | `Resources/folha.png`         | green leaf                |
| `release/*`             | `Resources/release.png`       | package box               |
| `bugfix/*`              | `Resources/bugfix.png`        | red bug                   |
| `hotfix/*`              | `Resources/hotfix.png`        | red fire extinguisher     |
| `support/*`             | `Resources/support.png`       | first-aid kit             |

The plugin remains self-contained: images are embedded in the DLL and do not depend on external files on the user's machine.

### Keyboard and mouse shortcuts

- **Double-click** a branch or tag to checkout.
- **Enter** checks out the selected node.
- **Esc** closes the BranchHierarchy window.
- Right-click opens the context menu.

### Persistent non-modal window

- The window can stay open during normal GitExtensions work.
- Actions return focus to BranchHierarchy when they complete.
- GitFlow is the main exception because it opens its own modal window and keeps focus while open.

## Dependencies

### Required for use

| Dependency               | Version / path                                   | Purpose                                |
| ------------------------ | ------------------------------------------------ | -------------------------------------- |
| **Windows**              | Windows desktop                                  | WinForms plugin host                   |
| **Git**                  | Available in PATH or GitExtensions configuration | Repository operations                  |
| **GitExtensions**        | Compatible .NET 9 build                          | Host application that loads the plugin |
| **ZimerfeldTree plugin** | Installed DLL                                    | The plugin itself                      |

> **Attention:** GitExtensions 3.x (`.NET Framework 4.8`) is incompatible; the plugin requires `net9.0-windows`.

### Conditional - build/development only

| Dependency     | Purpose                       |
| -------------- | ----------------------------- |
| **.NET SDK**   | Build the project             |
| **NuGet CLI**  | Generate `.nupkg` packages    |
| **PowerShell** | Run build and install scripts |

## Installation

### Option A - PowerShell (recommended)

From the repository root:

```powershell
cd tools
.\install.ps1
```

![Installation via install.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotInstall.png)

Restart GitExtensions after installation.

### Option B - Manual

Build or extract:

```text
tools\net9.0-windows\GitExtensions.Plugins.ZimerfeldTree.dll
```

Copy `GitExtensions.Plugins.ZimerfeldTree.dll` to:

```text
C:\Program Files\GitExtensions\Plugins\
```

Restart GitExtensions. The plugin should appear under **Plugins** and in **Settings -> Plugins -> ZimerfeldTree**.

## Uninstall

```powershell
cd tools
.\uninstall.ps1
```

![Uninstall via uninstall.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUninstall.png)

This removes the plugin DLL from the GitExtensions Plugins folder. GitExtensions itself is not affected.

## DLL update

Use the update script to replace only the installed DLL:

```powershell
cd tools
.\update-dll.ps1
```

![Update via update-dll.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUpdate.png)

## Build

```powershell
# Increments version, builds, and creates the .nupkg
# Run as Administrator to also copy the DLL into Plugins\
pwsh C:\NUGET\ZimerfeldTree\build.ps1
```

The script:

1. Reads and increments the version in the `.nuspec`.
2. Synchronizes the `.csproj` version.
3. Builds in Release mode.
4. Copies the DLL to `C:\Program Files\GitExtensions\Plugins\` when run as Administrator.
5. Packs the `.nupkg` in the repository root.

Successful build:

![Successful build](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBuild.png)

Build with no changes:

![Build with no changes](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotNoBuild.png)

## Branch hierarchy - limitations

### Hierarchy is by branch ancestry and path grouping

The plugin combines git commit ancestry with path grouping. Git itself does not store where a branch was created from; it stores only which commit each branch points to. When the ancestry is ambiguous, path grouping and GitFlow rules help make the tree readable.

### A real branch cannot be only a folder node

If a branch name is also a prefix for other branches, the tree must represent both concepts: the actual branch and the folder path. This avoids hiding a real ref behind a visual grouping node.

### GitFlow does not model feature-under-feature

GitFlow defines a fixed hierarchy where `feature/*` branches derive from `develop` and are siblings. Sub-features are usually modeled as commits in the same branch or sibling branches with a common prefix.

### Two branches on the same commit are not parent and child

Example:

```text
# No hierarchy - both point to commit c19d7dc
master
develop

# Hierarchy - gridsolo is one commit ahead
develop
`-- feature/gridsolo
```

This is a git limitation, not a plugin limitation.

Automatic solution: when using **based on:** in GitFlow -> Start, the plugin can create an empty commit in the new branch:

```powershell
git commit --allow-empty -m "Start <branch>"
```

That gives the new branch a distinct tip commit, making parent-child detection possible.

## Usage

1. Open GitExtensions.
2. Go to **Plugins -> ZimerfeldTree**.
3. Use the tree to inspect, filter, checkout, create, merge, rebase, rename, delete, or run GitFlow/Restore operations.

## Integrated plugins

### [GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)

GitExtensions plugin that automatically generates a commit message summarizing staged changes in one Conventional Commits-style sentence (`feat` / `fix` / `docs` / `test` / `chore`).
