---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [decisao, adr, ui, janela, nao-modal, singleton]
status: aceita
criado: 2026-06-01
---

# ADR — Janela principal não-modal e singleton

## Contexto
A árvore de branches é consultada e manipulada **continuamente** durante o trabalho (checkout, criar branch, GitFlow, restore). Um diálogo modal bloquearia o GitExtensions e forçaria abrir/fechar a cada operação.

## Decisão
Expor a janela principal (`BranchHierarchyForm`) como **`Form` não-modal, singleton por sessão**, `Sizable` e `CenterScreen`, aberta pelo menu Plugins → ZimerfeldTree. Fica disponível ao lado do host enquanto se trabalha; reabrir traz a instância existente à frente. O **working directory** vem do próprio `cboRepo` (lido do XML de settings do GitExtensions), **independente** do repositório ativo no host.

## Consequências
**Positivas:**
- Fluxo fluido — a janela permanece aberta durante o trabalho.
- Desacoplamento do host → robusta entre versões do GitExtensions.
- O `cboRepo` independente permite operar num repo diferente do aberto no host.

**Trade-offs:**
- A janela **sonda o estado sozinha** (`RefreshTreeAsync`, `FileSystemWatcher`, `git fetch` ao abrir) em vez de reagir a eventos do host.
- Commit/Push reaproveitam os diálogos nativos via `IGitUICommands.WithWorkingDirectory(dir)`.

## 🔗 Relacionado
- [[🪟 BranchHierarchyForm (PT)|🪟 BranchHierarchyForm]]
- [[🌳 ZimerfeldTreePlugin (PT)|🌳 ZimerfeldTreePlugin]]
- [[🏗️ Arquitetura (PT)|🏗️ Arquitetura]]
