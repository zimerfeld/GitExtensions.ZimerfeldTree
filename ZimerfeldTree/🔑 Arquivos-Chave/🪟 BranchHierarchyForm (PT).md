---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, janela-principal, arvore]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs
---

# BranchHierarchyForm.cs

Janela principal não-modal — a árvore hierárquica de branches. **~2066 linhas** (a maior parte da UI). Título da barra: `ZimerfeldTree - BranchHierarchy`.

**Caminho:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs`

---

## Papel

`Form` WinForms `Sizable`, não-modal, singleton por sessão, `CenterScreen`. Exibe as branches em 3 seções fixas — **LOCAL / REMOTES / TAGS** — com contadores `(N)`, branch atual em negrito, filtro em tempo real e botões acima da árvore. O construtor **não** faz git; tudo é lido em background após o `Shown`.

## Blocos funcionais

| Bloco | Descrição |
|---|---|
| Árvore | `TreeView` com ancestralidade real (commits/GitFlow) + agrupamento por caminho (`/`). ImageList de [[🎨 NodeIcons (PT)|🎨 NodeIcons]] |
| Carga assíncrona | `FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)` num `Task.Run`; overlay 0→100%, 8 passos, botão Cancelar |
| Botões | Pull / Push / Commit / Excluir / GitFlow / Restore acima da árvore; contadores `↓N` / `↑N` / `(N)`; agem no HEAD |
| Contador de Commit ao vivo | `FileSystemWatcher` na pasta do working dir, debounce 600 ms → `git status` silencioso; ignora `.git` |
| Push protegido | Se a branch está atrás do remoto, oferece **pull --rebase + push automático** |
| Working Directory | `cboRepo` lido de `GitExtensions.settings` (XML) — independente do host |
| Modo Developer | Protege `main`/`master`/`develop` (checkbox bloqueado) quando desligado |
| Filtro | Substring case-insensitive em todas as seções, preservando nós-pai |
| Persistência | Estado de expand/collapse por working dir (`ZimerfeldTree.treestate.json`); UI settings (`ZimerfeldTree.uisettings.json`) |
| Menu de contexto | Ícones embutidos + cabeçalho com a branch em checkout; Baixar/Enviar agem na branch clicada |

## Relacionado

- [[⚙️ BranchHierarchyService (PT)|⚙️ BranchHierarchyService]] — a lógica git por trás
- [[🔀 GitFlowForm (PT)|🔀 GitFlowForm]] · [[⏪ RestoreForm (PT)|⏪ RestoreForm]] — abertas pelos botões
- [[🎨 NodeIcons (PT)|🎨 NodeIcons]]
- [[🏗️ Arquitetura (PT)|🏗️ Arquitetura]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (PT)|🌳 Interface ZimerfeldTree — botões e fluxos]]
