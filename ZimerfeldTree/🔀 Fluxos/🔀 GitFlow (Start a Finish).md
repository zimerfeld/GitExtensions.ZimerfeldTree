---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [fluxo, gitflow, start, finish, etapa2]
---

# Fluxo: 2 — GitFlow (Start → Finish)

Janela `GitFlowForm`, aberta pelo botão **GitFlow** ou pelo menu de contexto. Dirige o ciclo em **git puro** (sem o binário `git-flow`).

![[ScreenshotGitFlow.png]]

## Passos

```
1. [GitFlow]  →  escolher tipo (feature / bugfix / release / hotfix / support)
        │  nome (release/hotfix pré-preenchem yyyyMMddHHmm)
        │  opcional: based on → derivar de outra feature (feature filha de feature)
        ▼
2. [Start]    →  git checkout -b <prefixo><nome> <base>
        │  feature/release ← develop · hotfix/support ← main · bugfix ← release/* (obrigatório)
        │  based on → git commit --allow-empty (torna a hierarquia visível)
        ▼
3. [Publish]  →  git push --set-upstream <remote> <prefixo><nome>
   [Track]    →  git fetch + git checkout -b … --track <remote>/<ref>
   [Update]   →  git fetch + checkout + git merge <remote>/<pai>
        ▼
4. [Finish]   →  merge --no-ff no destino + branch -d (+ push --delete se remota existir)
        │  feature/bugfix: destino develop / release pai
        │  hotfix/release: checkout main + merge + git tag + checkout develop + merge
        │  release: fluxo completo automático + push de main/develop/tag + expande TAGS
```

## Regras-chave

- **Bugfix** só existe **vinculado a uma release** — Start bloqueado sem release; fica aninhado sob ela.
- **Feature filha de feature** via *based on:* → *finish* **cascateia** até `develop`. Ver [[🌿 GitFlow flexível — feature filha de feature]].
- **Erro de merge (conflito):** o plugin para e mostra o resultado; resolver com `git merge --abort` ou resolver + `git commit`.

> Mapa completo Start→git puro (todos os tipos) em [[👁️ Visão Geral]].

## Relacionado

- [[🌲 Abrir e navegar a árvore]]
- [[🔀 GitFlowForm]]
- [[🔀 GitFlow em git puro]]
- [[🔀 Interface GitFlow — botões e fluxos]]
- [[⚙️ git flow - chaves de config (CLI)]]
