---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, modelo, dados, enum]
arquivo: src/GitExtensions.ZimerfeldTree/BranchNode.cs
---

# BranchNode.cs

> 🇧🇷 Lee esta página en portugués → [[🌿 BranchNode (PT)|🌿 BranchNode]]
> 🇺🇸 Read this page in English → [[🌿 BranchNode (EN)]]

Modelos de datos del árbol. ~41 líneas.

**Ruta:** `src/GitExtensions.ZimerfeldTree/BranchNode.cs`

---

## Contenido

- **`BranchInfo`** — clase con los datos de una branch/tag (nombre, tipo, si es la actual, contadores ahead/behind, ref padre, etc.), usada por [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]] al construir el árbol.
- **`BranchType`** (enum) — `Local` · `Remote` · `Tag`. Determina el icono elegido en [[🎨 NodeIcons (ES)|NodeIcons]] y la sección (LOCAL / REMOTES / TAGS) donde aparece el nodo.

## Relacionado

- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]
- [[🎨 NodeIcons (ES)|NodeIcons]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
