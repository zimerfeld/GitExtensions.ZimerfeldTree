---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, mef, plugin, entrypoint, gitextensions]
arquivo: src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs
---

# ZimerfeldTreePlugin.cs

> 🇧🇷 Lee esta página en portugués → [[🌳 ZimerfeldTreePlugin]]
> 🇺🇸 Read this page in English → [[🌳 ZimerfeldTreePlugin (EN)]]

Punto de entrada MEF del plugin (`IGitPlugin`). ~238 líneas.

**Ruta:** `src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs`

---

## Papel

Clase exportada vía MEF (`[Export(typeof(IGitPlugin))]`), heredando de `GitPluginBase`. Es el objeto que **GitExtensions** instancia y lista en el menú **Plugins → ZimerfeldTree**. Ver [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]].

## Miembros principales

| Miembro | Papel |
|---|---|
| `Execute(...)` | Abre (o trae al frente) la **ventana singleton** `BranchHierarchyForm`. Reutiliza la instancia existente si ya está abierta. |
| `Register(IGitUICommands)` | Guarda `_commands` para poder abrir los diálogos nativos del host (Commit/Push/Pull) en el working dir elegido. |
| `Unregister(...)` | Limpia la referencia a `_commands`. |
| Icono | Usa `PluginIcon` (`Resources/ico.png`, "Árbol de la Vida") — ver [[🖼️ PluginIcon (ES)|PluginIcon]]. |

## Notas

- El icono y el título de las ventanas mantienen siempre el prefijo **`ZimerfeldTree - `** (`BranchHierarchy` / `GitFlow` / `Restore`).
- El working directory **no** proviene del repositorio activo del host — proviene del `cboRepo` de la ventana. Ver [[🪟 Janela não-modal singleton (ES)|Ventana no modal singleton]].

## Relacionado

- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
