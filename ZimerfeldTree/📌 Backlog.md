---
tipo: backlog
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-07
versao: 1.0.361
tags: [backlog, retomada]
---

# 📌 Backlog — GitExtensions.ZimerfeldTree

> 🇺🇸 Read this page in English → [[📌 Backlog (EN)]]

> [!tip] 🧭 Ponto de retomada
> **Comece por aqui** ao retomar o projeto em outra sessão. Estado atual + pendências, sempre atualizados ao fim de cada etapa de trabalho.

## ✅ Estado atual (2026-07-04)

- Versão **`1.0.358`** — plugin funcional, com as **três janelas em produção**: Branch Hierarchy (árvore não-modal), GitFlow e Restore (10 abas).
- Build/versionamento/deploy automatizados via `build.ps1` (docs carimbados a cada build). Ver [[💻 Ambiente Local (Dev)]].
- Distribuição: `.nupkg` no feed do nuget.org (instalável pelo Plugin Manager do GitExtensions) + repositório GitHub do owner `zimerfeld`. Ver [[🚀 Deploy em Produção (Prod)]].
- READMEs bilíngues (`README.md` / `README.pt-BR.md` / `README.en-US.md`) e cofre em sincronia com a versão.
- **Cofre reestruturado para o padrão "Cofre de Neurônios v2"** em 2026-07-04 (pastas por prioridade: impacto → reutilização → uso → operação; histórico de sessões removido por decisão do usuário).

## 📋 Próximos passos

- [ ] **Traduzir as notas do cofre para inglês** — criar os pares `(EN)` das notas de conteúdo (etapa seguinte da reestruturação v2; ver lista de notas sem par na Home).
- [ ] Manter atualizada a **contagem de adoção** (clones/downloads) do repo `zimerfeld/GitExtensions.ZimerfeldTree` (regra global do portfólio).
- [ ] Reespelhar [[📘 README — Instalação, Uso e Build]] quando o README do repo mudar de forma significativa (o `build.ps1` só carimba versão/data).

> [!note] 📥 Sem outras pendências registradas
> O [[📥 Inbox]] está vazio e o cofre não registra bugs abertos nem features planejadas no momento. Ao surgir demanda nova, registrar aqui.

## ✅ Feito recente
- [x] **Fix na landing page — quebra de linha de títulos/subtítulos em PT** — 2026-07-07: a landing page (`index.html`, publicada em **tree.zimerfeld.com** via GitHub Pages) compartilha um template i18n com a regra `html[data-lang="pt"] .lang-pt{display:inline}`, que tornava **todo** elemento em português `inline` — incluindo `h2`/`h3` — fazendo título/subtítulo colarem no texto seguinte quando o site abre em PT (em EN funcionava, pois `h2`/`h3` já são `block` por padrão). **Correção de 1 linha de CSS:** `html[data-lang="pt"] h2.lang-pt,html[data-lang="pt"] h3.lang-pt{display:block}` — restaura a quebra apenas nos títulos/subtítulos em PT, sem afetar o EN. Publicado via GitFlow como **hotfix** (`hotfix/pt-heading-break` → `main`, com back-merge no `develop`, que estava atrás do `main`) + tag **`202607071915pt-heading-break`**; `CNAME` preservado; deploy confirmado ao vivo.

## 🔗 Ligações
- [[🏠 Home]] — porta de entrada do cofre
- [[🌳 GitExtensions.ZimerfeldTree]] — nota-mãe do projeto
