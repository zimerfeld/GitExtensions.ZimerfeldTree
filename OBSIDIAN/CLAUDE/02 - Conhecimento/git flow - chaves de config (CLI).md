---
tipo: conhecimento
criado: 2026-06-01
tags: [conhecimento, git, gitflow]
---

# git flow — chaves de config (CLI)

## Problema
O GitExtensions grava configurações de gitflow no **formato interno dele** (`gitflow.branch.develop.type=base`, etc.), mas o plugin **Plugins → Gitflow** usa o **git flow CLI**, que espera chaves diferentes. Como as chaves esperadas não existem, o plugin acha que o gitflow nunca foi inicializado e continua mostrando **"Init Gitflow"**.

## Solução — adicionar as chaves no formato padrão
```bash
git config gitflow.branch.master main
git config gitflow.branch.develop develop
git config gitflow.prefix.feature feature/
git config gitflow.prefix.bugfix bugfix/
git config gitflow.prefix.release release/
git config gitflow.prefix.hotfix hotfix/
git config gitflow.prefix.support support/
git config gitflow.prefix.versiontag ""
```

> Fonte: `GitFlowFix.txt` na raiz do repositório [[GitExtensions.ZimerfeldTree]].

## 🔗 Relacionado
- [[GitExtensions.ZimerfeldTree]]
