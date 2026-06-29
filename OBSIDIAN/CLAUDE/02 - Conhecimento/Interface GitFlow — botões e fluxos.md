---
tipo: conhecimento
criado: 2026-06-01
atualizado: 2026-06-11 (regra "based on" dirigida pelo Type no Start — combo filtrado + checkbox habilitado/desabilitado por tipo)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, gitflow]
fonte: src\GitExtensions.ZimerfeldTree\GitFlowForm.cs
---

# Interface GitFlow — botões e fluxos

> [!abstract] Resumo
> Janela **modal** (`GitFlowForm`) que dirige operações git flow usando **git puro** (sem depender do binário `git-flow`): iniciar feature/release/hotfix, publicar, rastrear, atualizar e finalizar. A saída dos comandos aparece numa caixa de texto com scroll automático. Aberta pelo botão/menu **GitFlow** da [[Interface ZimerfeldTree — botões e fluxos]]. Projeto: [[GitExtensions.ZimerfeldTree]].

![[ScreenShots/ScreenshotGitFlow.png]]

## 🌳 Regras de Start e Finish por tipo

Diagrama-resumo: para cada tipo, a **base do Start**, o **branch criado** e o **destino do merge no Finish**.

![[ScreenShots/ScreenShotStartFinish.png]]

| Tipo | Start — base | Finish — destino |
| --- | --- | --- |
| **feature** | `develop` ou `feature/*` (opcional) | `develop` ou pai based-on (`merge --no-ff`) |
| **bugfix** | `release/*` (obrigatório) | a própria **release (pai)** — ou `develop` se a release não existir (`merge --no-ff`) |
| **release** | `develop` (fixo) | `main` (`merge --no-ff` + tag) + `develop`; push de main/develop/tag |
| **hotfix** | `main` (fixo) | `main` (`merge --no-ff` + tag) + `develop` |
| **support** | tag de produção (obrigatório) | só `main` (`merge --no-ff`, sem tag, sem develop) |

> Comum a todo Finish: fetch opcional · apaga branch local e remoto (exceto **Keep**) · religa os filhos na árvore. Detalhes completos em [[#Botão Finish (`_btnFinish`) → `DoFinish` ⚠️ fluxo composto]].

### Fluxo completo de comandos por tipo

Sequência de comandos `git` de cada tipo, do Start ao Finish (com remote, sem No fetch):

![[ScreenShots/ScreenShotFlowPerType.png]]

### Posicionamento do nó na árvore (based-on)

O git guarda só o commit-tip de cada branch, não a origem. O Start aninha o novo nó por um destes mecanismos:

![[ScreenShots/ScreenShotHierarchyBasedOn.png]]

- **commit vazio** (base develop/main + based-on): `git commit --allow-empty` → tip diverge → ancestralidade real
- **based-on override** (base `feature/*` custom + based-on): grava `.git/zimerfeld-basedon.json` (link visual, história limpa)
- **sem based-on**: `checkout -b` simples, aninha pela regra GitFlow + prefixo
- Finish → `RebaseBasedOnOnFinish` remove o link e re-aponta os filhos para o destino. Ver código em `BranchHierarchyService.cs` (`SaveBasedOnOverride`, `ApplyBasedOnOverrides`, `BreakCycles`).

## 🧭 Layout
- **Header** — `HEAD: <ref simbólico>` + link **"About GitFlow"**.
- **Start branch** (grupo) — `Type` (combo), `Expected name` (label de prefixo + caixa de texto) + botão **Start**, checkbox **based on:** + combo de base. O conteúdo do combo e o estado do checkbox são **dirigidos pelo Type** (ver [[#Regra "based on" por tipo (Start)]]).
- **Manage existing branches** (grupo) — `Type` (combo), `Branch` (combo de branches locais com o prefixo), botões **Publish / Track / Update / Finish**, checkboxes **Keep branch after finish** (`-k`, marcado por padrão) e **No fetch (--no-fetch)**.
- **Resultado dos comandos git** — caixa multilinha somente-leitura (fonte Consolas); limpa ao iniciar cada ação e faz scroll automático para o fim conforme os subcomandos são executados.
- **Fechar**.

Tipos suportados (`GitFlowTypes`): `feature`, `bugfix`, `release`, `hotfix`, `support`. O prefixo de cada tipo vem de `git config gitflow.prefix.<tipo>` (fallback `tipo/`).

## 🚀 Ao abrir (`Load` → `InitData` + `ApplySettings`)
1. Preenche `HEAD:` (`git rev-parse --symbolic-full-name HEAD`).
2. Combo "based on": preenchido por `ApplyStartTypeRule()` conforme o `Type` inicial (`feature` → `develop` + `feature/*`). Ver [[#Regra "based on" por tipo (Start)]].
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
3. Chama `ApplyStartTypeRule()` — ver abaixo.

### Regra "based on" por tipo (Start)
`ApplyStartTypeRule()` (disparado pelo `SelectedIndexChanged` do `cboStartType` e re-aplicado após um Start bem-sucedido) repopula `cboBasedOn` e define o estado de `chkBasedOn` conforme o tipo:

| `cboStartType` | `cboBasedOn` | `chkBasedOn` |
| --- | --- | --- |
| **hotfix**  | `main` (base fixa)                                | **desabilitado** |
| **release** | `develop` (base fixa)                             | **desabilitado** |
| **feature** | `develop` (1º item) + branches `feature/*` locais | **habilitado** |
| **bugfix**  | apenas branches `release/*` locais                | **marcado + habilitado (obrigatório)** |
| *outros (support)* | `develop` + todas as branches locais       | **habilitado** |

- O combo só fica **utilizável** quando o checkbox está **habilitado E marcado** (`_cboBasedOn.Enabled = _chkBasedOn.Enabled && _chkBasedOn.Checked`).
- hotfix/release: o checkbox é desmarcado e desabilitado → base fixa; o combo só exibe `main`/`develop`. O fallback de `DoStart` (sem "based on") já resolve para o mesmo `main`/`develop`, mantendo coerência.
- **bugfix (regra do projeto)**: um bugfix **só pode existir vinculado a uma release**. O checkbox já vem **marcado** e o combo lista só as `release/*`; o `DoStart` **bloqueia** o Start se não houver nenhuma release ou se a base escolhida não for uma `release/*`. A base release (não-raiz) faz o `DoStart` gravar um **based-on override** → o bugfix fica **aninhado sob a release** na árvore.
- feature: marque o checkbox para escolher a base no combo filtrado (feature filha de feature).
- Nomes de branch no combo são **completos** (ex.: `feature/x`, `release/2026`).

### Botão Start (`_btnStart`) → `DoStart`
1. Lê tipo e nome; se nome vazio → `MessageBox` e aborta.
1b. **bugfix**: se não houver nenhuma `release/*` → `MessageBox` (`bugfixNeedsRelease`) e aborta; se o checkbox não estiver marcado ou a base não for uma `release/*` existente → `MessageBox` (`bugfixSelectRelease`) e aborta.
2. Limpa `_txtResult`.
3. `git checkout -b <prefixo><nome> <base>` (base padrão: develop para feature/release; main para hotfix/support; **release para bugfix (obrigatório)**; ou a branch escolhida em "based on").
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
5. `git merge <remote>/<pai>` (ou `<pai>` local se No fetch). Pai = develop para feature/release; main para hotfix/support; **a release (pai)** para bugfix — mesclada sempre do ref local da release (releases costumam ser locais), com fallback para develop se nenhuma release for resolvida.
6. Sucesso → reveal.

### Botão Finish (`_btnFinish`) → `DoFinish` ⚠️ fluxo composto
1. Lê tipo+nome (aborta se vazio).
2. Limpa `_txtResult`.
3. Se No fetch desmarcado e remoto existir: `git fetch <remote>`.
4. **Merge sequence** (git puro, sem binário git-flow):
   - feature: `checkout <develop ou pai based-on>` → `merge --no-ff`.
   - bugfix: `checkout <release (pai based-on), ou develop se a release não existir>` → `merge --no-ff` (o pai based-on é a release escolhida no Start; só não é usado se tiver sido finalizada/apagada).
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
- **Show Debug** (`chkShowDebug`): persiste/recarrega o próprio estado **individualmente** (chave `showDebug` no mesmo `ZimerfeldTree.gitflowsettings.json`). Na primeira abertura (sem valor salvo) usa o estado herdado do owner (`showControlIds`).
- **Idioma** (`cboLanguage`): **por janela** (campo `_lang`), persistido individualmente (chave `language` no `ZimerfeldTree.gitflowsettings.json`), aplicado no `Load` via `ApplyLanguage()` com `I18n.Load(scope, _lang)`. **Não** chama mais `I18n.SetLanguage` global. Em `ApplySettings`, `_lang` é definido **antes** dos checkboxes (cujo `CheckedChanged` chama `SaveSettings`) para não sobrescrever o idioma salvo. Primeira abertura sem valor herda `I18n.Current`.

### Link "About GitFlow"
- Abre `MessageBox` descrevendo os comandos git executados por cada botão.

### Botão Fechar (`_btnClose`)
- `Close()` (também é o `CancelButton`). Não há `FormClosing` (os checkboxes já são salvos incrementalmente a cada `CheckedChanged`).
- No owner, após o modal fechar: recentraliza a ZimerfeldTree e **só** chama `RefreshTree()` se houve **release finish** (focar a tag). Caso contrário não refresca (o `RepoMutated` já atualizou ao vivo) e **não** chama `NotifyRepoChanged()`.

## ⚠️ Erros comuns (`ShowFlowError`)
Quando a saída contém "does not exist" / "not found" / "unknown revision" / "pathspec", a mensagem orienta a checar `git branch --list main master develop` e `git config gitflow.branch.*`, criar a branch faltante ou usar **GitFlow Initialize**.

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[git flow - chaves de config (CLI)]]
- [[Plugin MEF para GitExtensions]]
