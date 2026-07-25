---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, janela-principal, arvore]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs
---

# BranchHierarchyForm.cs

> 🇧🇷 Lee esta página en portugués → [[🪟 BranchHierarchyForm (PT)|🪟 BranchHierarchyForm]]
> 🇺🇸 Read this page in English → [[🪟 BranchHierarchyForm (EN)]]

Ventana principal no modal — el árbol jerárquico de branches. **~2066 líneas** (la mayor parte de la UI). Título de la barra: `ZimerfeldTree - BranchHierarchy`.

**Ruta:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyForm.cs`

---

## Papel

`Form` WinForms `Sizable`, no modal, singleton por sesión, `CenterScreen`. Muestra las branches en 3 secciones fijas — **LOCAL / REMOTES / TAGS** — con contadores `(N)`, la branch actual en negrita, filtro en tiempo real y botones encima del árbol. El constructor **no** hace git; todo se lee en segundo plano después del `Shown`.

## Bloques funcionales

| Bloque | Descripción |
|---|---|
| Árbol | `TreeView` con ancestralidad real (commits/GitFlow) + agrupación por ruta (`/`). ImageList de [[🎨 NodeIcons (ES)|NodeIcons]] |
| Carga asíncrona | `FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)` en un `Task.Run`; overlay 0→100%, 8 pasos, botón Cancelar |
| Botones | Pull / Push / Commit / Eliminar / GitFlow / Restore encima del árbol; contadores `↓N` / `↑N` / `(N)`; actúan sobre HEAD |
| Contador de Commit en vivo | `FileSystemWatcher` en la carpeta del working dir, debounce de 600 ms → `git status` silencioso; ignora `.git` |
| Push protegido | Si la branch está detrás del remoto, ofrece **pull --rebase + push automático** |
| Working Directory | `cboRepo` leído de `GitExtensions.settings` (XML) — independiente del host |
| Modo Developer | Protege `main`/`master`/`develop` (checkbox bloqueado) cuando está desactivado |
| Filtro | Subcadena sin distinción de mayúsculas/minúsculas en todas las secciones, preservando los nodos padre |
| Persistencia | Estado de expandir/contraer por working dir (`ZimerfeldTree.treestate.json`); configuración de UI (`ZimerfeldTree.uisettings.json`) |
| Menú contextual | Iconos integrados + encabezado con la branch en checkout; Pull/Push actúan sobre la branch clicada |

## Relacionado

- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]] — la lógica git detrás
- [[🔀 GitFlowForm (ES)|GitFlowForm]] · [[⏪ RestoreForm (ES)|RestoreForm]] — abiertas por los botones
- [[🎨 NodeIcons (ES)|NodeIcons]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
