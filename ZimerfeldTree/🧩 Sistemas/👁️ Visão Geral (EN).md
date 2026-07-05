---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [sistema, overview, plugin, gitextensions, winforms, gitflow]
versao: 1.0.361
---

# Overview

## What it is

A plugin for **[GitExtensions](https://gitextensions.github.io/)** (Windows) that displays the repository's branches **hierarchically as a tree** (showing child branches) instead of the default flat list, and makes using the **GitFlow** methodology visual, easy and intuitive. It has its own "Tree of Life" icon drawn/embedded (GDI+ / `Resources/ico.png`). Project note: [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]].

## Stack

| Item | Value |
|---|---|
| Language | C# (.NET 9), `Nullable` + `ImplicitUsings`, `LangVersion=latest` |
| Target | `net9.0-windows` |
| UI Framework | Windows Forms (`UseWindowsForms`) |
| Output type | `Library` (DLL loaded by GitExtensions, not an exe) |
| Output assembly | `GitExtensions.Plugins.ZimerfeldTree.dll` |
| Root namespace | `GitExtensions.ZimerfeldTree` |
| Plugin model | MEF (`System.ComponentModel.Composition`) — see [[🧩 Plugin MEF para GitExtensions (EN)|MEF Plugin for GitExtensions]] |
| Current version | **1.0.358** |
| Languages | English / Portuguese (per window, persisted individually) |
| Author | Zimerfeld |

> **External references** (from `C:\Program Files\GitExtensions\`, `Private=false`, not copied): `GitExtensions.Extensibility.dll`, `GitUIPluginInterfaces.dll`, `System.ComponentModel.Composition.dll`.

## The three windows

### 1. Branch Hierarchy (`BranchHierarchyForm`)
**Non-modal** window, singleton per session, opens centered and resizable, independent of GitExtensions. A tree in 3 fixed sections — **LOCAL**, **REMOTES**, **TAGS** — with `(N)` counters, the current branch in bold, real-time filtering, and **Pull / Push / Commit / Delete / GitFlow / Restore** buttons above the tree. Asynchronous loading with a progress overlay on first open. Control-by-control details: [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]].

### 2. GitFlow (`GitFlowForm`)
Modal window that drives the `git flow` commands using **native git only** (does not depend on the `git-flow` binary being installed): start/publish/track/update/finish for feature, bugfix, release, hotfix and support. Allows a **flexible hierarchy** (feature child of a feature, via *based on:*). Details: [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]] and [[⚙️ git flow - chaves de config (CLI) (EN)|git flow - config keys (CLI)]].

### 3. Restore (`RestoreForm`)
A "go back in time" hub with 10 tabs, from the safest to the most destructive: Emergency Plan, Restore File/Tree/Tag, Cherry-Pick, **Revert**, Reset Branch, **New Branch/Tag**, **Recover (Reflog)**, **Discard Local** and **Rebase**. Each category with an embedded explanation and teamwork guidance. Details: [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]].

## Flexible GitFlow hierarchy — feature child of a feature

Classic GitFlow does not foresee a feature being the child of a feature (all `feature/*` derive from `develop` and are siblings). **ZimerfeldTree GitFlow** allows a flexible hierarchy where a `feature/*` may derive from `develop` **or from another `feature/*`** above it (via *based on:* in Start). Consequence: *finish feature* must **cascade** the changes up to the parent node successively, reapplying *finish feature* until it reaches `develop`.

## Localization (English / Portuguese)

Each window chooses its language **independently** and remembers it. The main window uses the global `I18n.SetLanguage` (persisted in `ZimerfeldTree.language.json`); GitFlow and Restore have their own selector persisted in their respective settings files.

## Related

- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[🏷️ Versionamento (EN)|Versioning]]
- [[📘 README — Instalação, Uso e Build (EN)|README — Installation, Usage and Build]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
- [[🔑 Fatos-Chave (EN)|Key Facts]]
