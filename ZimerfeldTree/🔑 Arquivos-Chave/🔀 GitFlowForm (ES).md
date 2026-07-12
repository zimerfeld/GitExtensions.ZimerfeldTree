---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, gitflow, modal]
arquivo: src/GitExtensions.ZimerfeldTree/GitFlowForm.cs
---

# GitFlowForm.cs

> 🇧🇷 Lee esta página en portugués → [[🔀 GitFlowForm]]
> 🇺🇸 Read this page in English → [[🔀 GitFlowForm (EN)]]

Ventana modal que dirige los comandos `git flow` usando **solo git nativo**. ~758 líneas. Título: `ZimerfeldTree - GitFlow`.

**Ruta:** `src/GitExtensions.ZimerfeldTree/GitFlowForm.cs`

---

## Papel

No depende del binario `git-flow` instalado — cada botón dispara una secuencia de `git` puro. Cubre los cinco tipos: **feature, bugfix, release, hotfix, support**, con operaciones **start / publish / track / update / finish**. Ver [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]] y [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]].

## Reglas específicas

- **Bugfix** solo puede existir **vinculado a una release** — `DoStart` bloquea si no hay release; la base graba un *based-on override* y el bugfix queda anidado bajo la release.
- **based on:** permite una **feature hija de una feature**; en ese caso también ejecuta `git commit --allow-empty -m "chore: start <ref>"` para que la jerarquía sea visible. Ver [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de una feature]].
- **Nombre predeterminado** de release/hotfix precargado con `yyyyMMddHHmm`.
- **Finish release** ejecuta el flujo completo automático (merge en main + tag + merge en develop + push de todo). Ver el mapa Start→git puro en [[👁️ Visão Geral (ES)|Visión General]].

## Botón Initialize

Aplica de una vez las claves `gitflow.*` predeterminadas — ver [[⚙️ git flow - chaves de config (CLI) (ES)|git flow - claves de configuración (CLI)]].

## Relacionado

- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]
- [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]]
- [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de una feature]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
- [[🔀 GitFlow (Start a Finish) (ES)|GitFlow (Start a Finish)]]
