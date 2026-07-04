---
tipo: arquivo
tags: [arquivo, git, subprocesso, service, hierarquia, gitflow]
arquivo: src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs
atualizado: 2026-07-04
---

# BranchHierarchyService.cs

Executor de `git` como subprocesso + parser da saída + montagem da hierarquia de branches. ~831 linhas.

**Caminho:** `src/GitExtensions.ZimerfeldTree/BranchHierarchyService.cs`

---

## Papel

Toda a lógica git da janela principal e do GitFlow mora aqui. Roda `git` num `ProcessStartInfo` (stdout/stderr redirecionados, UTF-8, sem janela), parseia a saída e devolve modelos [[BranchNode|BranchInfo]].

## Coleta e hierarquia

- **Grafo de commits em memória:** um único `git log --all` constrói o grafo; pais resolvidos por **BFS** → **O(commits)** em vez de O(N²×subprocesso).
- LOCAL/REMOTES combinam **ancestralidade real** (parentesco por commits / GitFlow) **+ agrupamento por caminho** (`/`).
- `BuildGitFlowParentMap` usa a ancestralidade real para achar o pai GitFlow (ex.: a release pai de um bugfix).
- Detecta **violações** de hierarquia (ex.: `violLocalBugfix`) que acionam a auto-organização GitFlow.

## Comandos GitFlow (git puro)

Implementa as sequências de start / publish / track / update / finish para os cinco tipos. O mapa completo **Start→git** está em [[../Sistema/Visão Geral]] e [[GitFlow em git puro]].

## Limitações conhecidas

- **Duas branches no mesmo commit** não formam pai-filho (o BFS nunca encontra uma como pai da outra) → ambas viram raízes; solução automática = commit vazio no Start com *based on*. Ver [[Hierarquia de branches — branches no mesmo commit]].
- **Branch real não pode ser nó-pai** de outra (o ref seria arquivo **e** diretório).

## Relacionado

- [[BranchHierarchyForm]]
- [[GitFlowForm]] · [[RestoreForm]]
- [[BranchNode]]
- [[../Sistema/Arquitetura]]
