---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [operacao, dev, build, install, powershell]
---

# 💻 Local Environment (Dev)

> [!abstract] 🎯 Goal
> Build the **ZimerfeldTree** plugin and install it into the local GitExtensions for development and testing. Content derived from [[🔧 build.ps1 (EN)|🔧 build.ps1]], [[🏷️ Versionamento (EN)|Versioning]] and [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]].

## ⚡ TL;DR — the single command

```powershell
# at the repo root, as Administrator
.\build.ps1
```

Builds, bumps the version, installs the DLL into the local GitExtensions and produces the `.nupkg` — all in one step. To iterate quickly without touching the version:

```powershell
.\tools\update-dll.ps1      # only copies the compiled DLL (requires Admin)
```

## ⚙️ What the script does (in order)

```
build.ps1
  ├─ 1.  Reads the current version from the .nuspec
  ├─ 1b. Detects changes (sources + texts) vs. the last .nupkg → exits if nothing changed
  ├─ 1c. Closes GitExtensions and plugins before compiling
  ├─ 2.  Bump in the .nuspec  ← <version>
  ├─ 3.  Bump in the .csproj  ← <Version>
  ├─ 4.  Updates the NuGet link and "Current version" in README.md
  ├─ 4b. Stamps the header (Version/Updated) in the READMEs (md / pt-BR / en-US)
  ├─ 4c. Stamps the Obsidian vault (notes that mirror the version)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copies the DLL → C:\Program Files\GitExtensions\Plugins\  (requires Admin)
  │       and refreshes tools\net9.0-windows\  (for the nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg at the repo root
  └─ —   Removes .nupkg files of previous versions
```

## 🚩 Parameters / flags

- `-Force` — packs even when no changes are detected (the incremental detection compares package inputs against the last `.nupkg`, not the DLL — on purpose, to avoid rebuild loops).

## 🧰 Helper scripts (`tools\`)

| Script | Role |
|---|---|
| `install.ps1` | Installs the DLL into GitExtensions (Admin) |
| `uninstall.ps1` | Removes the DLL (Admin) — does not affect anything else in GitExtensions |
| `update-dll.ps1` | Quick deploy of the DLL only, no version bump (Admin) |

## 📐 Rules it follows

- **Versioning** `major.minor.BUILD` — only `BUILD` is incremented automatically; source of truth: `.nuspec` / `.csproj`. See [[🏷️ Versionamento (EN)|Versioning]].
- **Docs stamped before pack** — READMEs and vault are updated before step 7, keeping the `.nupkg` as the most recent artifact (correct timestamp-based change detection).
- **GitFlow** — development happens on a feature branch (Renato's global rule); the build does not interact with git.

## 🔧 Prerequisites

- **.NET SDK 9** (builds `net9.0-windows`) and **NuGet CLI** (resolved via PATH → `tools\nuget.exe` → automatic download).
- **GitExtensions 4.x** installed at `C:\Program Files\GitExtensions\`.
- PowerShell **as Administrator** for the DLL deploy.
- Installation details for each dependency: [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]].

## 🩹 Troubleshooting

- **Not running as Admin** → step 6 (DLL deploy) is skipped with a warning; run the terminal as Administrator.
- **"No changes" but you want to pack anyway** → use `.\build.ps1 -Force`.
- **NU5101 warning during pack** → intentional and filtered: the DLL sits directly in `lib\` (the "any" group) because the GitExtensions Plugin Manager only extracts that group; a `net9.0-windows` subfolder would break the installation.
- **GitExtensions open** → the script itself closes GitExtensions before compiling (step 1c).
- **GitExtensions 3.x** → incompatible (.NET Framework 4.8); the plugin requires `net9.0-windows`.

## 🔗 Links

- [[🔧 build.ps1 (EN)|🔧 build.ps1]] — the key-file note
- [[🏷️ Versionamento (EN)|Versioning]] — version scheme and full cycle
- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]] — step-by-step dependency installation
- [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]] — README mirror
- [[🚀 Deploy em Produção (Prod) (EN)|Production Deploy (Prod)]] — release publishing
