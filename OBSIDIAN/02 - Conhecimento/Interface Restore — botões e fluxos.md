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

## 🧭 Layout
- Janela **980 px** de largura, `TabControl` com **Multiline = true** (abas em múltiplas linhas — todas visíveis de uma vez). Header `HEAD: <ref>` + link **"Sobre o Restore"**. Caixa **Resultado** (Consolas, fundo bege `#EFEBD8`) preenche abaixo das abas. Rodapé: **Fechar** (centro, = `CancelButton`/Esc), **Show Debug** (esq.), **Idioma** (dir.).
- **Layout responsivo** (`LayoutResponsive`) — combos/campos esticados e botões realinhados à direita em runtime, **margem direita = esquerda** (`SideMargin = 14`); recalculado no `Load` e em `_tabs.ClientSizeChanged`.

## 🗂️ Abas (ordem segura → destrutiva) — campos de cada aba

🟢 **Seguras (não reescrevem histórico)**

- **Plano de Emergência** — **Branch:** (combo, pré-selec. branch atual) + **Tag:** (combo). Botões **Restaurar para a Tag** `checkout <tag> -- .` (staged) / **Resetar para a Tag** (vermelho) `reset --hard <tag>` (confirma).
  ![[ScreenShots/ScreenshotRestoreEmergencyPlan.png]]
- **Restaurar Arquivo** — **Commit hash:** (combo) + **Arquivo (caminho relativo):** (textbox) + **Procurar…** (`_btnBrowseFile`, `OpenFileDialog` em `_svc.WorkingDir`, valida dentro da raiz, grava relativo com `/`). Botão **Restaurar Arquivo** `checkout <hash> -- "<arquivo>"`.
  ![[ScreenShots/ScreenshotRestoreFile.png]]
- **Restaurar Árvore** — **Commit hash:** (`_cboTreeHash`). Botão **Restaurar Árvore** `checkout <hash> -- .` (toda a árvore rastreada, staged).
  ![[ScreenShots/ScreenshotRestoreTree.png]]
- **Cherry-Pick** — **Commit hash:** (combo, aceita range `antigo..recente`). Botão **Aplicar Cherry-Pick** `cherry-pick <hash>`.
  ![[ScreenShots/ScreenshotRestoreCherry-Pick.png]]
- **Reverter** — **Commit hash:** (`_cboRevertHash`). Botões **Reverter Commit** `revert --no-edit <hash>` / **Reverter Merge (-m 1)** `revert -m 1 --no-edit <hash>` (desfazer **seguro**, novo commit, p/ branch compartilhada).
  ![[ScreenShots/ScreenshotRestoreRevert.png]]
- **Nova Branch/Tag** — **Commit hash:** (`_cboNewRefHash`) + **Nome:** (`_txtNewRefName`). Botões **Inspecionar** `checkout <hash>` (🔵 detached HEAD, só leitura, confirma) / **Criar Tag** `tag <nome> <hash>` / **Criar Branch** `branch <nome> <hash>`.
  ![[ScreenShots/ScreenshotRestoreNewBranchTag.png]]

🟡 **Recuperação**
- **Recuperar (Reflog)** — **Entrada:** (`_cboReflog`, populado por `git log -g -150`, selector `%gd`=`HEAD@{n}`, subject `%gs`) + **Nome:** (`_txtReflogBranch`). Botões **Criar Branch Aqui** `branch <nome> <sha>` / **Resetar Atual p/ Aqui** (vermelho) `reset --hard <sha>` (confirma).
  ![[ScreenShots/ScreenshotRestoreRecoverReflog.png]]

🟠 **Descartar locais**
- **Descartar Locais** — botões **Descartar não staged (tracked)** `checkout -- .` / **Reset --hard HEAD** (vermelho) / **Remover não rastreados (clean -fd)** (vermelho); todos confirmam.
  ![[ScreenShots/ScreenshotRestoreDiscarLocal.png]]

🔴 **Reescrevem histórico**
- **Reset Branch** — **Branch:** (`_cboBranch`, pré-selec. atual) + **Commit hash:** (`_cboResetHash`) + **Modo** (radio `--mixed`/`--soft`/`--hard`). Botão **Resetar Branch** (vermelho) `reset --<modo> <hash>`; se a branch ≠ atual, faz `checkout <branch>` → reset → volta. `--hard` confirma.
  ![[ScreenShots/ScreenshotRestoreResetBranch.png]]
- **Rebase** — **Commit hash:** (`_cboRebaseHash`). Botões **Remover Commit do Histórico** (vermelho) `rebase --onto <hash>^ <hash>` (remove o commit, reaplica posteriores, confirma; em conflito anexa `rebaseConflictHint`) / **Abortar Rebase** `rebase --abort`.
  ![[ScreenShots/ScreenshotRestoreRebase.png]]

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
