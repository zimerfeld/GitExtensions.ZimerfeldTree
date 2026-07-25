---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, hierarquia, feature]
status: aceita
criado: 2026-06-18
---

# ADR — GitFlow flexível: feature filha de feature

## Contexto
O **GitFlow clássico** não prevê feature filha de feature — todas as `feature/*` derivam de `develop` e são irmãs. Mas no desenvolvimento real é comum encadear estágios: uma feature grande cujo trabalho se ramifica em sub-features dependentes que só fazem sentido sobre a feature pai.

## Decisão
Permitir uma **hierarquia flexível**: no *Start*, o campo **based on:** deixa uma `feature/*` derivar de `develop` **ou de outra `feature/*`** acima dela. Para tornar a hierarquia visível na árvore (duas branches poderiam ficar no mesmo commit), o Start com *based on* executa também um `git commit --allow-empty -m "chore: start <ref>"`.

## Consequência operacional
O *finish feature* precisa **cascatear**: reaplicar *finish feature* sucessivamente do nó filho até o pai, subindo a cadeia até chegar em `develop`.

## Bugfix — regra relacionada
Um **bugfix** só existe **vinculado a uma release** (o `DoStart` bloqueia sem release); a base grava um *based-on override* e o bugfix fica aninhado sob a release na árvore.

## 🔗 Relacionado
- [[🔀 GitFlow em git puro (PT)|🔀 GitFlow em git puro]]
- [[🌳 Árvore hierárquica vs lista plana (PT)|🌳 Árvore hierárquica vs lista plana]]
- [[🌿 Hierarquia de branches — branches no mesmo commit (PT)|🌿 Hierarquia de branches — branches no mesmo commit]]
- [[🔀 GitFlowForm (PT)|🔀 GitFlowForm]]
