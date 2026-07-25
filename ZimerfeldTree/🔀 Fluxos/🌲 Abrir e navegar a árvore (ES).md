---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [fluxo, arvore, abertura, overlay, etapa1]
---

# Flujo: 1 — Abrir y navegar el árbol

> 🇧🇷 Lee esta página en portugués → [[🌲 Abrir e navegar a árvore (PT)|🌲 Abrir e navegar a árvore]]
> 🇺🇸 Read this page in English → [[🌲 Abrir e navegar a árvore (EN)]]

Apertura de la ventana principal (`BranchHierarchyForm`) y navegación de la jerarquía de branches.

![[ScreenshotBranchHierarchy.png]]

## Pasos

```
1. Plugins → ZimerfeldTree   →  ZimerfeldTreePlugin.Execute()
        │  abre/trae al frente la ventana singleton (no hace git en el constructor)
        ▼
2. Shown  →  FirstLoadAsync → RefreshTreeAsync(showOverlay:true)  (Task.Run)
        │  overlay 0→100% · 8 pasos · botón Cancelar · form bloqueado
        │  1 único `git log --all` → grafo de commits → padres por BFS (O(commits))
        ▼
3. Árbol poblado en 3 secciones: LOCAL (N) · REMOTES (N) · TAGS (N)
        │  branch actual en negrita · barra de estado Local/Remoto/Tags
        │  el overlay se cierra en cuanto aparece el árbol (1ª apertura, sin retraso)
        ▼
4. git fetch del upstream en segundo plano  →  corrige contadores ↓N / ↑N
        ▼
5. Navegar: filtro en tiempo real · expandir/colapsar (estado persistido)
        │  botones Pull / Push / Commit / Eliminar / GitFlow / Restore (actúan sobre el HEAD)
        │  el menú contextual actúa sobre la branch clicada
```

## Detalles

- **Working Directory** viene del `cboRepo` (leído de `GitExtensions.settings`), independiente del host. Cambiar de repo reapunta el `FileSystemWatcher` del contador de Commit.
- **Overlay** solo en la 1ª visualización y en recargas explícitas — no reaparece al volver de GitFlow/Restore (el árbol ya está actualizado en vivo).
- **Contador de Commit en vivo** actualiza el `(N)` del botón Commit silenciosamente (debounce 600 ms, ignora `.git`).

## Relacionado

- [[🔀 GitFlow (Start a Finish) (ES)|GitFlow (Start a Finish)]]
- [[⏪ Restore (voltar no tempo) (ES)|Restore (volver en el tiempo)]]
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz de ZimerfeldTree — botones y flujos]]
