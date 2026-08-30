---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-08-29
criado: 2026-06-18
tags: [conhecimento, readme, instalacao, build, uso, gitflow, hierarquia, i18n]
fonte: README.md
versao: 1.0.362
---

# README — Install, Usage and Build

> Faithful mirror of the repository root `README.md` (and the `README.en-US.md` / `README.pt-BR.md` variants).
> Project note: [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]. Detailed flows in [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]], [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]] and [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].
> `build.ps1` stamps version + date in the READMEs **and in this note** (frontmatter `versao:`/`atualizado:`) on every build — re-mirror the body when the README changes significantly.

Current version: **1.0.362**

Plugin for **[GitExtensions](https://gitextensions.github.io/)** that displays the repository branches **hierarchically as a tree** (showing child branches) instead of the default flat list, and makes using the **GitFlow** methodology very easy, intuitive and pleasant to apply on projects of any size.

## ✨ High-level features
- **Hierarchical branch tree** — **LOCAL**, **REMOTES** and **TAGS** sections combining real commit ancestry with grouping by `/` path; the current branch in bold, live counters and real-time filtering.
- **GitFlow in one click** — start/publish/track/update/finish for feature, release, hotfix, bugfix and support, with a flexible hierarchy that even allows a *feature as a child of a feature* (finish cascades all the way to `develop`).
- **Manual Pull / Push / Commit** — buttons with arrow icons (↓ blue / ↑ green) and ahead/behind counters, background check of the remote on open, and a warning that **blocks the push when the branch is behind**.
- **Restore — "time travel" hub** — a dedicated window gathering all the safe ways to recover or undo history: restore file/tree/tag, cherry-pick, **revert**, create branch/tag from any commit, **recovery via reflog**, discard local changes and an advanced rebase to remove a commit — each with an embedded explanation and team-work guidance.
- **Localized (English / Portuguese)** — each window picks its language independently and remembers it.
- **Asynchronous loading** — the window opens immediately with a progress overlay (0→100%) while the data is read in the background; the constructor does no git.
- **Multi-selection via checkbox** + **Developer Mode** that protects `main`/`master`/`develop` from deletion when it is off.

## 🔀 GitFlow → pure git

The plugin runs **native git only** — it does not depend on the `git-flow` binary being installed. Each button in the GitFlow window triggers the equivalent sequence of git commands (start, publish, track, update and finish for each branch type). Details in [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]].

### Flexible hierarchy — feature as a child of a feature
Classic GitFlow does not allow a feature as a child of a feature (all `feature/*` derive from `develop` and are siblings). **ZimerfeldTree GitFlow** lets a `feature/*` derive from `develop` **or from another `feature/*`** above it (via *based on:* in Start); in that case *finish feature* **cascades** the changes to the parent node successively, until it reaches `develop`.

## ⛔ Hierarchy limitations
- **Grouping by name (`/`), not by commit kinship** for the folder axis — `master` and `develop` appear as siblings.
- **A real branch cannot be a parent node of another branch** — if `feature/login` exists, creating `feature/login/oauth` fails (the ref would be both a file **and** a directory). Solution: sibling names or a grouping node with no real branch.
- **Two branches on the exact same commit do not form parent-child** — automatic solution: an empty commit on Start with *based on*. See [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]].

## 🔌 Dependencies

### Required for use
| Program | Minimum version | Function |
|----------|---------------|--------|
| **Git for Windows** | any | Runs all git commands (choose "Git from command line and also from 3rd-party software" in the PATH). |
| **GitExtensions** | 4.x (.NET 9) | Host app that loads the plugin; provides the native Commit/Push/Pull dialogs. |
| **ZimerfeldTree plugin** | — | The DLL in `C:\Program Files\GitExtensions\Plugins\`. |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) is **incompatible** — the plugin requires `net9.0-windows`.

### Conditional — build / development
| Program | Function |
|----------|--------|
| **.NET SDK 9** | Build `net9.0-windows` |
| **NuGet CLI** | Generate the `.nupkg` (used by `build.ps1`) |

See also [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]].

## 📦 Installation
**Option A — GitExtensions Plugin Manager (recommended):** in **Plugins → Plugin Manager**, look up `GitExtensions.ZimerfeldTree` in the nuget.org feed and install it; restart and open **Plugins → ZimerfeldTree**. Requires neither PowerShell nor Administrator. It depends on packaging the DLL in the root `lib\` (the "any" group) + `<dependency id="GitExtensions.Extensibility" version="[0.4.0, 0.5.0)">` — the range must **contain** the version the Plugin Manager announces (v3.x → 0.4.0). See [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]].

**Option B — PowerShell (as Administrator):**
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

**Option C — Manual:** copy `GitExtensions.Plugins.ZimerfeldTree.dll` to `C:\Program Files\GitExtensions\Plugins\` and restart GitExtensions.

## 🗑️ Uninstall
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\uninstall.ps1
```
Removing the DLL does not affect any other part of GitExtensions.

## 🛠️ Build and versioning
On every run of `build.ps1`, the script:
1. Reads the current version from the `.nuspec` and detects changes (sources + `*.md`) vs. the last `.nupkg`.
2. Computes the new version (increments `build` by +1 → `major.minor.build`).
3. Writes the new version and date **in the docs first** (READMEs + Obsidian vault).
4. Bumps the `.nuspec` and the `.csproj`.
5. Builds in Release.
6. Copies the DLL to `C:\Program Files\GitExtensions\Plugins\` *(requires Admin)* and to `tools\net9.0-windows\`.
7. Generates `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` and removes `.nupkg` files of previous versions.

```powershell
cd C:\GitExtensions\ZimerfeldTree
.\build.ps1
```

**Quick deploy (without bumping the version):**
```powershell
.\tools\update-dll.ps1
```

See [[🏷️ Versionamento (EN)|Versioning]].

## 🤝 Related plugins
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — automatically generates the commit message (Conventional Commits) summarizing the staged files. By **zimerfeld**. GitHub: [zimerfeld/GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg).
- **[GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)** — manages Git LFS (Large File Storage): track, push, and pull large binary files directly from the GitExtensions interface. By **zimerfeld**.

## 💜 Support the project
Help keep this project always up to date: **[GitHub Sponsors → zimerfeld](https://github.com/sponsors/zimerfeld)** · **[Ko-fi → Buy me a coffee ☕](https://ko-fi.com/C0D621FCGD)**.

## 📄 License
[CC BY-NC-ND 4.0](LICENSE.txt)

## 🔗 Related
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[👁️ Visão Geral (EN)|Overview]]
- [[🏷️ Versionamento (EN)|Versioning]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]]
- [[🔑 Fatos-Chave (EN)|Key Facts]]
