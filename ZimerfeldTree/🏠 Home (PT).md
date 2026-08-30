---
tipo: moc
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-08-29
versao: 1.0.363
tags: [moc, home, zimerfeld, tree, gitextensions, gitflow]
---

# 🏠 GitExtensions.ZimerfeldTree — Cofre de Neurônios

> [!abstract] 🧠 O que é este cofre
> Memória persistente do Claude para o projeto **GitExtensions.ZimerfeldTree** — plugin para GitExtensions que exibe as branches em **árvore hierárquica** e disponibiliza **GitFlow visual** em git puro. O cofre é lido no início de cada sessão e atualizado sempre que algo relevante muda.

![[ScreenshotBranchHierarchy.png]]

## ⚡ Resumo executivo

- **O que é:** extensão (plugin MEF) para o **GitExtensions** que substitui a lista plana de branches por uma **árvore hierárquica** (LOCAL / REMOTES / TAGS) e expõe o fluxo **GitFlow** em cliques, sem depender do binário `git-flow` instalado. Três janelas dedicadas: **Branch Hierarchy** (árvore não-modal), **GitFlow** (start/publish/track/update/finish) e **Restore** (10 abas de "voltar no tempo"). Ícone próprio "Árvore da Vida" (GDI+ / `Resources/ico.png`).
- **Problema que resolve:** navegar branches numa lista plana e conduzir GitFlow por linha de comando é lento e propenso a erro. O plugin dá **contexto visual de parentesco** entre branches e transforma start/publish/track/update/finish em botões — com log de cada comando git executado.
- **Diferenciais:** **GitFlow flexível** (feature filha de feature via *based on:*, com *finish* em cascata até `develop`); janela **não-modal** e assíncrona (abre na hora, com overlay de progresso); **contador de Commit ao vivo** (FileSystemWatcher); **push protegido** contra divergência (oferece pull --rebase + push automático); **Modo Developer** que protege `main`/`master`/`develop`; janela **Restore** com 10 níveis de recuperação; **i18n** (Inglês / Português, por janela); banner de patrocínio (GitHub Sponsors + Ko-fi).
- **Stack:** C# / WinForms `Library`, alvo **net9.0-windows**, assembly `GitExtensions.Plugins.ZimerfeldTree.dll`, empacotado como **nupkg**; build e versionamento automatizados via `build.ps1`.
- **Estado atual:** versão **`1.0.361`** — funcional, com as três janelas em produção.
- **Público-alvo:** desenvolvedores e times que usam GitExtensions no Windows e adotam (ou querem adotar) GitFlow com clareza visual da hierarquia de branches.
- **Ângulo de negócio/portfólio:** produto **open source** sob o owner `zimerfeld`, reforçando autoridade técnica e servindo de vitrine para adoção (clones/downloads do NuGet) e captação de patrocínio. Integra-se ao irmão **GitExtensions.ZimerfeldCommitMsg**.

## 🧭 Navegação por prioridade

### 1️⃣ 🔑 Impacto — Arquivos-Chave
> Arquivos que, se manipulados, têm grande impacto no sistema.
- [[🌳 ZimerfeldTreePlugin (PT)|🌳 ZimerfeldTreePlugin]] — ponto de entrada MEF (`IGitPlugin`)
- [[🪟 BranchHierarchyForm (PT)|🪟 BranchHierarchyForm]] — a janela principal (árvore, botões, contador ao vivo)
- [[⚙️ BranchHierarchyService (PT)|⚙️ BranchHierarchyService]] — executor git + montagem da hierarquia
- [[🔀 GitFlowForm (PT)|🔀 GitFlowForm]] — a janela GitFlow (start/finish, based on)
- [[⏪ RestoreForm (PT)|⏪ RestoreForm]] — a janela Restore (10 abas)
- [[🔧 build.ps1 (PT)|🔧 build.ps1]] — build + versionamento + deploy
- [[🌿 BranchNode (PT)|🌿 BranchNode]] — modelos `BranchInfo` + `BranchType`
- [[🎨 NodeIcons (PT)|🎨 NodeIcons]] — ícones 16×16 da árvore (GDI+ + PNGs embutidos)
- [[🖼️ PluginIcon (PT)|🖼️ PluginIcon]] — ícone do plugin/janela (`Resources/ico.png`)

### 2️⃣ 🧩 Reutilização — Sistemas
> Subsistemas reutilizados por várias partes do projeto.
- [[👁️ Visão Geral (PT)|👁️ Visão Geral]] — o que o plugin faz, stack, as três janelas, versão atual
- [[🏗️ Arquitetura (PT)|🏗️ Arquitetura]] — as classes (Plugin → Forms → Service), threading, i18n, desacoplamento
- [[🏷️ Versionamento (PT)|🏷️ Versionamento]] — ciclo `build.ps1` / nuspec / csproj / carimbo de docs
- [[📦 Dependências (PT)|📦 Dependências]] — resumo de sistema (host + `git` + build)
- [[📦 Dependências do ZimerfeldTree (PT)|📦 Dependências do ZimerfeldTree]] — versão detalhada, passo a passo, com downloads

### 3️⃣ 🔀 Uso — Fluxos
> Fluxos de uso passo a passo.
- [[🌲 Abrir e navegar a árvore (PT)|🌲 Abrir e navegar a árvore]] — abertura assíncrona, overlay, navegação da hierarquia
- [[🔀 GitFlow (Start a Finish) (PT)|🔀 GitFlow (Start a Finish)]] — o ciclo GitFlow em git puro
- [[⏪ Restore (voltar no tempo) (PT)|⏪ Restore (voltar no tempo)]] — as 10 abas de recuperação

## 🚀 Operação
- [[💻 Ambiente Local (Dev) (PT)|💻 Ambiente Local (Dev)]] — `.\build.ps1` (Admin) · deploy rápido: `.\tools\update-dll.ps1`
- [[🚀 Deploy em Produção (Prod) (PT)|🚀 Deploy em Produção (Prod)]] — `.\build.ps1` → publicar `.nupkg` (nuget.org + GitHub release)

## ⚖️ Decisões
- [[🌳 Árvore hierárquica vs lista plana (PT)|🌳 Árvore hierárquica vs lista plana]] — por que árvore em vez da lista plana padrão
- [[🔀 GitFlow em git puro (PT)|🔀 GitFlow em git puro]] — sem dependência do binário `git-flow`
- [[🌿 GitFlow flexível — feature filha de feature (PT)|🌿 GitFlow flexível — feature filha de feature]] — hierarquia além do GitFlow clássico
- [[🪟 Janela não-modal singleton (PT)|🪟 Janela não-modal singleton]] — janela persistente + working dir independente
- [[🛡️ Modo Developer protege main-develop (PT)|🛡️ Modo Developer protege main-develop]] — proteção contra exclusão acidental
- [[⏪ Restore — central de voltar no tempo (PT)|⏪ Restore — central de voltar no tempo]] — 10 abas ordenadas por risco

## 📚 Conhecimento
- [[🌳 Interface ZimerfeldTree — botões e fluxos (PT)|🌳 Interface ZimerfeldTree — botões e fluxos]] — a árvore não-modal (Pull/Push/Commit/Excluir/GitFlow/Restore, filtro, overlay, contador ao vivo)
- [[🔀 Interface GitFlow — botões e fluxos (PT)|🔀 Interface GitFlow — botões e fluxos]] — start/publish/track/update/finish e a hierarquia *based on:*
- [[⏪ Interface Restore — botões e fluxos (PT)|⏪ Interface Restore — botões e fluxos]] — as 10 abas de "voltar no tempo", da mais segura à mais destrutiva
- [[🧩 Plugin MEF para GitExtensions (PT)|🧩 Plugin MEF para GitExtensions]] — modelo MEF de plugin (`IGitPlugin`)
- [[⚙️ git flow - chaves de config (CLI) (PT)|⚙️ git flow - chaves de config (CLI)]] — chaves `gitflow.*` esperadas pelo CLI vs. as gravadas pelo GitExtensions
- [[🌿 Hierarquia de branches — branches no mesmo commit (PT)|🌿 Hierarquia de branches — branches no mesmo commit]] — por que duas branches no mesmo commit não formam pai-filho e a solução (commit vazio no Start)
- [[📘 README — Instalação, Uso e Build (PT)|📘 README — Instalação, Uso e Build]] — espelho do README

## 💼 Negócio
- [[🌳 GitExtensions.ZimerfeldTree (PT)|🌳 GitExtensions.ZimerfeldTree]] — nota-mãe do projeto (espelha o README: objetivo, estrutura, funcionalidades, limitações, financiamento)

## 🧭 Meta
- [[🧭 Como usar este cofre (PT)|🧭 Como usar este cofre]] — protocolo de leitura/escrita do Claude
- [[🔑 Fatos-Chave (PT)|🔑 Fatos-Chave]] — verdades sempre úteis (paths, nomes, convenções)
- [[📥 Inbox (PT)|📥 Inbox]] — captura rápida
- [[👤 Renato (PT)|👤 Renato]] — contexto e preferências
- [[🧰 RTK (PT)|🧰 RTK]] — proxy CLI economizador de tokens

## 📌 Retomada
- [[📌 Backlog (PT)|📌 Backlog]] — **comece por aqui** ao retomar o projeto em outra sessão
