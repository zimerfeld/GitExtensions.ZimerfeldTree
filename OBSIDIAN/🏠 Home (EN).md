---
tipo: moc
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
versao: 1.0.361
tags: [moc, home, zimerfeld, tree, gitextensions, gitflow]
---

# 🏠 GitExtensions.ZimerfeldTree — Neuron Vault

> 🇧🇷 Leia esta página em português → [[🏠 Home]]

> [!abstract] 🧠 What this vault is
> Claude's persistent memory for the **GitExtensions.ZimerfeldTree** project — a GitExtensions plugin that displays branches as a **hierarchical tree** and provides **visual GitFlow** using pure git. The vault is read at the start of every session and updated whenever something relevant changes.

![[ScreenshotBranchHierarchy.png]]

## ⚡ Executive summary

- **What it is:** an extension (MEF plugin) for **GitExtensions** that replaces the flat branch list with a **hierarchical tree** (LOCAL / REMOTES / TAGS) and exposes the **GitFlow** workflow in clicks, without depending on the `git-flow` binary. Three dedicated windows: **Branch Hierarchy** (non-modal tree), **GitFlow** (start/publish/track/update/finish) and **Restore** (10 "go back in time" tabs). Custom "Tree of Life" icon (GDI+ / `Resources/ico.png`).
- **Problem it solves:** browsing branches in a flat list and driving GitFlow from the command line is slow and error-prone. The plugin provides **visual parent-child context** between branches and turns start/publish/track/update/finish into buttons — logging every git command executed.
- **Differentiators:** **flexible GitFlow** (feature child of feature via *based on:*, with *finish* cascading up to `develop`); **non-modal**, asynchronous window (opens instantly with a progress overlay); **live Commit counter** (FileSystemWatcher); **protected push** against divergence (offers pull --rebase + automatic push); **Developer Mode** protecting `main`/`master`/`develop`; **Restore** window with 10 recovery levels; **i18n** (English / Portuguese, per window); sponsorship banner (GitHub Sponsors + Ko-fi).
- **Stack:** C# / WinForms `Library`, targeting **net9.0-windows**, assembly `GitExtensions.Plugins.ZimerfeldTree.dll`, packaged as a **nupkg**; build and versioning automated via `build.ps1`.
- **Current state:** version **`1.0.358`** — functional, with all three windows in production.
- **Target audience:** developers and teams using GitExtensions on Windows who adopt (or want to adopt) GitFlow with visual clarity of the branch hierarchy.
- **Business/portfolio angle:** **open source** product under the `zimerfeld` owner, reinforcing technical authority and serving as a showcase for adoption (NuGet clones/downloads) and sponsorship. Integrates with its sibling **GitExtensions.ZimerfeldCommitMsg**.

## 🧭 Navigation by priority

### 1️⃣ 🔑 Impact — Key Files
> Files that, when touched, have a big impact on the system.
- [[🌳 ZimerfeldTreePlugin (EN)|🌳 ZimerfeldTreePlugin]] — MEF entry point (`IGitPlugin`)
- [[🪟 BranchHierarchyForm (EN)|🪟 BranchHierarchyForm]] — the main window (tree, buttons, live counter)
- [[⚙️ BranchHierarchyService (EN)|⚙️ BranchHierarchyService]] — git executor + hierarchy assembly
- [[🔀 GitFlowForm (EN)|🔀 GitFlowForm]] — the GitFlow window (start/finish, based on)
- [[⏪ RestoreForm (EN)|⏪ RestoreForm]] — the Restore window (10 tabs)
- [[🔧 build.ps1 (EN)|🔧 build.ps1]] — build + versioning + deploy
- [[🌿 BranchNode (EN)|🌿 BranchNode]] — `BranchInfo` + `BranchType` models
- [[🎨 NodeIcons (EN)|🎨 NodeIcons]] — 16×16 tree icons (GDI+ + embedded PNGs)
- [[🖼️ PluginIcon (EN)|🖼️ PluginIcon]] — plugin/window icon (`Resources/ico.png`)

### 2️⃣ 🧩 Reuse — Systems
> Subsystems reused across the project.
- [[👁️ Visão Geral (EN)|Overview]] — what the plugin does, stack, the three windows, current version
- [[🏗️ Arquitetura (EN)|Architecture]] — the classes (Plugin → Forms → Service), threading, i18n, decoupling
- [[🏷️ Versionamento (EN)|Versioning]] — `build.ps1` / nuspec / csproj cycle / doc stamping
- [[📦 Dependências (EN)|Dependencies]] — system summary (host + `git` + build)
- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]] — detailed step-by-step version, with downloads

### 3️⃣ 🔀 Usage — Flows
> Step-by-step usage flows.
- [[🌲 Abrir e navegar a árvore (EN)|Open and navigate the tree]] — asynchronous opening, overlay, hierarchy navigation
- [[🔀 GitFlow (Start a Finish) (EN)|GitFlow (Start to Finish)]] — the GitFlow cycle in pure git
- [[⏪ Restore (voltar no tempo) (EN)|Restore (going back in time)]] — the 10 recovery tabs

## 🚀 Operations
- [[💻 Ambiente Local (Dev) (EN)|Local Environment (Dev)]] — `.\build.ps1` (Admin) · quick deploy: `.\tools\update-dll.ps1`
- [[🚀 Deploy em Produção (Prod) (EN)|Production Deploy (Prod)]] — `.\build.ps1` → publish `.nupkg` (nuget.org + GitHub release)

## ⚖️ Decisions
- [[🌳 Árvore hierárquica vs lista plana (EN)|Hierarchical tree vs flat list]] — why a tree instead of the default flat list
- [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]] — no dependency on the `git-flow` binary
- [[🌿 GitFlow flexível — feature filha de feature (EN)|Flexible GitFlow — feature child of feature]] — hierarchy beyond classic GitFlow
- [[🪟 Janela não-modal singleton (EN)|Non-modal singleton window]] — persistent window + independent working dir
- [[🛡️ Modo Developer protege main-develop (EN)|Developer Mode protects main-develop]] — protection against accidental deletion
- [[⏪ Restore — central de voltar no tempo (EN)|Restore — go-back-in-time hub]] — 10 tabs ordered by risk

## 📚 Knowledge
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree UI — buttons and flows]] — the non-modal tree (Pull/Push/Commit/Delete/GitFlow/Restore, filter, overlay, live counter)
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow UI — buttons and flows]] — start/publish/track/update/finish and the *based on:* hierarchy
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore UI — buttons and flows]] — the 10 "go back in time" tabs, from safest to most destructive
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF plugin for GitExtensions]] — the MEF plugin model (`IGitPlugin`)
- [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]] — `gitflow.*` keys expected by the CLI vs. those written by GitExtensions
- [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]] — why two branches on the same commit don't form parent-child, and the fix (empty commit on Start)
- [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]] — README mirror

## 💼 Business
- [[🌳 GitExtensions.ZimerfeldTree (EN)|🌳 GitExtensions.ZimerfeldTree]] — project mother note (mirrors the README: goal, structure, features, limitations, funding)

## 🧭 Meta
- [[🧭 Como usar este cofre (EN)|How to use this vault]] — Claude's read/write protocol
- [[🔑 Fatos-Chave (EN)|Key Facts]] — always-useful truths (paths, names, conventions)
- [[📥 Inbox (EN)|📥 Inbox]] — quick capture
- [[👤 Renato (EN)|👤 Renato]] — context and preferences
- [[🧰 RTK (EN)|🧰 RTK]] — token-saving CLI proxy

## 📌 Resuming
- [[📌 Backlog (EN)|Backlog]] — **start here** when resuming the project in another session
