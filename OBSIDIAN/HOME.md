---
tipo: moc
tags: [home, moc, zimerfeld, tree, gitextensions, gitflow]
atualizado: 2026-07-04
versao: 1.0.358
---

# 🌳 ZimerfeldTree — Mapa do Cofre

![[Anexos/ScreenShots/ScreenshotBranchHierarchy.png]]

Plugin para **GitExtensions** (Windows) que exibe as branches do repositório **hierarquicamente em árvore** — mostrando branches filhas em vez da lista plana padrão — e disponibiliza a metodologia **GitFlow** de forma visual, fácil e intuitiva. Três janelas dedicadas: **Branch Hierarchy** (árvore não-modal), **GitFlow** (start/publish/track/update/finish em git puro) e **Restore** (10 abas de "voltar no tempo"). Ícone próprio "Árvore da Vida" desenhado/embutido (GDI+ / `Resources/ico.png`).

---

## ⚡ Resumo executivo

- **O que é:** extensão (plugin MEF) para o **GitExtensions** que substitui a lista plana de branches por uma **árvore hierárquica** (LOCAL / REMOTES / TAGS) e expõe o fluxo **GitFlow** em cliques, sem depender do binário `git-flow` instalado.
- **Problema que resolve:** navegar branches numa lista plana e conduzir GitFlow por linha de comando é lento e propenso a erro. O plugin dá **contexto visual de parentesco** entre branches e transforma start/publish/track/update/finish em botões — com log de cada comando git executado.
- **Diferenciais:** **GitFlow flexível** (feature filha de feature via *based on:*, com *finish* em cascata até `develop`); janela **não-modal** e assíncrona (abre na hora, com overlay de progresso); **contador de Commit ao vivo** (FileSystemWatcher); **push protegido** contra divergência (oferece pull --rebase + push automático); **Modo Developer** que protege `main`/`master`/`develop`; janela **Restore** com 10 níveis de recuperação; **i18n** (Inglês / Português, por janela); banner de patrocínio (GitHub Sponsors + Ko-fi).
- **Stack:** C# / WinForms `Library`, alvo **net9.0-windows**, assembly `GitExtensions.Plugins.ZimerfeldTree.dll`, empacotado como **nupkg**; build e versionamento automatizados via `build.ps1`.
- **Estado atual:** versão **`1.0.358`** — funcional, com as três janelas em produção.
- **Público-alvo:** desenvolvedores e times que usam GitExtensions no Windows e adotam (ou querem adotar) GitFlow com clareza visual da hierarquia de branches.
- **Ângulo de negócio/portfólio:** produto **open source** sob o owner `zimerfeld`, reforçando autoridade técnica e servindo de vitrine para adoção (clones/downloads do NuGet) e captação de patrocínio. Integra-se ao irmão **GitExtensions.ZimerfeldCommitMsg**.

---

## Navegação

### Sistema
- [[Visão Geral]] — o que o plugin faz, stack, as três janelas, versão atual
- [[Arquitetura]] — as classes (Plugin → Forms → Service), threading, i18n, desacoplamento
- [[Versionamento]] — ciclo `build.ps1` / nuspec / csproj / carimbo de docs
- [[Dependências]] — resumo de sistema (host + `git` + build)

### As três janelas (interfaces)
- [[Interface ZimerfeldTree — botões e fluxos]] — a árvore não-modal (Pull/Push/Commit/Excluir/GitFlow/Restore, filtro, overlay, contador ao vivo)
- [[Interface GitFlow — botões e fluxos]] — start/publish/track/update/finish e a hierarquia *based on:*
- [[Interface Restore — botões e fluxos]] — as 10 abas de "voltar no tempo", da mais segura à mais destrutiva

### Fluxos
- [[1 - Abrir e navegar a árvore]] — abertura assíncrona, overlay, navegação da hierarquia
- [[2 - GitFlow (Start a Finish)]] — o ciclo GitFlow em git puro
- [[3 - Restore (voltar no tempo)]] — as 10 abas de recuperação

### Arquivos-Chave
- [[ZimerfeldTreePlugin]] — ponto de entrada MEF (`IGitPlugin`)
- [[BranchHierarchyForm]] — a janela principal (árvore, botões, contador ao vivo)
- [[GitFlowForm]] — a janela GitFlow (start/finish, based on)
- [[RestoreForm]] — a janela Restore (10 abas)
- [[BranchHierarchyService]] — executor git + montagem da hierarquia
- [[BranchNode]] — modelos `BranchInfo` + `BranchType`
- [[NodeIcons]] · [[PluginIcon]] — ícones da árvore e do plugin
- [[build.ps1]] — build + versionamento + deploy

### Decisões Arquiteturais
- [[Árvore hierárquica vs lista plana]] — por que árvore em vez da lista plana padrão
- [[GitFlow em git puro]] — sem dependência do binário `git-flow`
- [[GitFlow flexível — feature filha de feature]] — hierarquia além do GitFlow clássico
- [[Janela não-modal singleton]] — janela persistente + working dir independente
- [[Modo Developer protege main-develop]] — proteção contra exclusão acidental
- [[Restore — central de voltar no tempo]] — 10 abas ordenadas por risco

### Conhecimento
- [[Plugin MEF para GitExtensions]] — modelo MEF de plugin (`IGitPlugin`)
- [[git flow - chaves de config (CLI)]] — chaves `gitflow.*` esperadas pelo CLI vs. as gravadas pelo GitExtensions
- [[Hierarquia de branches — branches no mesmo commit]] — por que duas branches no mesmo commit não formam pai-filho e a solução (commit vazio no Start)
- [[README — Instalação, Uso e Build]] — espelho do README

### Projeto & Pessoas
- [[GitExtensions.ZimerfeldTree]] — nota-hub do projeto (espelha o README: objetivo, estrutura, funcionalidades, limitações)
- [[Renato]] — contexto e preferências

### Índice do cofre
- [[🧠 HOME - Cofre de Neurônios]] — MOC do cofre (Dataview, sessões, tags)
- [[🔑 Fatos-Chave]] — verdades sempre úteis (paths, nomes, convenções)
- [[🧭 Como usar este cofre]] — protocolo de leitura/escrita
- [[📥 Inbox]] — captura rápida

### Sessões recentes
- [[2026-06-17 - Restore expandido (revert, reflog, rebase, descartar, nova branch, restaurar árvore)]]
- [[2026-06-17 - Abertura assíncrona com overlay (controles vazios primeiro)]]
- [[2026-06-16 - Pull-Push remoto ao abrir, ícones, menu na branch clicada, aviso de push]]
- [[2026-06-07 - Refresh, overlay, eco e botão Restore]]
- [[2026-06-06 - Push fix, double refresh, Voltar Versão menu]]
- [[2026-06-06 - Hierarquia branches mesmo commit, commit automático no Start]]
- [[2026-06-02 - Checkout TAG, Origin e HEAD detached]]
- [[2026-06-01 - Ícone customizado do develop]]
- [[2026-06-01 - Criação do cofre de neurônios]]

---

## Estrutura de Pastas do Repo

```
GitExtensions.ZimerfeldTree/
├── src/GitExtensions.ZimerfeldTree/
│   ├── ZimerfeldTreePlugin.cs        ← entry point MEF (IGitPlugin)
│   ├── BranchHierarchyForm.cs        ← janela principal: árvore hierárquica (não-modal)
│   ├── GitFlowForm.cs                ← janela GitFlow: start/publish/track/update/finish
│   ├── RestoreForm.cs                ← janela Restore: 10 abas de "voltar no tempo"
│   ├── BranchHierarchyService.cs     ← lógica git: coleta, hierarquia, GitFlow
│   ├── BranchNode.cs                 ← modelos: BranchInfo + enum BranchType
│   ├── NodeIcons.cs                  ← ícones 16×16 da árvore (GDI+ + PNGs embutidos)
│   ├── PluginIcon.cs                 ← ícone do plugin/janela (Resources/ico.png)
│   ├── Resources/                    ← PNGs embutidos (nós, seções, menu, plugin)
│   ├── *.csproj / *.nuspec           ← manifestos MSBuild / NuGet
├── tools/                            ← install/uninstall/update-dll + geradores de ícone
├── OBSIDIAN/                         ← 🧠 este cofre de memória
├── build.ps1                         ← incrementa versão + build + deploy + nupkg + carimba docs
└── README.md / README.pt-BR.md / README.en-US.md
```

---

## Versão Atual

`1.0.358` — compilada em `net9.0-windows`, WinForms `Library`, esquema `major.minor.BUILD` gerenciado pelo `build.ps1` (fonte da verdade: `.nuspec` / `.csproj`).
