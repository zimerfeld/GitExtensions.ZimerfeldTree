---
tipo: conhecimento
criado: 2026-06-06
atualizado: 2026-06-07 (botão Restore, helper RevealInTree, fechamento sem refresh/notify)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, restore]
fonte: src\GitExtensions.ZimerfeldTree\RestoreForm.cs
---

# Interface Restore — botões e fluxos

> [!abstract] Resumo
> Janela **modal** (`RestoreForm`) que dirige operações de restauração de histórico git: restaurar arquivo de commit, cherry-pick e reset de branch. Acessível via botão **Restore** (antes "Voltar Versão") da [[Interface ZimerfeldTree — botões e fluxos]]. Projeto: [[GitExtensions.ZimerfeldTree]].

![[ScreenShots/ScreenshotRestore.png]]

## 🧭 Layout
- **Header** — `HEAD: <ref simbólico>` + link **"About Restore"** (canto superior direito).
- **Restaurar Arquivo** (grupo) — `Commit hash` (combobox com histórico) + `Arquivo` (caminho relativo, TextBox) + botão **Restaurar**.
- **Cherry-Pick** (grupo) — `Commit(s)` (combobox com histórico, aceita hash simples ou range `antigo..recente`) + botão **Cherry-Pick**.
- **Reset Branch** (grupo) — `Branch` (combobox de branches locais, padrão `develop`) + `Commit hash` (combobox com histórico) + radio buttons `--mixed` / `--soft` / `--hard` + botão **Reset**.
- **Resultado** — caixa multilinha somente-leitura (fonte Consolas), scroll automático para o fim.
- **Fechar** (também é o `CancelButton` — Esc).

## 🚀 Ao abrir (`Load` → `InitData`)
1. Preenche `HEAD:` (`git rev-parse --symbolic-full-name HEAD`).
2. Popula combo de branches para Reset (`git branch --format=%(refname:short)`).
3. Restaura últimos valores dos campos de `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`.

## 🖱️ Botões e ações

> `RevealInTree(branch)` — helper espelhando o da [[Interface GitFlow — botões e fluxos]]: dispara `RepoMutated` (a ZimerfeldTree atualiza a árvore em background e revela a branch quando informada) e reativa o modal. Passar `null` só refresca. Os handlers chamam este helper em vez de `RepoMutated?.Invoke` direto.

### Botão Restaurar (`_btnRestoreFile`) → `DoRestoreFile`
1. Lê hash e caminho do arquivo; aborta se vazios.
2. `git checkout <hash> -- "<arquivo>"` — recupera o arquivo no estado do commit e o coloca como **staged**.
3. Exibe resultado. Sucesso → `RevealInTree(null)` (só refresca).

### Botão Cherry-Pick (`_btnCherryPick`) → `DoCherryPick`
1. Lê campo `Commit(s)`:
   - Hash simples → `git cherry-pick <hash>`.
   - Range com `..` → `git cherry-pick <antigo>..<recente>`.
2. Exibe resultado. Sucesso → `RevealInTree(null)`.

### Botão Reset (`_btnReset`) → `DoReset`
1. Lê branch, hash e modo (`--mixed` / `--soft` / `--hard`).
2. `--hard` pede confirmação (`MessageBox`).
3. Se a branch não for a atual: `git checkout <branch>`.
4. `git reset <modo> <hash>`.
5. Se fez checkout temporário: `git checkout <branch-original>` para retornar.
6. Exibe resultado. Sucesso → `RevealInTree(branch)` (refresca e revela a branch resetada).

## ⚙️ Comportamento da janela
- Posicionada **lado a lado** com BranchHierarchy (ambas centralizadas na tela — mesmo comportamento da janela GitFlow).
- Após cada operação bem-sucedida, a árvore de BranchHierarchy é **atualizada em background** (via `RevealInTree`/`RepoMutated`) sem perder o foco da janela Restore.
- **Ao fechar** (botão Fechar, Esc ou X): o owner **não** dispara refresh extra nem `NotifyRepoChanged` — a árvore já está atualizada das operações feitas ao vivo. O `FormClosing` da janela apenas **persiste os campos** em `ZimerfeldRestore.settings.json` (`SaveSettings`), restaurados na próxima abertura.
- Link **About Restore** exibe `MessageBox` descrevendo o propósito de cada operação.

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[GitExtensions.ZimerfeldTree]]
- [[Interface GitFlow — botões e fluxos]]
