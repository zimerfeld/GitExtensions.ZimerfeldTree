---
tipo: conhecimento
criado: 2026-06-01
atualizado: 2026-06-01
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, gitflow]
fonte: src\GitExtensions.ZimerfeldTree\GitFlowForm.cs
---

# Interface GitFlow — botões e fluxos

> [!abstract] Resumo
> Janela **modal** (`GitFlowForm`) que dirige comandos `git flow` (git-flow-next): iniciar feature/release/hotfix, publicar, rastrear, atualizar e finalizar. A saída crua dos comandos aparece numa caixa de texto. Aberta pelo botão/menu **GitFlow** da [[Interface ZimerfeldTree — botões e fluxos]]. Requer **git-flow-next** instalado e as chaves de config corretas (ver [[git flow - chaves de config (CLI)]]). Projeto: [[GitExtensions.ZimerfeldTree]].

## 🧭 Layout
- **Header** — `HEAD: <ref simbólico>` + link **"About GitFlow"**.
- **Start branch** (grupo) — `Type` (combo), `Expected name` (label de prefixo + caixa de texto) + botão **Start**, checkbox **based on:** + combo de base (default `develop`).
- **Manage existing branches** (grupo) — `Type` (combo), `Branch` (combo de branches locais com o prefixo), botões **Publish / Track / Update / Finish**, checkboxes **Keep branch after finish** (`-k`, marcado por padrão) e **No fetch (--no-fetch)**.
- **Result of git flow command run** — caixa multilinha somente-leitura (fonte Consolas) com a saída.
- **Fechar**.

Tipos suportados (`GitFlowTypes`): `feature`, `bugfix`, `release`, `hotfix`, `support`. O prefixo de cada tipo vem de `git config gitflow.prefix.<tipo>` (fallback `tipo/`).

## 🚀 Ao abrir (`Load` → `InitData` + `ApplySettings`)
1. Preenche `HEAD:` (`git rev-parse --symbolic-full-name HEAD`).
2. Combo "based on": `develop` + todas as branches locais.
3. **Detecta o tipo git-flow da branch atual** e abre o painel Manage já apontando para ela.
4. `Type` (Start) começa em `feature`; `Type` (Manage) na branch detectada.
5. Carrega checkboxes salvos de `%APPDATA%\GitExtensions\ZimerfeldTree.gitflowsettings.json`.

## 🔁 `RunFlow(args)` — o executor comum
Toda ação passa por aqui:
1. Cursor de espera; roda `git <args>` (`RunGitFlow` → stdout+stderr combinados + exit code).
2. Escreve `command - git <args>` + saída na caixa de Result (com `append` opcional para fluxos de várias etapas).
3. Atualiza o label `HEAD:` e **recarrega o combo de branches** do Manage (uma branch deletada por finish some daqui).
4. Se exit code ≠ 0 e não for `suppressError` → `MessageBox` de erro (`ShowFlowError`).
5. Retorna `true` se exit code == 0.

`RevealInTree(branch, checkout)`: opcionalmente faz `git checkout "<branch>"`, dispara `RepoMutated` (a ZimerfeldTree atrás atualiza e revela/seleciona a branch) e reativa o modal.

---

## 🖱️ Botões e ações — passo a passo

### Combo Type — Start
1. `SelectedIndexChanged`: atualiza o label de prefixo (`git config gitflow.prefix.<tipo>`).
2. Se o tipo for **release** e o nome estiver vazio → preenche automaticamente com a convenção **`yyyyMMddHHmm`** (ex.: `202606011230`). Não sobrescreve entrada manual.

### Botão Start (`_btnStart`) → `DoStart`
1. Lê tipo e nome; se nome vazio → `MessageBox` e aborta.
2. Se **based on** marcado → acrescenta `"<base>"` ao comando.
3. Executa `git flow <tipo> start "<nome>" ["<base>"]`.
4. Limpa a caixa de nome.
5. Em caso de sucesso: pré-seleciona a nova branch no painel **Manage**, e **faz checkout** dela + revela na ZimerfeldTree (`RevealInTree(prefixo+nome, checkout:true)`).
6. Falha → reativa o modal.

### Botão Publish (`_btnPublish`) → `DoPublish`
1. Lê tipo+nome (aborta se vazio).
2. `git flow <tipo> publish "<nome>"` (envia a branch ao remoto).
3. Sucesso → `RevealInTree(prefixo+nome, checkout:true)`.

### Botão Track (`_btnTrack`) → `DoTrack`
1. Lê tipo+nome.
2. `git flow <tipo> track "<nome>"` (cria branch local rastreando a remota de mesmo nome).
3. Sucesso → reveal + checkout.

### Botão Update (`_btnUpdate`) → `DoUpdate`
1. Lê tipo+nome.
2. `git flow <tipo> update "<nome>"` (traz commits da branch pai, ex.: develop, para a atual).
3. Sucesso → reveal + checkout.

### Botão Finish (`_btnFinish`) → `DoFinish` ⚠️ fluxo composto
1. Lê tipo+nome (aborta se vazio). Detecta `isRelease` / `isHotfix`.
2. Monta **flags**: `-k` se "Keep branch"; `--no-fetch` se marcado; para **release/hotfix** acrescenta `-m "<nome>"` (mensagem da tag anotada — sem isso o git-flow abriria editor e abortaria com "no tag message?").
3. Se **não** for no-fetch → `git fetch` antes (evita divergências).
4. Se **release** e não no-fetch → faz **push da branch para o remoto primeiro** (`git push <remote> <prefixo+nome>`), senão o finish não acha o ref remoto. Falha aqui aborta.
5. Executa `git flow <tipo> finish <flags> "<nome>"`.
6. **Se falhar com "merge is already in progress"** → `ResolveInProgressMerge`:
   - extrai a branch travada do texto de erro;
   - `git flow <tipo> finish --abort "<nome>"` e `git merge --abort`;
   - **deadlock do git-flow-next:** se persistir, apaga o estado órfão `.git/gitflow/state/*.json` (`ClearGitFlowState`);
   - volta para a branch original e **repete o finish**.
   - Outras falhas → `ShowFlowError` (com dica sobre branch base/produção ausente).
7. Sucesso, **não-release** (feature/bugfix/hotfix/support): o finish já mesclou e deletou a branch, deixando você na base → `RevealInTree(branch atual, checkout:false)`. Fim.
8. Sucesso, **release** — sequência de pós-finish:
   1. Guarda `LastFinishedReleaseTag = nome` (a ZimerfeldTree foca a tag ao fechar).
   2. Resolve nomes reais de master/develop (`git config gitflow.branch.*`) e o remoto default.
   3. Sem remoto configurado → `MessageBox` "finalizada localmente" e para.
   4. `git push <remote> <master>` → `git push <remote> <develop>` (cada um aborta se falhar).
   5. `git push <remote> refs/tags/<nome>` (envia a tag; git flow a cria só local).
   6. **Limpa a branch release remota** só se ainda existir (`git ls-remote --heads ...`): `git push <remote> --delete <prefixo+nome>`; senão escreve uma nota "(pulado: já não existe)".
   7. `git checkout <develop>` + `RevealInTree(develop, checkout:false)`.

### Checkboxes do Finish
- **Keep branch after finish** (`-k`) e **No fetch (--no-fetch)**: ao mudar, salvam em `ZimerfeldTree.gitflowsettings.json`.

### Link "About GitFlow"
- Abre `MessageBox` explicando Publish/Track/Update/Finish e os checkboxes; lembra que requer **git-flow-next**.

### Botão Fechar (`_btnClose`)
- `Close()` (também é o `CancelButton`).

## ⚠️ Erros comuns (`ShowFlowError`)
Quando a saída contém "couldn't find remote ref" / "does not exist" / "start point branch", a mensagem orienta a checar `git branch --list main master develop` e `git config gitflow.branch.*`, criar a branch faltante ou marcar **No fetch**. Ver [[git flow - chaves de config (CLI)]].

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[git flow - chaves de config (CLI)]]
- [[Plugin MEF para GitExtensions]]
