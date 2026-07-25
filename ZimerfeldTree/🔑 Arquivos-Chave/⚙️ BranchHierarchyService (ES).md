---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, git, subprocesso, service, hierarquia, gitflow]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs
---

# BranchHierarchyService.cs

> 🇧🇷 Lee esta página en portugués → [[⚙️ BranchHierarchyService (PT)|⚙️ BranchHierarchyService]]
> 🇺🇸 Read this page in English → [[⚙️ BranchHierarchyService (EN)]]

Ejecutor de `git` como subproceso + parser de la salida + construcción de la jerarquía de branches. ~831 líneas.

**Ruta:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs`

---

## Papel

Toda la lógica git de la ventana principal y de GitFlow vive aquí. Ejecuta `git` en un `ProcessStartInfo` (stdout/stderr redirigidos, UTF-8, sin ventana), parsea la salida y devuelve modelos [[🌿 BranchNode (ES)|BranchInfo]].

## Recopilación y jerarquía

- **Grafo de commits en memoria:** un único `git log --all` construye el grafo; los padres se resuelven por **BFS** → **O(commits)** en lugar de O(N²×subproceso).
- LOCAL/REMOTES combinan **ancestralidad real** (parentesco por commits / GitFlow) **+ agrupación por ruta** (`/`).
- `BuildGitFlowParentMap` usa la ancestralidad real para encontrar el padre GitFlow (p. ej., la release padre de un bugfix).
- Detecta **violaciones** de jerarquía (p. ej., `violLocalBugfix`) que activan la auto-organización de GitFlow.

## Comandos GitFlow (git puro)

Implementa las secuencias de start / publish / track / update / finish para los cinco tipos. El mapa completo **Start→git** está en [[👁️ Visão Geral (ES)|Visión General]] y [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]].

## Limitaciones conocidas

- **Dos branches en el mismo commit** no forman una relación padre-hijo (el BFS nunca encuentra una como padre de la otra) → ambas se convierten en raíces; solución automática = commit vacío en el Start con *based on*. Ver [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]].
- Una **branch real no puede ser un nodo padre** de otra (el ref sería archivo **y** directorio a la vez).

## Relacionado

- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[🔀 GitFlowForm (ES)|GitFlowForm]] · [[⏪ RestoreForm (ES)|RestoreForm]]
- [[🌿 BranchNode (ES)|BranchNode]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
