---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [sistema, dependencias, instalacao, gitextensions, git, gitflow]
versao: 1.0.358
---

# Dependencies

> [!abstract] Summary
> Programs required to **use** and to **build** the plugin. The step-by-step detail (downloads, verification, install procedure) lives in the tools note [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]] — this note is the system-level summary equivalent to the sibling vault's.

## Required for use

| Program | Version | Role |
|---|---|---|
| **Git for Windows** | any ([download](https://git-scm.com/download/win)) | Runs **all** the plugin's git commands. On the *"Adjusting your PATH"* screen choose **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | **Host** app that loads the plugin via MEF; provides the native Commit/Push/Pull dialogs. The `.msi` installs the .NET 9 Desktop Runtime |
| **ZimerfeldTree Plugin** | — | The DLL `GitExtensions.Plugins.ZimerfeldTree.dll` in `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) is **incompatible** — the plugin requires `net9.0-windows`.

## Conditional — build / development

| Program | Role |
|---|---|
| **.NET SDK 9** ([download](https://dotnet.microsoft.com/download/dotnet/9.0)) | Compile `net9.0-windows` |
| **NuGet CLI** ([download](https://www.nuget.org/downloads)) | Generate `.nupkg` (used by `build.ps1`; resolved via PATH → `tools\nuget.exe` → automatic download) |

## External references (not copied)

From `C:\Program Files\GitExtensions\`, with `Private=false` (the host runtime resolves them at load time):
- `GitExtensions.Extensibility.dll`
- `GitUIPluginInterfaces.dll`
- `System.ComponentModel.Composition.dll`

## GitFlow with no external dependency

> [!info]
> All the GitFlow commands (Start, Publish, Track, Update, Finish) use **pure git**. The `git-flow` binary does **not** need to be installed. See [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]].

## Related

- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]] — detailed version, step by step
- [[🏗️ Arquitetura (EN)|Architecture]]
- [[👁️ Visão Geral (EN)|Overview]]
- [[🔧 build.ps1 (EN)|build.ps1]]
