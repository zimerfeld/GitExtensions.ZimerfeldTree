---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, icone, plugin, resources]
arquivo: src/GitExtensions.ZimerfeldTree/PluginIcon.cs
---

# PluginIcon.cs

Plugin/window icon — "Tree of Life". ~33 lines.

**Path:** `src/GitExtensions.ZimerfeldTree/PluginIcon.cs`

---

## Role

Loads `Resources/ico.png` **once** and keeps it cached, serving the icon for:
- the **Plugins → ZimerfeldTree** menu item in the host, and
- the **title bar** of the three windows (`BranchHierarchy` / `GitFlow` / `Restore`).

Unlike [[🎨 NodeIcons (EN)|NodeIcons]] (16×16 icons of the tree nodes), this is the single, larger identity icon of the plugin.

## Related

- [[🌳 ZimerfeldTreePlugin (EN)|ZimerfeldTreePlugin]]
- [[🎨 NodeIcons (EN)|NodeIcons]]
- [[🏗️ Arquitetura (EN)|Architecture]]
