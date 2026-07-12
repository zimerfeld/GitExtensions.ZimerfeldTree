---
tipo: backlog
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-07
versao: 1.0.361
tags: [backlog, retomada]
---

# 📌 Backlog — GitExtensions.ZimerfeldTree

> [!tip] 🧭 Resumption point
> **Start here** when resuming the project in another session. Current state + pending items, always updated at the end of each work stage.

## ✅ Current state (2026-07-04)

- Version **`1.0.358`** — functional plugin, with the **three windows in production**: Branch Hierarchy (non-modal tree), GitFlow and Restore (10 tabs).
- Build/versioning/deploy automated via `build.ps1` (docs stamped on every build). See [[💻 Ambiente Local (Dev) (EN)|Local Environment (Dev)]].
- Distribution: `.nupkg` on the nuget.org feed (installable through the GitExtensions Plugin Manager) + GitHub repository under the `zimerfeld` owner. See [[🚀 Deploy em Produção (Prod) (EN)|Production Deploy (Prod)]].
- Bilingual READMEs (`README.md` / `README.pt-BR.md` / `README.en-US.md`) and vault in sync with the version.
- **Vault restructured to the "Neuron Vault v2" standard** on 2026-07-04 (folders by priority: impact → reuse → usage → operations; session history removed by user decision).

## 📋 Next steps

- [ ] **Translate the vault notes into English** — create the `(EN)` pairs of the content notes (next stage of the v2 restructuring; see the list of unpaired notes in the Home).
- [ ] Keep the **adoption count** (clones/downloads) of the `zimerfeld/GitExtensions.ZimerfeldTree` repo up to date (global portfolio rule).
- [ ] Re-mirror [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]] whenever the repo README changes significantly (`build.ps1` only stamps version/date).

> [!note] 📥 No other pending items recorded
> The [[📥 Inbox (EN)|Inbox]] is empty and the vault records no open bugs or planned features at the moment. When new demand arises, record it here.

## ✅ Recently done
- [x] **Landing-page fix — PT title/subtitle line break** — 2026-07-07: the landing page (`index.html`, served at **tree.zimerfeld.com** via GitHub Pages) shares an i18n template with the rule `html[data-lang="pt"] .lang-pt{display:inline}`, which forced **every** Portuguese element to `inline` — including `h2`/`h3` — making the title/subtitle collapse into the following text when the site opens in PT (EN was fine, since `h2`/`h3` are `block` by default). **1-line CSS fix:** `html[data-lang="pt"] h2.lang-pt,html[data-lang="pt"] h3.lang-pt{display:block}` — restores the break only on PT titles/subtitles, with no effect on EN. Shipped via GitFlow as a **hotfix** (`hotfix/pt-heading-break` → `main`, with a back-merge into `develop`, which was behind `main`) + tag **`202607071915pt-heading-break`**; `CNAME` preserved; deploy verified live.

## 🔗 Links
- [[🏠 Home (EN)|Home]] — vault entry point
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]] — project mother note
