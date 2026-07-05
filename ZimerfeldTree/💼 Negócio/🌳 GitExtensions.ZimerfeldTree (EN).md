---
tipo: negocio
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-07-01 (behind push: the warning now offers **Pull with rebase and then push automatically** — `git pull --rebase` reapplies the local commits on top of the remote ones, with no merge, leaving the branch fast-forward; the `PullRebase` service method + `DoPullRebaseThenPush`) | 2026-06-27 (live Commit counter: a FileSystemWatcher on the working directory folder updates the `(N)` of the Commit button silently, with debounce and ignoring `.git`) | 2026-06-26 (funding: FUNDING.yml with github+ko_fi, NuGet version/downloads badges and a "why donate" line in the READMEs) | 2026-06-18 (doc: flexible GitFlow — feature as a child of a feature; cascading finish up to develop. 1.0.323: Pull/Push icons on the buttons and menu; remote check on open via a fetch of the current branch; Pull/Push menu acts on the clicked branch; a warning blocks a push when the branch is behind; header with the checked-out branch in the context menu)
tags: [projeto, csharp, gitextensions, plugin, winforms]
status: ativo
linguagem: C#
versao: 1.0.361
repo: C:\GitExtensions\GitExtensions.ZimerfeldTree
---

# 🌳 GitExtensions.ZimerfeldTree

> [!info] This note mirrors the repository `README.md`
> The README content (features, dependencies, installation, structure and limitations) lives here in the vault. The **detailed flows of each window** are in [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]], [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]] and [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

## 💜 Support the project / Funding
Donation channels (the **Sponsor** button + badges at the top of the READMEs):
- **GitHub Sponsors:** [zimerfeld](https://github.com/sponsors/zimerfeld) · **Ko-fi:** [C0D621FCGD ☕](https://ko-fi.com/C0D621FCGD)
- **`.github/FUNDING.yml`:** declares `github: zimerfeld` **and** `ko_fi: C0D621FCGD` (shows the native Sponsor button with both).
- **Social proof in the README:** version badges + **NuGet downloads** (`shields.io/nuget/v` and `/dt` of the `GitExtensions.ZimerfeldTree` package) + a short "why donate" line (maintenance in free time + compatibility with new GitExtensions versions).

## 🎯 Goal
Plugin for **[GitExtensions](https://gitextensions.github.io/)** that displays the repository branches **hierarchically** as a tree (showing child branches), instead of the default flat list. It has its own drawn/embedded "Tree of Life" icon (GDI+ / `Resources/ico.png`).

## 📂 Project structure
```
C:\GitExtensions\ZimerfeldTree\
├─ src\GitExtensions.ZimerfeldTree\        # plugin code
│   ├─ ZimerfeldTreePlugin.cs              # MEF entry point (IGitPlugin)
│   ├─ BranchHierarchyForm.cs              # main window: hierarchical branch tree
│   ├─ GitFlowForm.cs                      # Git Flow window: start/publish/track/update/finish
│   ├─ RestoreForm.cs                      # Restore window: 10 "time travel" tabs (restore/revert/reset/reflog/rebase…)
│   ├─ BranchHierarchyService.cs           # git logic: collection, hierarchy, Git Flow
│   ├─ BranchNode.cs                       # models: BranchInfo class + BranchType enum
│   ├─ NodeIcons.cs                        # 16×16 tree icons (GDI+ + embedded PNGs)
│   ├─ PluginIcon.cs                       # plugin/window icon (Resources/ico.png)
│   ├─ Resources\                          # embedded PNGs (node, menu and plugin icons)
│   ├─ GitExtensions.ZimerfeldTree.csproj
│   └─ GitExtensions.ZimerfeldTree.nuspec  # NuGet package metadata
├─ build.ps1                               # build + versioning + deploy
├─ README.md                               # rich documentation
└─ ZimerfeldTree\                          # 🧠 this memory vault
```

## ⚙️ Technical stack
- **Language:** C# (`net9.0-windows`), `Nullable` + `ImplicitUsings`, `LangVersion=latest`
- **UI:** WinForms (`UseWindowsForms`)
- **Output type:** `Library` (a DLL loaded by GitExtensions, not an exe)
- **AssemblyName:** `GitExtensions.Plugins.ZimerfeldTree`
- **Root namespace:** `GitExtensions.ZimerfeldTree`
- **Plugin model:** MEF (`System.ComponentModel.Composition`) — see [[🧩 Plugin MEF para GitExtensions (EN)|MEF plugin for GitExtensions]]
- **External references** (from `C:\Program Files\GitExtensions\`, `Private=false`, not copied):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`

## 📄 Source files (`src\GitExtensions.ZimerfeldTree\`)
| File | Lines | Role |
|---------|-------:|-------|
| `BranchHierarchyForm.cs` | ~2066 | Main non-modal window (most of the UI) |
| `BranchHierarchyService.cs` | ~831 | Runs git commands and parses the output |
| `GitFlowForm.cs` | ~758 | Modal window that drives `git flow` commands (pure git) |
| `RestoreForm.cs` | ~1473 | Modal window: 10 recovery/undo tabs (restore file/tree/tag, cherry-pick, revert, reset, new branch/tag, reflog, discard, rebase) |
| `NodeIcons.cs` | ~381 | 16×16 GDI+ icons + embedded PNGs (ImageList) |
| `ZimerfeldTreePlugin.cs` | ~238 | The plugin's MEF entry point (IGitPlugin) |
| `BranchNode.cs` | ~41 | Models: `BranchInfo` class + `BranchType` enum (Local/Remote/Tag) |
| `PluginIcon.cs` | ~33 | Plugin/window icon (`Resources/ico.png`), loaded once and cached |
| `*.nuspec` / `*.csproj` | — | NuGet/MSBuild manifests (read by `build.ps1`) |

### 🖼️ Resources (`src\GitExtensions.ZimerfeldTree\Resources\`)
| Group | Files | Use |
|-------|----------|-----|
| Plugin/window | `ico.png` | "Tree of Life" icon (Plugins menu + title bar) |
| Tree sections | `local.png`, `remotes.png`, `tags.png` | LOCAL / REMOTES / TAGS headers |
| Branch nodes | `master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png` | Icons by GitFlow branch type |
| Remote / tag | `origin.png`, `remote-branch.png`, `tag.png` | Remote group (rocket), remote branch, tag |
| Context menu | `ctx-checkout.png`, `ctx-collapse.png`, `ctx-commit.png`, `ctx-delete.png`, `ctx-expand.png`, `ctx-gitflow.png`, `ctx-merge.png`, `ctx-new-branch.png`, `ctx-pull.png`, `ctx-push.png`, `ctx-rebase.png`, `ctx-refresh.png`, `ctx-rename.png`, `ctx-restore.png` | Tree context menu icons. `ctx-pull` (blue ↓ arrow) / `ctx-push` (green ↑ arrow) also used on the Pull/Push buttons — generated via Pillow (see `tools\make_pull_push_icons.py`) |

> Each `<EmbeddedResource>` is **conditional on the file existing** (`Condition="Exists(...)"`). At runtime, `NodeIcons.LoadEmbedded` reads the resource by `GitExtensions.ZimerfeldTree.Resources.<file>` and resizes to 16×16. If missing/unreadable, it falls back to the **GDI+ reserve glyph** — the build never breaks for a missing image.

## ✨ Main features
- A **non-modal** window, singleton per session, opens **centered** and resizable (`Sizable`), independent of GitExtensions. Title bar: **`ZimerfeldTree - BranchHierarchy`** (helpers: `ZimerfeldTree - GitFlow`, `ZimerfeldTree - Restore`) — the **ZimerfeldTree** prefix is always kept, followed by the specific window name. `BranchHierarchyForm` is just the internal C# class name
- A tree in 3 fixed sections: **LOCAL**, **REMOTES**, **TAGS**, with `(N)` counters and a status bar `Local: N | Remote: N | Tags: N`
- LOCAL/REMOTES combine **real ancestry** (commit / GitFlow kinship) **+ grouping by path** (`/`). E.g. `feature/teste` → `feature` folder → `teste` leaf
- **Asynchronous loading**: the window opens immediately with the controls rendered but empty (no computed data) + a progress overlay (0→100%), a cumulative list of the 8 steps, a Cancel button, the form blocked during the load. The constructor does **not** do git; everything is read in the background (`Task.Run`) fired by `Shown` (`FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true, finalDelay:false)`). On reloads the overlay closes after 1 s at "Done."; on the **first open** it closes as soon as the tree is populated (without the delay)
- **Optimized hierarchy:** a single `git log --all` builds the commit graph in memory, parents via BFS → **O(commits)** instead of O(N²×subprocess)
- **Overlay only on the first display and on explicit reloads** — it does not appear when reactivating after closing GitFlow/Restore (the tree is already updated live) nor on the echo of our own `NotifyRepoChanged`
- A **Working Directory** selector (a combo read from `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) and the **current branch in bold** + highlight color
- **Real-time filtering** in all sections (case-insensitive substring), preserving parent nodes with matching children
- **Pull / Push / Commit / Delete / GitFlow / Restore buttons** above the tree (when there is a checked-out branch); counters `↓N` / `↑N` / `(N)`. **Pull/Push show arrow icons** (↓ blue / ↑ green) instead of the old `↓`/`↑` characters. They act on **HEAD**
- **Live Commit counter** — a `FileSystemWatcher` over the working directory folder (with subfolders) updates the `(N)` of the Commit button **silently** on file create/edit/delete, with no tree rebuild and no overlay. Bursts are grouped by a 600 ms debounce → a single `git status` in the background; changes in `.git` are ignored (avoids echo; `.gitignore`/`.gitattributes` count). Re-pointed when switching repos. Detail in [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- **Remote check on open** — a `git fetch` of the current branch's upstream runs in the background after the window appears (offline-safe on open); it corrects the Pull/Push counters and adds `↓N` to the `Branch:` label
- **Push protected against divergence** — if the branch to push is **behind** the remote, a warning offers to **Pull with rebase and then push automatically** (`git pull --rebase` reapplies the local commits on top of the remote ones, with no merge commit → fast-forward branch → accepted push); a rebase with a conflict is reported and the push is skipped, avoiding the `non-fast-forward` rejection
- **Multi-selection via checkbox** — each branch (local/remote) and tag has a checkbox (sections and folders do not); checking 2+ enables batch deletion. The **Delete** button changes to `Delete (N)` and the context menu reduces to **Delete + Refresh**
- **"Developer Mode" checkbox** (next to Show Debug) — **off (default):** `main`/`master`/`develop` are **protected**, with the checkbox blocked (they cannot be checked or deleted); **on:** enables checking/deleting those specific branches. Turning the mode off **automatically unchecks** any checked main/master/develop. The state is persisted in `ZimerfeldTree.uisettings.json`
- **Automatic focus after Commit** — the window regains focus and refreshes the tree when the Commit window closes
- **"Show Debug" checkbox** — `TYPE:`/`ID:` tooltips on all controls (and the window Handle); the state is persisted in `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`
- **Tree state persistence** (expand/collapse) per Working Directory in `ZimerfeldTree.treestate.json` — a stable path per node (e.g. `LOCAL|master|develop|feature`), a 500 ms debounce + save on close, restored on the `Shown` of the first open
- **Automatic organization as GitFlow** — detects a non-standard hierarchy and auto-organizes; a "Restore real hierarchy" / "Organize as GitFlow" button
- **Automatic refresh** on checkout, repository switch, init/reopen; a manual **Refresh** button
- **Context menu** with embedded icons (Pull, Push, Commit, Checkout, New branch, Merge, Rebase, Rename, Delete, GitFlow…, Restore…, Expand/Collapse, Refresh) + a **header at the top** with the checked-out branch. **Pull/Push act on the clicked branch** (checking it out first), with their own counters
- **GitFlow Initialize button** — applies the standard `gitflow.*` keys at once (see [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]])
- **Restore** (`RestoreForm`) — the "time travel" hub (980 px, 10 tabs, from the safest to the most destructive): Emergency Plan (branch←tag), Restore File (with **Browse…** restricted to the repo root), Restore Tree, Cherry-Pick, **Revert** (commit / merge -m 1), Reset Branch, **New Branch/Tag** (+Inspect detached), **Recover (Reflog)**, **Discard Local** (checkout/reset --hard HEAD/clean), **Rebase** (remove commit). **About Restore** = a scrollable window with an explanation per category + team work

> Control-by-control details: [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]] · [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]] · [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

![[ScreenshotGitFlow.png]]

## 🔄 GitFlow commands → pure git

The plugin runs **native git only** — it does not depend on the `git-flow` binary being installed.
Each button in the GitFlow window triggers the sequence below:

### Start
| Type | git command |
|------|-------------|
| `feature`, `release` | `git checkout -b <prefix><name> develop` |
| `bugfix` | `git checkout -b <prefix><name> <release/*>` — **release base required** |
| `hotfix`, `support` | `git checkout -b <prefix><name> main` |
| any (based on checked) | `git checkout -b <prefix><name> <chosen base>` |

> **Bugfix rule:** a bugfix **can only exist linked to a release**. `DoStart` blocks the Start if there is no release or if the chosen base is not a `release/*`; the release base writes a *based-on override* → the bugfix ends up **nested under the release** in the tree (also via `BuildGitFlowParentMap`, which uses the real ancestry to find the parent release). A bugfix outside this rule becomes a **violation** (`violLocalBugfix`) that triggers the GitFlow auto-organization.
> **based on:** allows a feature-as-child-of-feature; in that case the plugin also runs `git commit --allow-empty -m "chore: start <prefix><name>"` so the hierarchy is visible (see Limitations).
> **Default release/hotfix name:** when choosing the `release` or `hotfix` type, the name is pre-filled with `yyyyMMddHHmm` (only if the field is empty).

### Publish
```
git push --set-upstream <remote> <prefix><name>
```

### Track
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout -b <prefix><name> --track <remote>/<prefix><name>
```

### Update
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout <prefix><name>
git merge <remote>/<parent>                            # (or git merge <parent> if No fetch)
```
> Parent = `develop` for feature/release; `main` for hotfix/support; **the release (parent)** for bugfix (from the local ref; develop fallback)

### Finish — feature / bugfix
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout <target>                                  # feature: develop or based-on parent
                                                       # bugfix: release (based-on parent), or develop if the release does not exist
git merge --no-ff <prefix><name>
git branch -d <prefix><name>                           # (if Keep unchecked)
git push <remote> --delete <prefix><name>              # (only if the remote branch exists)
```

### Finish — hotfix
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout main
git merge --no-ff hotfix/<name>
git tag -a <name> -m "<name>"
git checkout develop
git merge --no-ff hotfix/<name>
git branch -d hotfix/<name>                            # (if Keep unchecked)
git push <remote> --delete hotfix/<name>              # (only if the remote branch exists)
```

### Finish — release (full automatic flow)
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout main
git merge --no-ff release/<name>
git tag -a <name> -m "<name>"
git checkout develop
git merge --no-ff release/<name>
git branch -d release/<name>                           # (if Keep unchecked)
git push <remote> --delete release/<name>            # (only if the remote branch exists)
git push <remote> main
git push <remote> develop
git push <remote> refs/tags/<name>
git checkout develop
```
> On completion, the **TAGS** section is expanded and focus goes to the created tag. Remote = `origin` (or the first one configured).

### Finish — support
```
git fetch <remote>                                     # (if No fetch unchecked)
git checkout main
git merge --no-ff support/<name>
git branch -d support/<name>                           # (if Keep unchecked)
git push <remote> --delete support/<name>            # (only if the remote branch exists)
```

> **Merge errors** (conflict): the plugin stops and shows the result. The repository stays in a "merging" state — resolve with `git merge --abort` or resolve the conflicts and `git commit`.

## 🔌 Dependencies

### Required for use
| Program | Minimum version | Function |
|----------|---------------|--------|
| **Git for Windows** | any ([download](https://git-scm.com/download/win)) | Runs all git commands. On the *"Adjusting your PATH"* screen choose **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | Host app that loads the plugin; provides the native Commit/Push/Pull dialogs. The `.msi` installer installs the .NET 9 Desktop Runtime |
| **ZimerfeldTree plugin** | — | The DLL in `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) is **incompatible** — the plugin requires `net9.0-windows`.

### Conditional — build / development
| Program | Function |
|----------|--------|
| **.NET SDK 9** ([download](https://dotnet.microsoft.com/download/dotnet/9.0)) | Build `net9.0-windows` |
| **NuGet CLI** ([download](https://www.nuget.org/downloads)) | Generate the `.nupkg` (used by `build.ps1`) |

See also [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]].

## 🛠️ Build / installation
```powershell
# Build + pack the nupkg (manages the major.minor.BUILD version). As Admin it also copies the DLL.
.\build.ps1
# Helper scripts in tools\
tools\install.ps1      # installs the plugin
tools\uninstall.ps1    # removes it
tools\update-dll.ps1   # updates only the DLL
```
`build.ps1`: (1) reads and increments `<version>` in the nuspec; (2) syncs `<Version>` in the csproj; (3) updates `README.md`; (4) builds in Release; (5) if Admin, copies the DLL to `C:\Program Files\GitExtensions\Plugins\`; (6) runs `nuget pack`.

Build completed successfully (version incremented, DLL copied and `.nupkg` generated):

![[ScreenshotBuild.png]]

When **no change** is detected in the sources, the script keeps the version and skips build/pack:

![[ScreenshotNoBuild.png]]

**Manual installation:** copy `GitExtensions.Plugins.ZimerfeldTree.dll` to `C:\Program Files\GitExtensions\Plugins\` and restart GitExtensions.

`tools\install.ps1` (as Admin):

![[ScreenshotInstall.png]]

**Uninstall:** delete that DLL (does not affect GitExtensions). Via `tools\uninstall.ps1`:

![[ScreenshotUninstall.png]]

**Update only the DLL:** `tools\update-dll.ps1` (as Admin) — copies the freshly built DLL to `Plugins\` without reinstalling:

![[ScreenshotUpdate.png]]

## ⛔ Branch hierarchy limitations
- **Grouping is by name (`/`), not by commit kinship** for the folder axis — `master` and `develop` appear as siblings; to nest by name use `/`.
- **A real branch cannot be a parent node of another branch** — if `feature/login` exists, creating `feature/login/oauth` fails (`cannot lock ref … exists`), since the ref would be both a file **and** a directory. Solution: sibling names (`feature/login-oauth`) or a grouping node with no real branch (`feature/login/base` + `feature/login/oauth`).
- **Flexible GitFlow — feature under feature** — Classic GitFlow does not provide for a feature branch as a child of another feature (all `feature/*` derive from `develop` as siblings). **ZimerfeldTree GitFlow** breaks that rigidity: a `feature/*` can derive from `develop` **or from another `feature/*` above it** (via **based on:** at Start). Consequence: finishing such a feature must **cascade** its changes up to the parent `feature/*` node, successively re-applying *finish feature* until it reaches `develop`.
- **Two branches on the exact same commit do not form parent-child** — the ancestry BFS never finds one as the parent of the other; both become roots. Automatic solution: an empty commit on Start with **based on**. Detail in [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]].

## 🐛 Known pitfalls
> [!warning] MSB3277 (WindowsBase)
> The GitExtensions DLLs pull in WindowsBase 8.0 while the net9 ref pack provides 4.0. The runtime resolves the correct one at load time → the csproj **downgrades MSB3277 to a message** (`MSBuildWarningsAsMessages`). It is benign.

> [!warning] Git Flow showing "Init Gitflow"
> GitExtensions writes the config in its own internal format, but the git flow CLI expects other keys. Solution in [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]].

## 🔢 Versioning
- Current version: **1.0.361** (README + csproj + nuspec + vault in sync)
- Scheme: `major.minor.BUILD`, managed by `build.ps1`
- ⚠️ Keep the csproj and nuspec in sync

## 🎨 Icons (NodeIcons.cs)
- 16×16 icons generated at runtime via GDI+, with several **embedded PNGs** and a drawn fallback. Indices in `NodeIcons`: 0–4 generic, 5–7 sections, 8–15 GitFlow/leaf.
- The **remote group (`origin`)** uses `Resources\origin.png` (rocket) via `NodeIcons.Remote` — mapped in `GetFolderIconIndex`.
- **Develop (index 9)** uses `Resources\develop.png`, fallback `Wrench()`.

## 🔗 Integrated plugins (same author)
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — automatically generates the commit message (Conventional Commits) summarizing the staged files. GitHub: [zimerfeld/GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg).
- **[GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)** — manages Git LFS (Large File Storage): track, push, and pull large binary files directly from the GitExtensions interface.

## 🔗 Related
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF plugin for GitExtensions]]
- [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]]
- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]]
- [[🔑 Fatos-Chave (EN)|Key Facts]]
