---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-02
tags: [ferramenta, dependencias, instalacao, zimerfeldtree, gitextensions, git, gitflow]
---

# 🧩 ZimerfeldTree Dependencies

> [!abstract] Summary
> Complete list of programs and plugins required to run all the features of the **ZimerfeldTree** plugin. Includes the download URL and install procedure for each item.

---

## 1. Git for Windows

| Field    | Value |
|----------|-------|
| Role     | Runs **all** the git commands used by the plugin (branch, checkout, pull, push, commit, tag, describe, flow…) |
| Required | ✅ Yes — without git the plugin does not work |
| Download | https://git-scm.com/download/win |

### Installation
1. Download the `.exe` installer (64-bit) and run it.
2. On the **"Adjusting your PATH environment"** screen select **"Git from command line and also from 3rd-party software"** (lets GitExtensions call `git` without the full path).
3. Other options: default.
4. Verify: `git --version` in the terminal.

---

## 2. GitExtensions

| Field    | Value |
|----------|-------|
| Role     | **Host** application that loads the plugin via MEF; provides the native Commit (`StartCommitDialog`), Push (`StartPushDialog`) and Pull dialogs |
| Required | ✅ Yes — the plugin is a DLL loaded by GitExtensions |
| Minimum version | 4.x (.NET 9 runtime) |
| Download | https://github.com/gitextensions/gitextensions/releases |
| Official site | https://gitextensions.github.io/ |

### Installation
1. Download the `.msi` or `.exe` installer of the latest 4.x release.
2. Run the installer — it checks for and installs the **.NET 9 Desktop Runtime** automatically if not present.
3. After installing, the executable is at `C:\Program Files\GitExtensions\GitExtensions.exe`.
4. Verify: open GitExtensions and go to **Help → About**.

> [!warning] Version
> 3.x versions use .NET Framework 4.8 and are incompatible with the plugin (compiled for `net9.0-windows`).

---

## 3. ZimerfeldTree Plugin

| Field    | Value |
|----------|-------|
| Role     | The plugin itself — a DLL loaded by GitExtensions in the `Plugins\` folder |
| Required | ✅ Yes |
| Repository | https://github.com/zimerfeld/ZimerfeldTree |
| Target DLL | `C:\Program Files\GitExtensions\Plugins\GitExtensions.Plugins.ZimerfeldTree.dll` |

### Installation (option 1 — automatic script as Admin)
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

### Installation (option 2 — manual)
1. Copy `GitExtensions.Plugins.ZimerfeldTree.dll` to:
   ```
   C:\Program Files\GitExtensions\Plugins\
   ```
2. Restart GitExtensions.
3. Verify: menu **Plugins → ZimerfeldTree**.

### Build from source
```powershell
# Requires .NET SDK 9 and NuGet CLI (see items 5 and 6 below)
# Run as Administrator for automatic deploy
pwsh C:\GitExtensions\ZimerfeldTree\build.ps1
```

---

## 4. .NET SDK 9 *(build/development only)*

| Field    | Value |
|----------|-------|
| Role     | Compile the project `GitExtensions.ZimerfeldTree.csproj` (`net9.0-windows`) |
| Required | ⚠️ Conditional — only to compile the source |
| Download | https://dotnet.microsoft.com/download/dotnet/9.0 |

### Installation
1. Download the **.NET 9 SDK** installer (not to be confused with the Runtime).
2. Run the installer.
3. Verify: `dotnet --version` (should return `9.x.x`).

---

## 5. NuGet CLI *(build/development only)*

| Field    | Value |
|----------|-------|
| Role     | Generate the `.nupkg` package (used by `build.ps1`) |
| Required | ⚠️ Conditional — only to generate the NuGet package |
| Download | https://www.nuget.org/downloads |

### Installation
1. Download `nuget.exe` (latest stable version).
2. Place it in a folder on the `PATH` (e.g., `C:\Program Files\NuGet\`).
3. Verify: `nuget` in the terminal.

---

## Quick summary

> [!info] GitFlow with no external dependency
> All the GitFlow commands (Start, Publish, Track, Update, Finish) use **pure git**. The `git-flow` binary does not need to be installed.

| # | Program / Plugin      | Required for use | For GitFlow | For build |
|---|-----------------------|:----------------:|:-----------:|:---------:|
| 1 | Git for Windows       | ✅               | ✅          | ✅        |
| 2 | GitExtensions 4.x     | ✅               | ✅          | ✅        |
| 3 | ZimerfeldTree Plugin  | ✅               | ✅          | —         |
| 4 | .NET SDK 9            | —                | —           | ✅        |
| 5 | NuGet CLI             | —                | —           | ✅        |

---

## 🔗 Related
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF Plugin for GitExtensions]]
