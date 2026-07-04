---
tipo: sistema
tags: [arquitetura, classes, design, i18n, threading, gitextensions]
atualizado: 2026-07-04
versao: 1.0.358
---

# Arquitetura

## Diagrama de classes

```
GitExtensions (host)
    │
    │  MEF (System.ComponentModel.Composition)
    ▼
ZimerfeldTreePlugin        ← [Export(IGitPlugin)] : GitPluginBase
    │  Execute()  → abre/traz-à-frente a janela singleton
    │  captura _commands (Register/Unregister)
    ▼
BranchHierarchyForm (janela principal)   ← Sizable, não-modal, singleton
    │  cboRepo (working dir independente)
    │  RefreshTreeAsync(...) → Task.Run (overlay de progresso)
    │  botões: Pull / Push / Commit / Excluir / GitFlow / Restore
    │        │                       │
    │        ▼                       ▼
    │  GitFlowForm (modal)      RestoreForm (modal)
    │  start/publish/track/     10 abas: restore/revert/
    │  update/finish            reset/reflog/rebase…
    │        │                       │
    ▼        ▼                       ▼
BranchHierarchyService     ← executor de git + parser de saída + montagem da hierarquia
    │  RunGit(args) → (StdOut, StdErr, ExitCode)
    ▼
git (PATH)   ·   modelos: BranchInfo / BranchType (BranchNode.cs)
                 ícones:  NodeIcons (árvore) · PluginIcon (janela)
```

## As classes

### `ZimerfeldTreePlugin` — ponto de entrada
Herda de `GitPluginBase`, exportado via MEF como `IGitPlugin`. **`Execute`** abre (ou traz à frente) a **janela singleton** `BranchHierarchyForm`; **`Register`/`Unregister`** capturam/limpam `_commands` (`IGitUICommands`) usados para abrir os diálogos nativos de Commit/Push/Pull do host. Ver [[../Arquivos-Chave/ZimerfeldTreePlugin]].

### `BranchHierarchyForm` — a janela principal
WinForms `Sizable`, não-modal, `CenterScreen`, singleton por sessão. Árvore em 3 seções fixas (**LOCAL / REMOTES / TAGS**), filtro em tempo real, botões acima da árvore, overlay de progresso na 1ª carga, contador de Commit ao vivo (FileSystemWatcher). O construtor **não** faz git — tudo é lido em background disparado pelo `Shown`. Ver [[../Arquivos-Chave/BranchHierarchyForm]] e [[Interface ZimerfeldTree — botões e fluxos]].

### `GitFlowForm` — a janela GitFlow
Modal. Dirige start/publish/track/update/finish para feature, bugfix, release, hotfix e support usando **apenas git nativo** (não depende do binário `git-flow`). Permite **hierarquia flexível** (feature filha de feature via *based on:*). Ver [[../Arquivos-Chave/GitFlowForm]] e [[Interface GitFlow — botões e fluxos]].

### `RestoreForm` — a janela Restore
Modal, ~980 px, 10 abas da mais segura à mais destrutiva (Plano de Emergência → Restaurar Arquivo/Árvore/Tag → Cherry-Pick → Reverter → Reset → Nova Branch/Tag → Reflog → Descartar Locais → Rebase). Ver [[../Arquivos-Chave/RestoreForm]] e [[Interface Restore — botões e fluxos]].

### `BranchHierarchyService` — executor git + hierarquia
Roda `git` em subprocessos (stdout/stderr redirecionados) e **parseia** a saída. Constrói o grafo de commits com **um único `git log --all`** e resolve pais por BFS → **O(commits)** em vez de O(N²×subprocesso). Contém a lógica dos comandos GitFlow (git puro). Ver [[../Arquivos-Chave/BranchHierarchyService]].

### Modelos e ícones
- `BranchNode.cs` — `BranchInfo` (dados da branch) + enum `BranchType` (Local/Remote/Tag). Ver [[../Arquivos-Chave/BranchNode]].
- `NodeIcons.cs` — ícones 16×16 GDI+ + PNGs embutidos (ImageList da árvore). Ver [[../Arquivos-Chave/NodeIcons]].
- `PluginIcon.cs` — ícone "Árvore da Vida" do plugin/janela (`Resources/ico.png`), carregado 1× e cacheado. Ver [[../Arquivos-Chave/PluginIcon]].

## Desacoplamento do host

> [!important] A janela escolhe o repositório pelo próprio `cboRepo`
> O working directory vem do combo `cboRepo` (populado do XML de settings do GitExtensions), independente do repositório ativo no host. `Register` guarda o `_commands` só para poder abrir os diálogos nativos (Commit/Push/Pull) **no working dir selecionado**. Ver [[../Decisoes/Janela não-modal singleton]].

## Localização (i18n)

Inglês / Português, escolhido **por janela** e memorizado. A janela principal usa `I18n.SetLanguage` global (persistido em `ZimerfeldTree.language.json`); GitFlow e Restore têm seletor próprio persistido em seus arquivos de settings. Traduz **controles/rótulos**, nunca os dados.

## Threading

> A janela **abre instantânea**: o construtor faz **zero trabalho git**. A 1ª carga roda atrás do evento `Shown` (`FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)`).

- **`RefreshTreeAsync`** — coleta branches/tags/hierarquia numa `Task.Run` em thread de fundo e aplica à UI; overlay de progresso (0→100%, 8 passos) na 1ª abertura e nas recargas explícitas.
- **Contador de Commit ao vivo** — `FileSystemWatcher` com debounce de 600 ms → um único `git status` em background, sem rebuild da árvore. Ignora mudanças em `.git`.
- **Verificação do remoto ao abrir** — `git fetch` da upstream roda em segundo plano após `Shown` (offline-safe), corrigindo os contadores Pull/Push.

## Relacionado

- [[../Arquivos-Chave/ZimerfeldTreePlugin]]
- [[../Arquivos-Chave/BranchHierarchyForm]]
- [[../Arquivos-Chave/BranchHierarchyService]]
- [[../Decisoes/Janela não-modal singleton]]
- [[../Decisoes/GitFlow em git puro]]
- [[Dependências]]
- [[Visão Geral]]
