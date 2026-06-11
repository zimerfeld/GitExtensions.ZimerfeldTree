---
tipo: conhecimento
criado: 2026-06-01
atualizado: 2026-06-09 (1.0.254: checkboxes multi-seleção, botão Excluir, Modo Developer, persistência expande/recolhe no Shown)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, zimerfeldtree]
fonte: src\GitExtensions.ZimerfeldTree\BranchHierarchyForm.cs
---

# Interface ZimerfeldTree — botões e fluxos

> [!abstract] Resumo
> Janela **não-modal** (`BranchHierarchyForm`) que mostra LOCAL / REMOTES / TAGS em árvore hierárquica e fica aberta ao lado do GitExtensions. Este documento descreve **cada controle** e **o passo a passo exato** de cada ação. Para a janela `git flow` ver [[Interface GitFlow — botões e fluxos]]. Projeto: [[GitExtensions.ZimerfeldTree]].

![[ScreenshotBranchHierarchy.png]]

## 🚪 Como a janela abre
- Menu **Plugins → ZimerfeldTree** chama `ZimerfeldTreePlugin.Execute`.
- Form é **singleton** por sessão do GitExtensions: se já existe, só atualiza o working dir e traz à frente; senão cria novo.
- `Execute` retorna `false` → o GitExtensions **não** atualiza a própria UI (a janela gerencia o próprio estado).
- O plugin assina eventos do host (`Register`): `PostBrowseInitialize` → troca de repositório; `PostCheckoutBranch` / `PostCheckoutRevision` → refaz a árvore; `PostCommit` → refaz + foca; `PostRepositoryChanged` → `OnExternalChange`. Assim a árvore se mantém em sincronia automaticamente.
- `OnExternalChange` chama `NotifyExternalRepoChanged()`: refresca em mudanças externas **genuínas**, mas **ignora o eco** do nosso próprio `NotifyRepoChanged` (flag `_suppressEcho`) — evita refresh redundante / flash de overlay.
- O **overlay só aparece na primeira exibição** (`VisibleChanged` guarda `_initialLoadDone`): reativar a janela depois de fechar GitFlow/Restore **não** dispara overlay (a árvore já está atualizada).

## 🧭 Layout (de cima para baixo)
1. **Top panel** — label "Working Directory:", combo de repositórios (`_cboRepo`), label "Branch: \<atual\>".
2. **Filter panel** — caixa "Filtrar branches..." (`_txtFilter`) + botão **↺** (`_btnRefresh`).
3. **Warn panel** (oculto por padrão) — aviso de GitFlow + botão **Organizar como GitFlow / Restaurar hierarquia real** (`_btnGitFlow`).
4. **GitFlow button panel** — **Pull**, **Push**, **Commit**, **GitFlow**, **Restore** (só aparecem se houver branch atual).
5. **Árvore** (`TreeView`) — 3 seções fixas: **LOCAL (n)**, **REMOTES (n)**, **TAGS (n)**.
6. **Bottom panel** — botão **Fechar** (centralizado).
7. **Status strip** — `Local: n | Remoto: n | Tags: n`.
8. **Overlay de carregamento** — flutua sobre tudo durante o load.

## 🔄 Carregamento da árvore (`RefreshTreeAsync`)
Acionado por: **primeira** exibição da janela (`VisibleChanged`, só na carga inicial), botão ↺, menu "Atualizar", troca de repo, eventos de checkout/commit/repo-changed do host (eco próprio suprimido), e após cada mutação local.
Passos (com % no overlay):
1. `10%` branches locais — `git branch --format=%(refname:short)`
2. `30%` branches remotas — `git branch -r --format=%(refname:short)`
3. `50%` tags — `git tag --sort=-version:refname`
4. `65%` hierarquia local — `BuildParentMap` (1× `git log --all --format="%H %P"` + BFS)
5. `80%` hierarquia remota — `BuildRemoteParentMap`
6. `92%` sincronização — `git for-each-ref ...%(upstream:track)` → preenche ahead/behind
7. `96%` alterações pendentes — `git status --porcelain`, lido **no background** (off-UI-thread); o valor é **reaproveitado** por `UpdateCommitActionTexts(pending)` para o contador `Commit (n)`, sem um segundo `git status` na UI thread
8. `100%` Concluído.
- Chamadas concorrentes são **coalescidas** (`_isRefreshing`); um refresh em andamento é **cancelado** antes de iniciar outro.
- Erros viram `MessageBox`. Cancelar restaura a UI sem mexer na árvore existente.
- Branch atual aparece em **negrito + cor de destaque**, com indicadores de tracking: `(↓M↑N)` (↓ atrás / ↑ à frente) só quando há divergência.

## 🌲 Estrutura da árvore
- LOCAL e REMOTES combinam **dois eixos**: aninhamento vertical por **ancestralidade real de commits** (`parentMap`) + agrupamento horizontal por **`/` no nome** (ex.: `feature/teste` → pasta `feature` → folha `teste`).
- REMOTES é subdividido por remoto (`origin`, ...).
- Estado de expansão é **persistido por repositório** em `%APPDATA%\GitExtensions\ZimerfeldTree.treestate.json` (salvo com debounce de 500 ms; restaurado ao reabrir). Durante filtro tudo é expandido.

## ⚙️ Modo GitFlow forçado (auto-organização)
- `GetGitFlowViolations()` checa as regras esperadas: `master/main` raiz; `develop` filho de master; `feature/*` filho de develop (idem para cada remoto).
- Se houver violações **e o usuário ainda não escolheu manualmente**, a árvore **auto-organiza** no layout GitFlow (`BuildGitFlowParentMap` / `BuildGitFlowRemoteParentMap`: master=raiz, develop→master, feature/release→develop, hotfix→master).
- O painel de aviso mostra a contagem de violações ou "exibindo organização GitFlow".

---

## 🖱️ Botões e ações — passo a passo

### Combo de repositórios (`_cboRepo`)
1. `SelectedIndexChanged`: se mudou → define `WorkingDir`, **reabilita auto-organização** GitFlow, e `RefreshTree()`.
- Lista vem de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings` (histórico de repositórios) + o working dir atual.

### Caixa de filtro (`_txtFilter`)
1. `TextChanged` → `ApplyFilter`: reconstrói as 3 seções filtrando por **substring** (case-insensitive) no nome completo; expande tudo enquanto há filtro.

### Botão ↺ (`_btnRefresh`)
1. Chama `RefreshTree()` → recarrega tudo com overlay (ver acima).

### Botão "Organizar como GitFlow" / "Restaurar hierarquia real" (`_btnGitFlow`)
1. Marca `_gitFlowUserToggled = true` (escolha manual desliga a auto-organização).
2. Inverte `_gitFlowForced`.
3. `RefreshTree()` → árvore é reconstruída no layout escolhido.

### Botão Pull (`_btnPull`) → `DoPull`
1. Desabilita o botão.
2. Em background: `git pull --tags`.
3. Na UI: reabilita o botão, `RefreshTree()`, `NotifyRepoChanged()` (avisa o GitExtensions e devolve o foco à janela).
4. Se falhar e houver mensagem → `MessageBox` "Pull falhou".
- Rótulo mostra `Pull (↓M)` quando a branch atual está M commits atrás.

### Botão Push (`_btnPush`) → `DoPush`
1. **Preferencial:** abre o **diálogo nativo de Push do GitExtensions in-process** (`StartPushDialog`, `pushOnShow: true` — dispara o push automaticamente ao abrir).
   - Ao fechar: `RefreshTree()` + `NotifyRepoChanged()` — **sempre**, independentemente do valor de retorno (`pushCompleted` não é confiável com `pushOnShow`).
2. **Fallback** (sem `_openPushDialog`): lança `GitExtensions.exe push` como novo processo (fire-and-forget — sem refresh possível). Erro ao iniciar → `MessageBox`.
- Rótulo mostra `↑ Push (N)` quando a branch atual está N commits à frente do remoto.

### Botão Commit (`_btnCommitDedicated`) → `DoCommit`
1. **Preferencial:** abre a **janela de commit nativa do GitExtensions in-process** (`_openCommitDialog` → `IGitUICommands.StartCommitDialog`). Isso mantém os plugins de Commit Template visíveis (ex.: "Zimerfeld: Auto-resumo").
   - Retorno `true` (houve commit) → `RefreshTree()` + `NotifyRepoChanged()`.
   - Retorno `false` (fechou sem commitar) → nada.
   - Retorno `null` (indisponível) → cai no fallback.
2. **Fallback:** `OpenCommitWindow()` dispara um **novo processo** `GitExtensions.exe commit` (plugins não carregam nesse modo). Erro → `MessageBox`.
- Rótulo mostra `Commit (n)` com a contagem de mudanças pendentes (`git status --porcelain`) via `UpdateCommitActionTexts`. A contagem é recalculada: na construção, **após `LoadRepositories`** (já com o repo selecionado), ao abrir o menu de contexto, e em cada refresh — neste último **reaproveitando** o valor lido no background do `RefreshTreeAsync` (sem `git status` extra na UI thread).

### Botão GitFlow (`_btnGitFlowDedicated`) → `DoGitFlow`

![[ScreenshotGitFlow.png]]

1. Cria `GitFlowForm` (modal) e posiciona **lado a lado** com a ZimerfeldTree, ambas centralizadas (se a tela couber; senão centraliza sobre a janela).
2. Assina `RepoMutated`: a cada mutação dentro do GitFlow, agenda revelar a branch afetada e chama `RefreshTree()` **por trás do modal** (sem roubar foco).
3. `ShowDialog` (bloqueia).
4. Ao fechar: recentraliza a ZimerfeldTree; **só** chama `RefreshTree()` se houve **release finish** (para focar a nova tag) — caso contrário **não** refresca, pois o `RepoMutated` já atualizou ao vivo. **Não** chama `NotifyRepoChanged()` (não traz o GitExtensions minimizado para frente).
- Mesmo fluxo do item de menu "GitFlow…". Detalhes da janela em [[Interface GitFlow — botões e fluxos]].

### Botão Restore (`_btnRestore`) → `DoRestore`

![[ScreenshotRestore.png]]

> Renomeado de **Voltar Versão** (`_btnVoltar`) para **Restore** (`_btnRestore`).

1. Cria `RestoreForm` (modal) e posiciona **lado a lado** com a BranchHierarchy, ambas centralizadas — mesmo posicionamento da janela GitFlow.
2. Assina `RepoMutated`: após cada operação bem-sucedida, chama `RefreshTree()` **por trás do modal** (sem roubar foco).
3. `ShowDialog` (bloqueia).
4. Ao fechar: **não** refresca (o `RepoMutated` já atualizou ao vivo) e **não** chama `NotifyRepoChanged()`. A `RestoreForm` salva os campos via `FormClosing → SaveSettings`.

Três operações disponíveis na janela Restore:
- **Restaurar Arquivo** — `git checkout <hash> -- "<arquivo>"`: recupera um arquivo específico do estado de um commit e o coloca como staged
- **Cherry-Pick** — `git cherry-pick <hash>` ou range `<antigo>..<recente>`: aplica um ou mais commits sobre a branch atual
- **Reset Branch** — `git checkout <branch>` (se não for a atual) + `git reset --mixed|--soft|--hard <hash>` + retorno à branch original

Valores dos campos são persistidos em `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`. Ver [[Interface Restore — botões e fluxos]].

### Botão Excluir (`_btnExcluir`) → `DoDelete`
1. Texto dinâmico via `UpdateDeleteButtonText()`: `Excluir` (0 marcados) → `Excluir (N)`. Atualizado em `AfterCheck` e a cada rebuild.
2. `DoDelete`: alvos = checkboxes marcados se houver (`CheckedBranchNodes()`); senão o nó selecionado.
   - 2+ → `DoDeleteMultiple` (confirmação única listando itens, exclusão em lote, força em local não-mesclada).
   - 1 → fluxo individual. 0 → nada.
3. **Proteção:** main/master/develop são removidas dos alvos se `Modo Developer` desligado (`IsProtectedBranch`); se sobrar nada, exibe aviso "Branch protegida".

**Fluxo de exclusão em lote (passo a passo):**

1. Itens marcados — o botão mostra `Excluir (8)`:

![[ScreenshotBeforeDelete.png]]

2. Confirmação única listando os itens, com a opção **Excluir Remotamente?**:

![[ScreenshotConfirmDelete.png]]

3. Overlay de progresso durante a exclusão (lista de passos + botão **Abortar Operação**):

![[ScreenshotDuringDelete.png]]

4. Árvore reconstruída já sem os itens e com contadores atualizados:

![[ScreenshotAfterDelete.png]]

### Checkbox "Modo Developer" (`_chkDeveloperMode`)
1. Ao lado de `Show Debug` no rodapé. Estado persistido em `ZimerfeldTree.uisettings.json` (`developerMode`) via `SaveUiSettings()`, carregado por `LoadDeveloperMode()`.
2. `Tree_BeforeCheck` bloqueia **marcar** (não desmarcar) main/master/develop quando desligado.
3. Ao **desligar**, `UncheckProtectedBranches()` desmarca as protegidas que estavam marcadas.

### Botão Fechar (`_btnClose`)
1. `Close()`. Ao fechar, o estado de expansão da árvore é salvo em disco (`FormClosed → SaveTreeState`).

### Botão Cancelar (overlay, `_btnCancelRefresh`)
1. Desabilita-se, vira "Cancelando…" e cancela o `CancellationTokenSource` do refresh em curso.

---

## 🌳 Interações na árvore
- **Duplo-clique** em folha de branch → `DoCheckout`.
- **Enter** com branch selecionada → `DoCheckout`.
- **Clique-direito** → seleciona o nó sob o cursor e abre o menu de contexto.
- **Checkbox** em cada branch/tag (folha) para seleção múltipla. Seções/pastas têm o checkbox **oculto** (`ApplyCheckBoxVisibility` via `TVM_SETITEM`, após `EndUpdate`) e **bloqueado** (`Tree_BeforeCheck`). `_tree.CheckBoxes = true`.
- **Persistência expande/recolhe:** `AfterExpand`/`AfterCollapse` gravam o caminho estável (`GetNodeStablePath`) em `_treeStateByRepo` → debounce 500 ms + `FormClosed` → `treestate.json`. Restaurado em `RestoreTreeState`, reaplicado no **`Shown`** (handle nativo já existe; no construtor sem handle não cola).

## 📋 Menu de contexto (visibilidade depende do tipo do nó)
Definida em `CtxMenu_Opening`: `branch` = local|remote; `local`/`remote`/`tag` específicos. Separadores órfãos são ocultados.

| Item | Visível para | Ação (passo a passo) |
|------|--------------|----------------------|
| **Commit (n)** | sempre | Igual ao botão Commit → `DoCommit`. Mostra contagem de pendências. |
| **Checkout** | branch (local/remote) | `DoCheckout`: local → `git checkout "<nome>"`; remote → `CheckoutRemoteAsLocal` = `git checkout -b "<local>" --track "<origin/...>"`. Sucesso → `RefreshTree` + `NotifyRepoChanged`; erro → `MessageBox`. |
| **Nova branch daqui…** | local ou tag | `DoNewBranch`: pede nome (`InputDialog`) → `git checkout -b "<novo>" "<ref>"`. Sucesso → refresh + notify. |
| **Mesclar na branch atual** | local | `DoMerge`: confirma → `git merge "<nome>"`. Sucesso → refresh; erro → `MessageBox`. |
| **Rebase na branch atual** | local | `DoRebase`: confirma → `git rebase "<nome>"`. Sucesso → refresh; erro → `MessageBox`. |
| **Renomear…** | local | `DoRename`: pede novo nome → `git branch -m "<antigo>" "<novo>"`. |
| **Excluir…** | local/remote/tag | `DoDelete`: confirma; tag → `git tag -d` **+** `git push <remote> --delete <tag>` (remove local **e** do remoto; "remote ref does not exist" é tratado como sucesso); remote → `git push <remote> --delete <branch>`; local → `git branch -d`. Se "not fully merged" → oferece **forçar** (`git branch -D`). |
| **GitFlow…** | branch | Igual ao botão GitFlow → `DoGitFlow`. |
| **Restore…** | branch atual ≠ `develop` | Igual ao botão Restore → `DoRestore` (abre a janela Restore). Não depende do nó clicado — sempre age na branch em checkout. |
| **Expandir tudo** | sempre | `node.ExpandAll()`. |
| **Recolher tudo** | sempre | `CollapseRecursive(node)`. |
| **Atualizar** | sempre | `RefreshTree()`. |

## 🎨 Ícones por tipo de nó (`NodeIcons`)
- LOCAL (monitor) · REMOTES (nuvem) · TAGS (fita) · pasta de caminho (âmbar).
- `master`/`main` escudo dourado · `develop` (PNG embutido) · `feature/*` pasta=ramo, folha=folha verde · `bugfix/*` joaninha · `release/*` pacote · `hotfix/*` aviso · `support/*` engrenagem.
- Vários usam **PNG embutido** em `Resources\` com fallback GDI+. Ver [[2026-06-01 - Ícone customizado do develop]].

## 🔗 Relacionado
- [[Interface GitFlow — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[Plugin MEF para GitExtensions]]
- [[git flow - chaves de config (CLI)]]
