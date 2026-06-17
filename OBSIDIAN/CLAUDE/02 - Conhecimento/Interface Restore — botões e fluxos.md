---
tipo: conhecimento
criado: 2026-06-06
atualizado: 2026-06-17 (janela expandida para 980 px com 10 abas cobrindo TODAS as formas de voltar no tempo: + Restaurar Árvore, Reverter, Nova Branch/Tag (+Inspecionar), Recuperar (Reflog), Descartar Locais e Rebase; botão Procurar restrito à raiz do repo; About virou janela rolável com explicação por categoria + trabalho em equipe)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, restore]
fonte: src\GitExtensions.ZimerfeldTree\RestoreForm.cs
---

# Interface Restore — botões e fluxos

> [!abstract] Resumo
> Janela **modal** (`RestoreForm`) — a central de "voltar no tempo" do código. Reúne **todas** as formas de recuperar, desfazer ou descartar um estado do repositório, cada uma em sua **aba**, organizadas da mais segura à mais destrutiva. Acessível via botão **Restore** da [[Interface ZimerfeldTree — botões e fluxos]]. Projeto: [[GitExtensions.ZimerfeldTree]].

![[ScreenShots/ScreenshotRestore.png]]

## 🧭 Layout
- Janela **980 px** de largura, `TabControl` com **Multiline = true** (abas em múltiplas linhas — todas visíveis de uma vez). Header `HEAD: <ref>` + link **"Sobre o Restore"**. Caixa **Resultado** (Consolas, fundo bege `#EFEBD8`) preenche abaixo das abas. Rodapé: **Fechar** (centro, = `CancelButton`/Esc), **Show Debug** (esq.), **Idioma** (dir.).
- **Layout responsivo** (`LayoutResponsive`) — combos/campos esticados e botões realinhados à direita em runtime, **margem direita = esquerda** (`SideMargin = 14`); recalculado no `Load` e em `_tabs.ClientSizeChanged`.

## 🗂️ Abas (ordem segura → destrutiva)

🟢 **Seguras (não reescrevem histórico)**
- **Plano de Emergência** — branch ← **tag**: `checkout <tag> -- .` (restaurar, staged) / `reset --hard <tag>` (resetar, confirma).
- **Restaurar Arquivo** — `checkout <hash> -- "<arquivo>"`. Botão **Procurar…** (`_btnBrowseFile`) abre `OpenFileDialog` em `_svc.WorkingDir`; valida que o arquivo está **dentro da raiz** (rejeita fora, com aviso) e grava o caminho **relativo** com `/`.
- **Restaurar Árvore** (`_cboTreeHash`) — `checkout <hash> -- .` (toda a árvore rastreada de qualquer commit, staged).
- **Cherry-Pick** — `cherry-pick <hash>` (aceita range `antigo..recente`).
- **Reverter** (`_cboRevertHash`) — `DoRevert(merge)`: `revert --no-edit <hash>` / `revert -m 1 --no-edit <hash>`. Desfazer **seguro** (novo commit) para branch compartilhada.
- **Nova Branch/Tag** (`_cboNewRefHash` + `_txtNewRefName`) — `DoNewRef(tag)`: `branch <nome> <hash>` / `tag <nome> <hash>`.

🔵 **Inspecionar** — botão **Inspecionar** na aba Nova Branch/Tag: `checkout <hash>` (detached HEAD, só leitura; confirma).

🟡 **Recuperação** — **Recuperar (Reflog)** (`_cboReflog` + `_txtReflogBranch`): combo populado por `git log -g -150 ...` (selector `%gd` = `HEAD@{n}`, subject `%gs`). `branch <nome> <sha>` (recriar/recuperar) ou `reset --hard <sha>` (confirma).

🟠 **Descartar locais** — **Descartar Locais**: `checkout -- .` / `reset --hard HEAD` / `clean -fd` (todos confirmam; os dois últimos em vermelho).

🔴 **Reescrevem histórico**
- **Reset Branch** (`_cboBranch` pré-selec. branch atual + `_cboResetHash`) — `reset --mixed/--soft/--hard <hash>`; se a branch ≠ atual, faz `checkout <branch>` → reset → volta. `--hard` confirma.
- **Rebase** (`_cboRebaseHash`) — `rebase --onto <hash>^ <hash>` (remove o commit, reaplica posteriores; confirma). Em conflito, anexa `rebaseConflictHint` ao resultado; botão **Abortar Rebase** → `rebase --abort`.

> **Dropdowns de commit** (`HashCombos`: Restaurar Arquivo, Árvore, Cherry-Pick, Reverter, Reset, Nova Branch/Tag, Rebase) populados por `git log --all --source -200 ... %h␟%S␟%cd␟%s`; item `(YYYY-MM-dd HH:mm:ss) [branch] hash → mensagem`, mais novo primeiro, prompt **Selecione...**, **não** persistidos. O combo do Reflog usa fonte própria (`LoadReflogRefs`).

## 🚀 Ao abrir (`Load` → `InitData`)
1. `HEAD:` via `GetHeadRef()`. 2. Combos de branch (Reset + Emergência) via `git branch`. 3. Tags na Emergência. 4. Todos os `HashCombos` recebem prompt + refs; o Reflog recebe prompt + `LoadReflogRefs()`. 5. `RestoreSettings` — **nenhum combo** é restaurado; só `restoreFile`, `resetMode`, `showDebug`, `language`. 6. `SelectBranchDefault` pré-seleciona a branch em checkout nos dois combos de branch.

## 🧩 Helpers
- `RevealInTree(branch)` — dispara `RepoMutated` (ZimerfeldTree atualiza a árvore em background e revela a branch) e reativa o modal; `null` só refresca.
- `HashOf(combo)` — retorna `CommitRef.Hash` do item selecionado ou o texto digitado (ignora o prompt).
- `RunGit(args, append)` — executa via `_svc.RunGitFlow`, escreve no Resultado e reatualiza `HEAD:`.

## ⚙️ Comportamento da janela
- Posicionada **lado a lado** com BranchHierarchy (fallback de `DoRestore` lida com telas menores que `main + 980 + gap`).
- Após cada operação bem-sucedida, a árvore atualiza **em background** sem roubar o foco do Restore.
- **Ao fechar**: o owner **não** dispara refresh extra nem traz o GitExtensions à frente; `FormClosing` só persiste campos não-combo (`restoreFile`, `resetMode`, `showDebug`, `language`) em `ZimerfeldRestore.settings.json`.
- **Show Debug** e **Idioma** são **por janela** (mesmo arquivo de settings), independentes das demais.
- **Sobre o Restore** (`ShowAbout`) — agora abre uma **janela própria rolável** (`TextBox` read-only, redimensionável) com a explicação completa: cada aba por **categoria de segurança** (🟢🔵🟡🟠🔴) + seção **👥 Trabalho em equipe** (vários devs na mesma `main` → use Reverter, `pull --rebase` antes do push; várias branches na `develop` → Cherry-Pick, Reverter Merge -m 1, abortar rebase/merge, criar branch a partir de commit). Texto vem da chave `aboutBody` (en/pt).

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[Interface GitFlow — botões e fluxos]]
