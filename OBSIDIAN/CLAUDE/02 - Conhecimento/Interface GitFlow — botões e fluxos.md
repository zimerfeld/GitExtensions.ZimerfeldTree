---
tipo: conhecimento
criado: 2026-06-01
atualizado: 2026-06-05 (git-flow-next removido; scroll no txtResult; remoção remota em todos os Finish)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, gitflow]
fonte: src\GitExtensions.ZimerfeldTree\GitFlowForm.cs
---

# Interface GitFlow — botões e fluxos

> [!abstract] Resumo
> Janela **modal** (`GitFlowForm`) que dirige operações git flow usando **git puro** (sem depender do binário `git-flow`): iniciar feature/release/hotfix, publicar, rastrear, atualizar e finalizar. A saída dos comandos aparece numa caixa de texto com scroll automático. Aberta pelo botão/menu **GitFlow** da [[Interface ZimerfeldTree — botões e fluxos]]. Projeto: [[GitExtensions.ZimerfeldTree]].

![[ScreenshotGitFlow.png]]

## 🧭 Layout
- **Header** — `HEAD: <ref simbólico>` + link **"About GitFlow"**.
- **Start branch** (grupo) — `Type` (combo), `Expected name` (label de prefixo + caixa de texto) + botão **Start**, checkbox **based on:** + combo de base (default `develop`).
- **Manage existing branches** (grupo) — `Type` (combo), `Branch` (combo de branches locais com o prefixo), botões **Publish / Track / Update / Finish**, checkboxes **Keep branch after finish** (`-k`, marcado por padrão) e **No fetch (--no-fetch)**.
- **Resultado dos comandos git** — caixa multilinha somente-leitura (fonte Consolas); limpa ao iniciar cada ação e faz scroll automático para o fim conforme os subcomandos são executados.
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
2. Appenda `command - git <args>` + saída na caixa de Result via `AppendText` (scroll automático para o fim). Cada botão chama `_txtResult.Clear()` antes do primeiro `RunFlow`, limpando o resultado anterior.
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
2. Limpa `_txtResult`.
3. `git checkout -b <prefixo><nome> <base>` (base padrão: develop para feature/bugfix/release; main para hotfix/support; ou a branch escolhida em "based on").
4. Limpa a caixa de nome.
5. Sucesso: pré-seleciona a nova branch no painel **Manage** e revela na ZimerfeldTree (`RevealInTree(prefixo+nome, checkout:false)` — o checkout já foi feito pelo `-b`).
6. Falha → reativa o modal.

### Botão Publish (`_btnPublish`) → `DoPublish`
1. Lê tipo+nome (aborta se vazio); aborta se sem remoto configurado.
2. Limpa `_txtResult`.
3. `git push --set-upstream <remote> <prefixo+nome>`.
4. Sucesso → `RevealInTree(prefixo+nome, checkout:false)`.

### Botão Track (`_btnTrack`) → `DoTrack`
1. Lê tipo+nome; aborta se sem remoto.
2. Limpa `_txtResult`.
3. Se No fetch desmarcado: `git fetch <remote>`.
4. `git checkout -b <prefixo+nome> --track <remote>/<prefixo+nome>`.
5. Sucesso → reveal.

### Botão Update (`_btnUpdate`) → `DoUpdate`
1. Lê tipo+nome.
2. Limpa `_txtResult`.
3. Se No fetch desmarcado e remoto existir: `git fetch <remote>`.
4. `git checkout <prefixo+nome>`.
5. `git merge <remote>/<pai>` (ou `<pai>` local se No fetch). Pai = develop para feature/bugfix/release; main para hotfix/support.
6. Sucesso → reveal.

### Botão Finish (`_btnFinish`) → `DoFinish` ⚠️ fluxo composto
1. Lê tipo+nome (aborta se vazio).
2. Limpa `_txtResult`.
3. Se No fetch desmarcado e remoto existir: `git fetch <remote>`.
4. **Merge sequence** (git puro, sem binário git-flow):
   - feature/bugfix: `checkout develop` → `merge --no-ff`.
   - hotfix/release: `checkout main` → `merge --no-ff` → `tag -a <nome> -m <nome>` → `checkout develop` → `merge --no-ff`.
   - support: `checkout main` → `merge --no-ff`.
5. Se **Keep** desmarcado: `git branch -d <prefixo+nome>`.
6. **Remoção remota** (todos os tipos): `git ls-remote --heads <remote> <branch>` → se existir, `git push <remote> --delete <branch>`; senão appenda nota "(pulado: já não existe)".
7. Pós-finish para **release** (adicional):
   a. `LastFinishedReleaseTag = nome` (ZimerfeldTree foca a tag ao fechar).
   b. Sem remoto → aviso "finalizada localmente" e para.
   c. `git push <remote> <main>` → `git push <remote> <develop>`.
   d. `git push <remote> refs/tags/<nome>`.
   e. Remoção remota da branch release (passo 6 já executado antes do passo 7).
   f. `git checkout <develop>` + reveal.
8. Não-release (feature/bugfix/hotfix/support): `RevealInTree(branch atual, checkout:false)`.

> Erros de merge param o fluxo e exibem o resultado no painel. Resolver manualmente (`git merge --abort` ou commit).

### Checkboxes do Finish
- **Keep branch after finish** e **No fetch (--no-fetch)**: ao mudar, salvam em `ZimerfeldTree.gitflowsettings.json`.

### Link "About GitFlow"
- Abre `MessageBox` descrevendo os comandos git executados por cada botão.

### Botão Fechar (`_btnClose`)
- `Close()` (também é o `CancelButton`).

## ⚠️ Erros comuns (`ShowFlowError`)
Quando a saída contém "does not exist" / "not found" / "unknown revision" / "pathspec", a mensagem orienta a checar `git branch --list main master develop` e `git config gitflow.branch.*`, criar a branch faltante ou usar **GitFlow Initialize**.

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[git flow - chaves de config (CLI)]]
- [[Plugin MEF para GitExtensions]]
