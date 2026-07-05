---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, mef, plugin, entrypoint, gitextensions]
arquivo: src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs
---

# ZimerfeldTreePlugin.cs

MEF entry point of the plugin (`IGitPlugin`). ~238 lines.

**Path:** `src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs`

---

## Role

Class exported via MEF (`[Export(typeof(IGitPlugin))]`), inheriting from `GitPluginBase`. It is the object that **GitExtensions** instantiates and lists under the **Plugins → ZimerfeldTree** menu. See [[🧩 Plugin MEF para GitExtensions (EN)|MEF Plugin for GitExtensions]].

## Main members

| Member | Role |
|---|---|
| `Execute(...)` | Opens (or brings to front) the **singleton window** `BranchHierarchyForm`. Reuses the existing instance if already open. |
| `Register(IGitUICommands)` | Stores `_commands` so it can open the host's native dialogs (Commit/Push/Pull) in the chosen working dir. |
| `Unregister(...)` | Clears the reference to `_commands`. |
| Icon | Uses `PluginIcon` (`Resources/ico.png`, "Tree of Life") — see [[🖼️ PluginIcon (EN)|PluginIcon]]. |

## Notes

- The icon and the window titles always keep the prefix **`ZimerfeldTree - `** (`BranchHierarchy` / `GitFlow` / `Restore`).
- The working directory does **not** come from the host's active repository — it comes from the window's `cboRepo`. See [[🪟 Janela não-modal singleton (EN)|Non-modal singleton window]].

## Related

- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[🏗️ Arquitetura (EN)|Architecture]]
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF Plugin for GitExtensions]]
