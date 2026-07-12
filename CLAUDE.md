# CLAUDE.md

Guia para o Claude Code neste repositório.

## Idioma das respostas — sempre Português (BR)

- **Responda no chat sempre em Português do Brasil (pt-BR).**
- Vale para toda a comunicação no chat (explicações, resumos, perguntas,
  relatórios de status), independentemente do idioma em que a mensagem do
  usuário for escrita.
- Isto se aplica apenas às respostas do chat. Código, nomes de arquivos,
  mensagens de commit, conteúdo de documentos e demais artefatos seguem a
  convenção de idioma própria de cada um (por exemplo, os documentos de
  localização podem estar em en-US, pt-BR ou es-ES).

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

## Rito pós-publicação — sincronizar develop e abrir a próxima feature

Sempre que publicar a `main` (após a integração/release):

1. **Sincronize a `develop` com a `main`** para que as duas não divirjam — faça o
   *back-merge* de `main` em `develop` e **push** da `develop`. A `develop` deve
   sempre conter tudo o que já está na `main`.
2. **Crie uma nova feature a partir da `develop`** (`feature/<nome>`), com um
   **nome sugestivo** para a próxima demanda, e passe a desenvolver nela.

Assim o ciclo GitFlow fecha: produção sai da `main`, a `develop` fica alinhada e
já existe uma branch de trabalho pronta para a próxima entrega.
