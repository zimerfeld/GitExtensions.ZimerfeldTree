---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, gitflow, modal]
arquivo: src/GitExtensions.ZimerfeldTree/GitFlowForm.cs
---

# GitFlowForm.cs

Janela modal que dirige os comandos `git flow` usando **apenas git nativo**. ~758 linhas. Título: `ZimerfeldTree - GitFlow`.

**Caminho:** `src/GitExtensions.ZimerfeldTree/GitFlowForm.cs`

---

## Papel

Não depende do binário `git-flow` instalado — cada botão dispara uma sequência de `git` puro. Cobre os cinco tipos: **feature, bugfix, release, hotfix, support**, com operações **start / publish / track / update / finish**. Ver [[🔀 GitFlow em git puro]] e [[🔀 Interface GitFlow — botões e fluxos]].

## Regras específicas

- **Bugfix** só pode existir **vinculado a uma release** — o `DoStart` bloqueia se não houver release; a base grava um *based-on override* e o bugfix fica aninhado sob a release.
- **based on:** permite **feature filha de feature**; nesse caso executa também `git commit --allow-empty -m "chore: start <ref>"` para a hierarquia ficar visível. Ver [[🌿 GitFlow flexível — feature filha de feature]].
- **Nome padrão** de release/hotfix pré-preenchido com `yyyyMMddHHmm`.
- **Finish release** roda o fluxo completo automático (merge em main + tag + merge em develop + push de tudo). Ver o mapa Start→git puro em [[👁️ Visão Geral]].

## Botão Initialize

Aplica de uma vez as chaves `gitflow.*` padrão — ver [[⚙️ git flow - chaves de config (CLI)]].

## Relacionado

- [[⚙️ BranchHierarchyService]]
- [[🔀 GitFlow em git puro]]
- [[🌿 GitFlow flexível — feature filha de feature]]
- [[🔀 Interface GitFlow — botões e fluxos]]
- [[🔀 GitFlow (Start a Finish)]]
