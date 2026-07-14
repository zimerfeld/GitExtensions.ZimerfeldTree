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

## Paridade de idiomas — PT, EN e ES sempre juntos

- **Ao criar ou alterar qualquer conteúdo traduzível, faça-o nos três idiomas:
  Português (pt-BR), Inglês (en-US) e Espanhol (es-ES).** Nunca deixe um idioma
  para trás.
- Isto vale para todo o material que já segue a convenção multi-idioma:
  - **Dicionários de UI** em `src/GitExtensions.ZimerfeldTree/Resources/`
    (`*.pt-BR.json`, `*.en-US.json`, `*.es-ES.json`) — mantenha **as mesmas
    chaves** nos três arquivos (paridade de chaves), traduzindo o valor.
  - **READMEs** (`README.pt-BR.md`, `README.en-US.md`, `README.es-ES.md`) e os
    blocos por idioma do `README.md`.
  - **Cofre OBSIDIAN** (`ZimerfeldTree/`) — cada nota existe como `<Nome>.md`
    (pt-BR), `<Nome> (EN).md` e `<Nome> (ES).md`, com `lang:` correto no
    front-matter e os links internos apontando para a versão do mesmo idioma.
- Se adicionar uma nova chave/nota/seção em um idioma, adicione **imediatamente**
  a correspondente nos outros dois, para que os três permaneçam em paridade.

## Combo de idioma — sempre AUTO/PT/EN/ES, com AUTO por padrão

- O seletor de idioma (combo) de **todas** as janelas deve exibir **sempre** as
  quatro opções, **nesta ordem**: **AUTO** (automático, segue a cultura do SO),
  **PT** (Português), **EN** (Inglês) e **ES** (Espanhol).
- **AUTO é a opção pré-selecionada por padrão** — é o valor inicial quando não há
  preferência salva. A persistência por janela continua valendo: se o usuário
  escolher um idioma, aquela janela reabre no idioma escolhido; sem escolha,
  volta a AUTO.
- **Formato dos rótulos** (chaves `lang*`): ícone + nome do idioma + código do país
  entre parênteses, com o **nome traduzido em cada dicionário** e o **ícone/código
  universais**. Padrão de referência (pt-BR):
  **🌐 Automático**, **🇧🇷 Português (BR)**, **🇺🇸 Inglês (US)**, **🇪🇸 Espanhol (ES)**.
  Em en-US: 🌐 Automatic / 🇧🇷 Portuguese (BR) / 🇺🇸 English (US) / 🇪🇸 Spanish (ES);
  em es-ES: 🌐 Automático / 🇧🇷 Portugués (BR) / 🇺🇸 Inglés (US) / 🇪🇸 Español (ES).
- Ao acrescentar um novo idioma, mantenha a ordem AUTO → PT → EN → ES → (novos) e
  garanta a paridade da ordem entre o `enum AppLanguage`, o preenchimento do combo
  (`PopulateLanguageCombo`) e o mapeamento índice→idioma (`OnLanguageChanged`) nas
  três janelas, além das chaves `lang*` nos dicionários dos três idiomas.

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
