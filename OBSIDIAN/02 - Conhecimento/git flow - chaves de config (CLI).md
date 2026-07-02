---
tipo: conhecimento
criado: 2026-06-01
atualizado: 2026-06-02
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

---

## Manutenção — refs remotas obsoletas

### Problema
Branches deletados no servidor continuam aparecendo na árvore do Git Extensions sob `origin/feature/` porque o Git **não remove automaticamente** os ponteiros locais de rastreamento.

### Limpeza manual
```bash
git remote prune origin
```

### Regra permanente (configurada em 2026-06-02)
```bash
git config --global fetch.prune true
```
Com esta configuração, todo `git fetch` (inclusive F5 no Git Extensions) remove automaticamente as refs obsoletas.

> Ver sessão [[2026-06-02 - Pruning de refs remotas obsoletas]].

## 🔗 Relacionado
- [[GitExtensions.ZimerfeldTree]]
- [[2026-06-02 - Pruning de refs remotas obsoletas]]
