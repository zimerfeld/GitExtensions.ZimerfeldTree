---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [fluxo, gitflow, start, finish, etapa2]
---

# Flujo: 2 — GitFlow (Start → Finish)

> 🇧🇷 Lee esta página en portugués → [[🔀 GitFlow (Start a Finish)]]
> 🇺🇸 Read this page in English → [[🔀 GitFlow (Start a Finish) (EN)]]

Ventana `GitFlowForm`, abierta con el botón **GitFlow** o desde el menú contextual. Dirige el ciclo en **git puro** (sin el binario `git-flow`).

![[ScreenshotGitFlow.png]]

## Pasos

```
1. [GitFlow]  →  elegir tipo (feature / bugfix / release / hotfix / support)
        │  nombre (release/hotfix pre-rellenan yyyyMMddHHmm)
        │  opcional: based on → derivar de otra feature (feature hija de feature)
        ▼
2. [Start]    →  git checkout -b <prefijo><nombre> <base>
        │  feature/release ← develop · hotfix/support ← main · bugfix ← release/* (obligatorio)
        │  based on → git commit --allow-empty (hace visible la jerarquía)
        ▼
3. [Publish]  →  git push --set-upstream <remote> <prefijo><nombre>
   [Track]    →  git fetch + git checkout -b … --track <remote>/<ref>
   [Update]   →  git fetch + checkout + git merge <remote>/<padre>
        ▼
4. [Finish]   →  merge --no-ff en el destino + branch -d (+ push --delete si existe la remota)
        │  feature/bugfix: destino develop / release padre
        │  hotfix/release: checkout main + merge + git tag + checkout develop + merge
        │  release: flujo completo automático + push de main/develop/tag + expande TAGS
```

## Reglas clave

- **Bugfix** solo existe **vinculado a una release** — Start bloqueado sin release; queda anidado bajo ella.
- **Feature hija de feature** vía *based on:* → *finish* **se encadena** hasta `develop`. Ver [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de feature]].
- **Error de merge (conflicto):** el plugin se detiene y muestra el resultado; resolver con `git merge --abort` o resolver + `git commit`.

> Mapa completo Start→git puro (todos los tipos) en [[👁️ Visão Geral (ES)|Visión General]].

## Relacionado

- [[🌲 Abrir e navegar a árvore (ES)|Abrir y navegar el árbol]]
- [[🔀 GitFlowForm (ES)|GitFlowForm]]
- [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz de GitFlow — botones y flujos]]
- [[⚙️ git flow - chaves de config (CLI) (ES)|git flow - claves de config (CLI)]]
