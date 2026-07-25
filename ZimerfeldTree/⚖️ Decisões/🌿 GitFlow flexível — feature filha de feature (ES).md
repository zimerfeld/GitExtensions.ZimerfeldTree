---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, hierarquia, feature]
status: aceita
criado: 2026-06-18
---

# ADR — GitFlow flexible: feature hija de feature

> 🇧🇷 Lee esta página en portugués → [[🌿 GitFlow flexível — feature filha de feature (PT)|🌿 GitFlow flexível — feature filha de feature]]
> 🇺🇸 Read this page in English → [[🌿 GitFlow flexível — feature filha de feature (EN)]]

## Contexto
El **GitFlow clásico** no contempla que una feature sea hija de otra feature — todas las `feature/*` derivan de `develop` y son hermanas entre sí. Pero en el desarrollo real es común encadenar etapas: una feature grande cuyo trabajo se ramifica en sub-features dependientes que solo tienen sentido sobre la feature padre.

## Decisión
Permitir una **jerarquía flexible**: en *Start*, el campo **based on:** deja que una `feature/*` derive de `develop` **o de otra `feature/*`** por encima de ella. Para que la jerarquía sea visible en el árbol (dos branches podrían quedar en el mismo commit), el Start con *based on* también ejecuta un `git commit --allow-empty -m "chore: start <ref>"`.

## Consecuencia operativa
El *finish feature* necesita **encadenarse en cascada**: reaplicar *finish feature* sucesivamente desde el nodo hijo hasta el padre, subiendo la cadena hasta llegar a `develop`.

## Bugfix — regla relacionada
Un **bugfix** solo existe **vinculado a una release** (`DoStart` lo bloquea sin release); la base registra un *based-on override* y el bugfix queda anidado bajo la release en el árbol.

## 🔗 Relacionado
- [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]]
- [[🌳 Árvore hierárquica vs lista plana (ES)|Árbol jerárquico vs. lista plana]]
- [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]]
- [[🔀 GitFlowForm (ES)|GitFlowForm]]
