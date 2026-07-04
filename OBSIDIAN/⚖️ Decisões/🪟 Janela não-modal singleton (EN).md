---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, ui, janela, nao-modal, singleton]
status: aceita
criado: 2026-06-01
---

# ADR — Non-modal, singleton main window

## Context
The branch tree is consulted and manipulated **continuously** during work (checkout, creating a branch, GitFlow, restore). A modal dialog would block GitExtensions and force opening/closing on each operation.

## Decision
Expose the main window (`BranchHierarchyForm`) as a **non-modal `Form`, singleton per session**, `Sizable` and `CenterScreen`, opened via the Plugins → ZimerfeldTree menu. It stays available beside the host while you work; reopening brings the existing instance to front. The **working directory** comes from its own `cboRepo` (read from the GitExtensions settings XML), **independent** of the active repository in the host.

## Consequences
**Positive:**
- Smooth flow — the window stays open during work.
- Decoupled from the host → robust across GitExtensions versions.
- The independent `cboRepo` lets you operate on a repo different from the one open in the host.

**Trade-offs:**
- The window **probes state on its own** (`RefreshTreeAsync`, `FileSystemWatcher`, `git fetch` on open) instead of reacting to host events.
- Commit/Push reuse the native dialogs via `IGitUICommands.WithWorkingDirectory(dir)`.

## 🔗 Related
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[🌳 ZimerfeldTreePlugin (EN)|ZimerfeldTreePlugin]]
- [[🏗️ Arquitetura (EN)|Architecture]]
