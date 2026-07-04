---
tipo: fluxo
tags: [fluxo, arvore, abertura, overlay, etapa1]
atualizado: 2026-07-04
---

# Fluxo: 1 — Abrir e navegar a árvore

Abertura da janela principal (`BranchHierarchyForm`) e navegação da hierarquia de branches.

![[Anexos/ScreenShots/ScreenshotBranchHierarchy.png]]

## Passos

```
1. Plugins → ZimerfeldTree   →  ZimerfeldTreePlugin.Execute()
        │  abre/traz-à-frente a janela singleton (não faz git no construtor)
        ▼
2. Shown  →  FirstLoadAsync → RefreshTreeAsync(showOverlay:true)  (Task.Run)
        │  overlay 0→100% · 8 passos · botão Cancelar · form bloqueado
        │  1 único `git log --all` → grafo de commits → pais por BFS (O(commits))
        ▼
3. Árvore populada em 3 seções: LOCAL (N) · REMOTES (N) · TAGS (N)
        │  branch atual em negrito · status bar Local/Remoto/Tags
        │  overlay fecha assim que a árvore aparece (1ª abertura, sem atraso)
        ▼
4. git fetch da upstream em background  →  corrige contadores ↓N / ↑N
        ▼
5. Navegar: filtro em tempo real · expandir/recolher (estado persistido)
        │  botões Pull / Push / Commit / Excluir / GitFlow / Restore (agem no HEAD)
        │  menu de contexto age na branch clicada
```

## Detalhes

- **Working Directory** vem do `cboRepo` (lido de `GitExtensions.settings`), independente do host. Trocar de repo reaponta o `FileSystemWatcher` do contador de Commit.
- **Overlay** só na 1ª exibição e recargas explícitas — não reaparece ao voltar de GitFlow/Restore (árvore já atualizada ao vivo).
- **Contador de Commit ao vivo** atualiza o `(N)` do botão Commit silenciosamente (debounce 600 ms, ignora `.git`).

## Relacionado

- [[2 - GitFlow (Start a Finish)]]
- [[3 - Restore (voltar no tempo)]]
- [[../Arquivos-Chave/BranchHierarchyForm]]
- [[Interface ZimerfeldTree — botões e fluxos]]
