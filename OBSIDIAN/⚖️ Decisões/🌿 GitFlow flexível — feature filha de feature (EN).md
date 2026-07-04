---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, hierarquia, feature]
status: aceita
criado: 2026-06-18
---

# ADR — Flexible GitFlow: feature child of a feature

## Context
**Classic GitFlow** does not foresee a feature being the child of a feature — all `feature/*` derive from `develop` and are siblings. But in real development it is common to chain stages: a large feature whose work branches out into dependent sub-features that only make sense on top of the parent feature.

## Decision
Allow a **flexible hierarchy**: in *Start*, the **based on:** field lets a `feature/*` derive from `develop` **or from another `feature/*`** above it. To make the hierarchy visible in the tree (two branches could sit on the same commit), Start with *based on* also runs a `git commit --allow-empty -m "chore: start <ref>"`.

## Operational consequence
*finish feature* must **cascade**: reapply *finish feature* successively from the child node up to the parent, climbing the chain until it reaches `develop`.

## Bugfix — related rule
A **bugfix** only exists **bound to a release** (`DoStart` blocks without a release); the base records a *based-on override* and the bugfix stays nested under the release in the tree.

## 🔗 Related
- [[🔀 GitFlow em git puro (EN)|GitFlow in pure git]]
- [[🌳 Árvore hierárquica vs lista plana (EN)|Hierarchical tree vs. flat list]]
- [[🌿 Hierarquia de branches — branches no mesmo commit (EN)|Branch hierarchy — branches on the same commit]]
- [[🔀 GitFlowForm (EN)|GitFlowForm]]
