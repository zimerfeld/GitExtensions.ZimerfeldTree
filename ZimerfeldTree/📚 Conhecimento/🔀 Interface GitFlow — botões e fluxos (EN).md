---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-06-11 (the "based on" rule driven by the Type in Start — combo filtered + checkbox enabled/disabled by type)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, gitflow]
fonte: src\GitExtensions.ZimerfeldTree\GitFlowForm.cs
---

# GitFlow Interface — buttons and flows

> [!abstract] Summary
> **Modal** window (`GitFlowForm`) that drives git flow operations using **pure git** (without depending on the `git-flow` binary): start feature/release/hotfix, publish, track, update and finish. The command output appears in a text box with automatic scrolling. Opened by the **GitFlow** button/menu of the [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]. Project: [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]].

![[ScreenshotGitFlow.png]]

## 🌳 Start and Finish rules by type

Summary diagram: for each type, the **Start base**, the **branch created** and the **merge target on Finish**.

![[ScreenShotStartFinish.png]]

| Type | Start — base | Finish — target |
| --- | --- | --- |
| **feature** | `develop` or `feature/*` (optional) | `develop` or the based-on parent (`merge --no-ff`) |
| **bugfix** | `release/*` (required) | the **release (parent)** itself — or `develop` if the release does not exist (`merge --no-ff`) |
| **release** | `develop` (fixed) | `main` (`merge --no-ff` + tag) + `develop`; push of main/develop/tag |
| **hotfix** | `main` (fixed) | `main` (`merge --no-ff` + tag) + `develop` |
| **support** | production tag (required) | `main` only (`merge --no-ff`, no tag, no develop) |

> Common to every Finish: optional fetch · deletes the local and remote branch (except **Keep**) · reconnects the children in the tree. Full details in [[#Finish button (`_btnFinish`) → `DoFinish` ⚠️ composite flow]].

### Full command flow by type

The sequence of `git` commands for each type, from Start to Finish (with remote, without No fetch):

![[ScreenShotFlowPerType.png]]

### Node positioning in the tree (based-on)

Git stores only the tip commit of each branch, not the origin. Start nests the new node through one of these mechanisms:

![[ScreenShotHierarchyBasedOn.png]]

- **empty commit** (base develop/main + based-on): `git commit --allow-empty` → the tip diverges → real ancestry
- **based-on override** (custom `feature/*` base + based-on): writes `.git/zimerfeld-basedon.json` (visual link, clean history)
- **without based-on**: a plain `checkout -b`, nested by the GitFlow rule + prefix
- Finish → `RebaseBasedOnOnFinish` removes the link and re-points the children to the target. See the code in `BranchHierarchyService.cs` (`SaveBasedOnOverride`, `ApplyBasedOnOverrides`, `BreakCycles`).

## 🧭 Layout
- **Header** — `HEAD: <symbolic ref>` + an **"About GitFlow"** link.
- **Start branch** (group) — `Type` (combo), `Expected name` (prefix label + text box) + **Start** button, **based on:** checkbox + base combo. The combo content and the checkbox state are **driven by the Type** (see [[#The "based on" rule by type (Start)]]).
- **Manage existing branches** (group) — `Type` (combo), `Branch` (combo of local branches with the prefix), buttons **Publish / Track / Update / Finish**, checkboxes **Keep branch after finish** (`-k`, checked by default) and **No fetch (--no-fetch)**.
- **Git command result** — a read-only multiline box (Consolas font); cleared when each action starts and auto-scrolls to the end as the subcommands run.
- **Close**.

Supported types (`GitFlowTypes`): `feature`, `bugfix`, `release`, `hotfix`, `support`. The prefix of each type comes from `git config gitflow.prefix.<type>` (fallback `type/`).

## 🚀 On open (`Load` → `InitData` + `ApplySettings`)
1. Fills `HEAD:` (`git rev-parse --symbolic-full-name HEAD`).
2. "based on" combo: filled by `ApplyStartTypeRule()` according to the initial `Type` (`feature` → `develop` + `feature/*`). See [[#The "based on" rule by type (Start)]].
3. **Detects the git-flow type of the current branch** and opens the Manage panel already pointing to it.
4. `Type` (Start) starts at `feature`; `Type` (Manage) at the detected branch.
5. Loads the saved checkboxes from `%APPDATA%\GitExtensions\ZimerfeldTree.gitflowsettings.json`.

## 🔁 `RunFlow(args)` — the common executor
Every action goes through here:
1. Wait cursor; runs `git <args>` (`RunGitFlow` → combined stdout+stderr + exit code).
2. Appends `command - git <args>` + the output to the Result box via `AppendText` (auto-scroll to the end). Each button calls `_txtResult.Clear()` before the first `RunFlow`, clearing the previous result.
3. Updates the `HEAD:` label and **reloads the Manage branch combo** (a branch deleted by finish disappears from here).
4. If exit code ≠ 0 and it is not `suppressError` → an error `MessageBox` (`ShowFlowError`).
5. Returns `true` if exit code == 0.

`RevealInTree(branch, checkout)`: optionally does `git checkout "<branch>"`, fires `RepoMutated` (the ZimerfeldTree behind refreshes and reveals/selects the branch) and reactivates the modal.

---

## 🖱️ Buttons and actions — step by step

### Type combo — Start
1. `SelectedIndexChanged`: updates the prefix label (`git config gitflow.prefix.<type>`).
2. If the type is **release** or **hotfix** and the name is empty → auto-fills it with the **`yyyyMMddHHmm`** convention (e.g. `202606011230`). Does not overwrite manual input.
3. Calls `ApplyStartTypeRule()` — see below.

### The "based on" rule by type (Start)
`ApplyStartTypeRule()` (fired by the `SelectedIndexChanged` of `cboStartType` and re-applied after a successful Start) repopulates `cboBasedOn` and sets the state of `chkBasedOn` according to the type:

| `cboStartType` | `cboBasedOn` | `chkBasedOn` |
| --- | --- | --- |
| **hotfix**  | `main` (fixed base)                                | **disabled** |
| **release** | `develop` (fixed base)                             | **disabled** |
| **feature** | `develop` (1st item) + local `feature/*` branches | **enabled** |
| **bugfix**  | only local `release/*` branches                   | **checked + enabled (required)** |
| *others (support)* | `develop` + all local branches             | **enabled** |

- The combo only becomes **usable** when the checkbox is **enabled AND checked** (`_cboBasedOn.Enabled = _chkBasedOn.Enabled && _chkBasedOn.Checked`).
- hotfix/release: the checkbox is unchecked and disabled → fixed base; the combo only shows `main`/`develop`. The `DoStart` fallback (without "based on") already resolves to the same `main`/`develop`, keeping consistency.
- **bugfix (project rule)**: a bugfix **can only exist linked to a release**. The checkbox already comes **checked** and the combo lists only the `release/*` branches; `DoStart` **blocks** the Start if there is no release or if the chosen base is not a `release/*`. The release base (non-root) makes `DoStart` write a **based-on override** → the bugfix ends up **nested under the release** in the tree.
- feature: check the checkbox to choose the base in the filtered combo (feature as a child of a feature).
- Branch names in the combo are **full** (e.g. `feature/x`, `release/2026`).

### Start button (`_btnStart`) → `DoStart`
1. Reads the type and name; if the name is empty → `MessageBox` and aborts.
1b. **bugfix**: if there is no `release/*` → `MessageBox` (`bugfixNeedsRelease`) and aborts; if the checkbox is not checked or the base is not an existing `release/*` → `MessageBox` (`bugfixSelectRelease`) and aborts.
2. Clears `_txtResult`.
3. `git checkout -b <prefix><name> <base>` (default base: develop for feature/release; main for hotfix/support; **release for bugfix (required)**; or the branch chosen in "based on").
4. Clears the name box.
5. Success: pre-selects the new branch in the **Manage** panel and reveals it in ZimerfeldTree (`RevealInTree(prefix+name, checkout:false)` — the checkout was already done by `-b`).
6. Failure → reactivates the modal.

### Publish button (`_btnPublish`) → `DoPublish`
1. Reads type+name (aborts if empty); aborts if no remote is configured.
2. Clears `_txtResult`.
3. `git push --set-upstream <remote> <prefix+name>`.
4. Success → `RevealInTree(prefix+name, checkout:false)`.

### Track button (`_btnTrack`) → `DoTrack`
1. Reads type+name; aborts if no remote.
2. Clears `_txtResult`.
3. If No fetch is unchecked: `git fetch <remote>`.
4. `git checkout -b <prefix+name> --track <remote>/<prefix+name>`.
5. Success → reveal.

### Update button (`_btnUpdate`) → `DoUpdate`
1. Reads type+name.
2. Clears `_txtResult`.
3. If No fetch is unchecked and a remote exists: `git fetch <remote>`.
4. `git checkout <prefix+name>`.
5. `git merge <remote>/<parent>` (or the local `<parent>` if No fetch). Parent = develop for feature/release; main for hotfix/support; **the release (parent)** for bugfix — always merged from the local release ref (releases are usually local), with a fallback to develop if no release is resolved.
6. Success → reveal.

### Finish button (`_btnFinish`) → `DoFinish` ⚠️ composite flow
1. Reads type+name (aborts if empty).
2. Clears `_txtResult`.
3. If No fetch is unchecked and a remote exists: `git fetch <remote>`.
4. **Merge sequence** (pure git, no git-flow binary):
   - feature: `checkout <develop or based-on parent>` → `merge --no-ff`.
   - bugfix: `checkout <release (based-on parent), or develop if the release does not exist>` → `merge --no-ff` (the based-on parent is the release chosen in Start; it is only not used if it was finished/deleted).
   - hotfix/release: `checkout main` → `merge --no-ff` → `tag -a <name> -m <name>` → `checkout develop` → `merge --no-ff`.
   - support: `checkout main` → `merge --no-ff`.
5. If **Keep** is unchecked: `git branch -d <prefix+name>`.
6. **Remote removal** (all types): `git ls-remote --heads <remote> <branch>` → if it exists, `git push <remote> --delete <branch>`; otherwise appends the note "(skipped: no longer exists)".
7. Post-finish for **release** (additional):
   a. `LastFinishedReleaseTag = name` (ZimerfeldTree focuses the tag on close).
   b. No remote → a "finished locally" warning and stops.
   c. `git push <remote> <main>` → `git push <remote> <develop>`.
   d. `git push <remote> refs/tags/<name>`.
   e. Remote removal of the release branch (step 6 was already executed before step 7).
   f. `git checkout <develop>` + reveal.
8. Non-release (feature/bugfix/hotfix/support): `RevealInTree(current branch, checkout:false)`.

> Merge errors stop the flow and show the result in the panel. Resolve manually (`git merge --abort` or commit).

### Finish checkboxes
- **Keep branch after finish** and **No fetch (--no-fetch)**: when changed, they save to `ZimerfeldTree.gitflowsettings.json`.
- **Show Debug** (`chkShowDebug`): persists/reloads its own state **individually** (the `showDebug` key in the same `ZimerfeldTree.gitflowsettings.json`). On the first open (with no saved value) it uses the state inherited from the owner (`showControlIds`).
- **Language** (`cboLanguage`): **per window** (the `_lang` field), persisted individually (the `language` key in `ZimerfeldTree.gitflowsettings.json`), applied on `Load` via `ApplyLanguage()` with `I18n.Load(scope, _lang)`. It no longer calls the global `I18n.SetLanguage`. In `ApplySettings`, `_lang` is set **before** the checkboxes (whose `CheckedChanged` calls `SaveSettings`) so as not to overwrite the saved language. The first open with no value inherits `I18n.Current`.

### "About GitFlow" link
- Opens a `MessageBox` describing the git commands executed by each button.

### Close button (`_btnClose`)
- `Close()` (it is also the `CancelButton`). There is no `FormClosing` (the checkboxes are already saved incrementally on each `CheckedChanged`).
- In the owner, after the modal closes: it recenters the ZimerfeldTree and **only** calls `RefreshTree()` if there was a **release finish** (to focus the tag). Otherwise it does not refresh (`RepoMutated` already updated it live) and does **not** call `NotifyRepoChanged()`.

## ⚠️ Common errors (`ShowFlowError`)
When the output contains "does not exist" / "not found" / "unknown revision" / "pathspec", the message advises checking `git branch --list main master develop` and `git config gitflow.branch.*`, creating the missing branch or using **GitFlow Initialize**.

## 🔗 Related
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[⚙️ git flow - chaves de config (CLI) (EN)|git flow — config keys (CLI)]]
- [[🧩 Plugin MEF para GitExtensions (EN)|MEF plugin for GitExtensions]]
