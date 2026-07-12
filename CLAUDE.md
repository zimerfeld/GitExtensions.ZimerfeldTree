# CLAUDE.md

Guia para o Claude Code neste repositório.

## Fluxo de publicação — NÃO criar nem aprovar Pull Requests

- **Não peça para criar Pull Requests e não crie Pull Requests.**
- **Não peça para aprovar Pull Requests e não aprove Pull Requests.**

Motivo: o processo de publicação estabelecido é baseado em **GitFlow com GitHub
Actions**. A publicação em produção acontece a partir da branch **`main`**,
disparada automaticamente pelo GitHub (Actions) ou manualmente via **`wrangler`**
pelo terminal. Pull Requests não fazem parte desse fluxo de release.

O que fazer em vez disso:

- Desenvolva na branch designada, faça commits claros e **push** para a branch.
- Deixe a integração para `main` e a publicação a cargo do fluxo GitFlow/Actions
  já estabelecido — não abra, não solicite e não aprove PRs por conta própria.
- Se uma mudança precisar chegar à produção, informe que o push foi feito e que a
  publicação segue pelo processo GitFlow/Actions (GitHub) ou `wrangler` (terminal).
