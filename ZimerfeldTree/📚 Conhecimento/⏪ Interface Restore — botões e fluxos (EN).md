---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-06
historico: 2026-06-17 (window expanded to 980 px with 10 tabs covering ALL the ways to travel back in time: + Restore Tree, Revert, New Branch/Tag (+Inspect), Recover (Reflog), Discard Local and Rebase; Browse button restricted to the repo root; About became a scrollable window with an explanation per category + team work)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, restore]
fonte: src\GitExtensions.ZimerfeldTree\RestoreForm.cs
---

# Restore Interface — buttons and flows

> [!abstract] Summary
> **Modal** window (`RestoreForm`) — the code's "time travel" hub. It gathers **all** the ways to recover, undo or discard a state of the repository, each in its own **tab**, organized from the safest to the most destructive. Reachable via the **Restore** button of the [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]. Project: [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]].

## 🧭 Layout
- **980 px** wide window, a `TabControl` with **Multiline = true** (tabs on multiple lines — all visible at once). Header `HEAD: <ref>` + an **"About Restore"** link. A **Result** box (Consolas, beige background `#EFEBD8`) fills the space below the tabs. Footer: **Close** (center, = `CancelButton`/Esc), **Show Debug** (left), **Language** (right).
- **Responsive layout** (`LayoutResponsive`) — combos/fields stretched and buttons realigned to the right at runtime, **right margin = left margin** (`SideMargin = 14`); recalculated on `Load` and on `_tabs.ClientSizeChanged`.

## 🗂️ Tabs (safe → destructive order) — fields of each tab

🟢 **Safe (do not rewrite history)**

- **Emergency Plan** — **Branch:** (combo, current branch pre-selected) + **Tag:** (combo). Buttons **Restore to Tag** `checkout <tag> -- .` (staged) / **Reset to Tag** (red) `reset --hard <tag>` (confirms).
  ![[ScreenshotRestoreEmergencyPlan.png]]
- **Restore File** — **Commit hash:** (combo) + **File (relative path):** (textbox) + **Browse…** (`_btnBrowseFile`, `OpenFileDialog` in `_svc.WorkingDir`, validates inside the root, stores relative with `/`). Button **Restore File** `checkout <hash> -- "<file>"`.
  ![[ScreenshotRestoreFile.png]]
- **Restore Tree** — **Commit hash:** (`_cboTreeHash`). Button **Restore Tree** `checkout <hash> -- .` (the whole tracked tree, staged).
  ![[ScreenshotRestoreTree.png]]
- **Cherry-Pick** — **Commit hash:** (combo, accepts a range `old..recent`). Button **Apply Cherry-Pick** `cherry-pick <hash>`.
  ![[ScreenshotRestoreCherry-Pick.png]]
- **Revert** — **Commit hash:** (`_cboRevertHash`). Buttons **Revert Commit** `revert --no-edit <hash>` / **Revert Merge (-m 1)** `revert -m 1 --no-edit <hash>` (**safe** undo, new commit, for a shared branch).
  ![[ScreenshotRestoreRevert.png]]
- **New Branch/Tag** — **Commit hash:** (`_cboNewRefHash`) + **Name:** (`_txtNewRefName`). Buttons **Inspect** `checkout <hash>` (🔵 detached HEAD, read-only, confirms) / **Create Tag** `tag <name> <hash>` / **Create Branch** `branch <name> <hash>`.
  ![[ScreenshotRestoreNewBranchTag.png]]

🟡 **Recovery**
- **Recover (Reflog)** — **Entry:** (`_cboReflog`, populated by `git log -g -150`, selector `%gd`=`HEAD@{n}`, subject `%gs`) + **Name:** (`_txtReflogBranch`). Buttons **Create Branch Here** `branch <name> <sha>` / **Reset Current to Here** (red) `reset --hard <sha>` (confirms).
  ![[ScreenshotRestoreRecoverReflog.png]]

🟠 **Discard local**
- **Discard Local** — buttons **Discard unstaged (tracked)** `checkout -- .` / **Reset --hard HEAD** (red) / **Remove untracked (clean -fd)** (red); all confirm.
  ![[ScreenshotRestoreDiscarLocal.png]]

🔴 **Rewrite history**
- **Reset Branch** — **Branch:** (`_cboBranch`, current pre-selected) + **Commit hash:** (`_cboResetHash`) + **Mode** (radio `--mixed`/`--soft`/`--hard`). Button **Reset Branch** (red) `reset --<mode> <hash>`; if the branch ≠ current, it does `checkout <branch>` → reset → comes back. `--hard` confirms.
  ![[ScreenshotRestoreResetBranch.png]]
- **Rebase** — **Commit hash:** (`_cboRebaseHash`). Buttons **Remove Commit from History** (red) `rebase --onto <hash>^ <hash>` (removes the commit, reapplies the later ones, confirms; on conflict it appends `rebaseConflictHint`) / **Abort Rebase** `rebase --abort`.
  ![[ScreenshotRestoreRebase.png]]

> **Commit dropdowns** (`HashCombos`: Restore File, Tree, Cherry-Pick, Revert, Reset, New Branch/Tag, Rebase) populated by `git log --all --source -200 ... %h␟%S␟%cd␟%s`; each item `(YYYY-MM-dd HH:mm:ss) [branch] hash → message`, newest first, prompt **Select...**, **not** persisted. The Reflog combo uses its own source (`LoadReflogRefs`).

## 🚀 On open (`Load` → `InitData`)
1. `HEAD:` via `GetHeadRef()`. 2. Branch combos (Reset + Emergency) via `git branch`. 3. Tags in Emergency. 4. All `HashCombos` receive prompt + refs; Reflog receives prompt + `LoadReflogRefs()`. 5. `RestoreSettings` — **no combo** is restored; only `restoreFile`, `resetMode`, `showDebug`, `language`. 6. `SelectBranchDefault` pre-selects the checked-out branch in both branch combos.

## 🧩 Helpers
- `RevealInTree(branch)` — fires `RepoMutated` (ZimerfeldTree refreshes the tree in the background and reveals the branch) and reactivates the modal; `null` only refreshes.
- `HashOf(combo)` — returns `CommitRef.Hash` of the selected item or the typed text (ignores the prompt).
- `RunGit(args, append)` — runs via `_svc.RunGitFlow`, writes to Result and refreshes `HEAD:`.

## ⚙️ Window behavior
- Positioned **side by side** with BranchHierarchy (`DoRestore`'s fallback handles screens smaller than `main + 980 + gap`).
- After each successful operation, the tree refreshes **in the background** without stealing focus from Restore.
- **On close**: the owner does **not** fire an extra refresh nor bring GitExtensions to the front; `FormClosing` only persists non-combo fields (`restoreFile`, `resetMode`, `showDebug`, `language`) in `ZimerfeldRestore.settings.json`.
- **Show Debug** and **Language** are **per window** (same settings file), independent of the others.
- **About Restore** (`ShowAbout`) — now opens its **own scrollable window** (a resizable read-only `TextBox`) with the full explanation: each tab by **safety category** (🟢🔵🟡🟠🔴) + a **👥 Team work** section (several devs on the same `main` → use Revert, `pull --rebase` before pushing; several branches on `develop` → Cherry-Pick, Revert Merge -m 1, abort rebase/merge, create a branch from a commit). The text comes from the `aboutBody` key (en/pt).

## 🔗 Related
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[🔀 Interface GitFlow — botões e fluxos (EN)|GitFlow Interface — buttons and flows]]
