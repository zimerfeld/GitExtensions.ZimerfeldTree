---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, modelo, dados, enum]
arquivo: src/GitExtensions.ZimerfeldTree/BranchNode.cs
---

# BranchNode.cs

Data models of the tree. ~41 lines.

**Path:** `src/GitExtensions.ZimerfeldTree/BranchNode.cs`

---

## Contents

- **`BranchInfo`** — class with the data of a branch/tag (name, type, whether it is the current one, ahead/behind counters, parent ref, etc.), used by [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]] when assembling the tree.
- **`BranchType`** (enum) — `Local` · `Remote` · `Tag`. Drives the icon chosen in [[🎨 NodeIcons (EN)|NodeIcons]] and the section (LOCAL / REMOTES / TAGS) where the node appears.

## Related

- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]
- [[🎨 NodeIcons (EN)|NodeIcons]]
- [[🏗️ Arquitetura (EN)|Architecture]]
