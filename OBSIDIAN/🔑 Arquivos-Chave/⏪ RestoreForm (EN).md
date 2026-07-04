---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, restore, modal, recuperacao]
arquivo: src/GitExtensions.ZimerfeldTree/RestoreForm.cs
---

# RestoreForm.cs

Modal "go back in time" window — 10 recovery/undo tabs, from the safest to the most destructive. ~1473 lines. Title: `ZimerfeldTree - Restore`.

**Path:** `src/GitExtensions.ZimerfeldTree/RestoreForm.cs`

---

## The 10 tabs (safe → destructive)

| # | Tab | git action |
|---|---|---|
| 1 | Emergency Plan | branch ← tag |
| 2 | Restore File | `git restore` (with **Browse…** restricted to the repo root) |
| 3 | Restore Tree | restores the tree at a commit |
| 4 | Restore Tag | checkout/restore from a tag |
| 5 | Cherry-Pick | `git cherry-pick` |
| 6 | **Revert** | `git revert` (commit / merge `-m 1`) |
| 7 | Reset Branch | `git reset` |
| 8 | **New Branch/Tag** | creates branch/tag (+ Inspect detached) |
| 9 | **Recover (Reflog)** | `git reflog` → recover lost refs |
| 10 | **Discard Local** / **Rebase** | checkout / reset `--hard HEAD` / clean · rebase (removes commit) |

## Notes

- Each category carries an **embedded explanation** and teamwork guidance.
- **About Restore** = scrollable window with an explanation per category.
- Width ~980 px.

## Related

- [[⚙️ BranchHierarchyService (EN)|BranchHierarchyService]]
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]] — opens Restore via the button
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
- [[⏪ Restore (voltar no tempo) (EN)|Restore (go back in time)]]
- [[⏪ Restore — central de voltar no tempo (EN)|Restore — central hub for going back in time]]
