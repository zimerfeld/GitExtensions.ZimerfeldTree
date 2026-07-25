---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, icones, gdi, resources, imagelist]
arquivo: src/GitExtensions.ZimerfeldTree/NodeIcons.cs
---

# NodeIcons.cs

> 🇧🇷 Lee esta página en portugués → [[🎨 NodeIcons (PT)|🎨 NodeIcons]]
> 🇺🇸 Read this page in English → [[🎨 NodeIcons (EN)]]

Iconos 16×16 del árbol — PNGs embebidos con fallback dibujado en GDI+. ~381 líneas.

**Ruta:** `src/GitExtensions.ZimerfeldTree/NodeIcons.cs`

---

## Cómo funciona

- Genera/carga los iconos 16×16 del `ImageList` del árbol. Índices: **0–4** genéricos, **5–7** secciones, **8–15** GitFlow/hoja.
- **`LoadEmbedded`** lee el recurso mediante `GitExtensions.ZimerfeldTree.Resources.<archivo>` y lo redimensiona a 16×16. Si falta o no se puede leer, recurre al **glifo GDI+ de reserva** → el build nunca se rompe por falta de imagen (cada `<EmbeddedResource>` es `Condition="Exists(...)"`).
- **Grupo de remote (`origin`)** usa `Resources\origin.png` (cohete) vía `NodeIcons.Remote`, mapeado en `GetFolderIconIndex`.
- **Develop (índice 9)** usa `Resources\develop.png`, fallback `Wrench()`.

## PNGs de nodos (Resources/)

`master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png`, `origin.png`, `remote-branch.png`, `tag.png` + los `ctx-*.png` del menú contextual. `ctx-pull`/`ctx-push` (flecha ↓ azul / ↑ verde) también alimentan los botones Pull/Push.

## Relacionado

- [[🌿 BranchNode (ES)|BranchNode]]
- [[🖼️ PluginIcon (ES)|PluginIcon]]
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
