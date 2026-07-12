---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, icone, plugin, resources]
arquivo: src/GitExtensions.ZimerfeldTree/PluginIcon.cs
---

# PluginIcon.cs

> 🇧🇷 Lee esta página en portugués → [[🖼️ PluginIcon]]
> 🇺🇸 Read this page in English → [[🖼️ PluginIcon (EN)]]

Icono del plugin/ventana — "Árbol de la Vida". ~33 líneas.

**Ruta:** `src/GitExtensions.ZimerfeldTree/PluginIcon.cs`

---

## Papel

Carga `Resources/ico.png` **una única vez** y lo mantiene en caché, sirviendo el icono para:
- el elemento de menú **Plugins → ZimerfeldTree** en el host, y
- la **barra de título** de las tres ventanas (`BranchHierarchy` / `GitFlow` / `Restore`).

A diferencia de [[🎨 NodeIcons (ES)|NodeIcons]] (iconos 16×16 de los nodos del árbol), este es el icono único y más grande de identidad del plugin.

## Relacionado

- [[🌳 ZimerfeldTreePlugin (ES)|ZimerfeldTreePlugin]]
- [[🎨 NodeIcons (ES)|NodeIcons]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
