---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [fluxo, restore, recuperacao, desfazer, etapa3]
---

# Flow: 3 — Restore (go back in time)

The `RestoreForm` window, opened by the **Restore** button. 10 tabs ordered from the safest to the most destructive.

![[ScreenshotRestore.png]]

## Steps

```
[Restore]  →  choose the tab according to the goal (safe → destructive):

  1. Emergency Plan       branch ← tag (safety net)
  2. Restore File         git restore (Browse… restricted to the repo root)
  3. Restore Tree         restores the tree at a commit
  4. Restore Tag          checkout/restore from a tag
  5. Cherry-Pick          git cherry-pick
  6. Revert               git revert (commit / merge -m 1)
  7. Reset Branch         git reset
  8. New Branch/Tag       creates a ref (+ Inspect detached)
  9. Recover (Reflog)     git reflog → recover lost refs
 10. Discard Local        checkout / reset --hard HEAD / clean
     Rebase               removes commit
```

## Details

- Each category carries an **embedded explanation** + teamwork guidance; a scrollable **"About Restore"** tab.
- The **Reflog** is the safety net even after destructive operations.
- On closing, the main window regains focus and refreshes the tree.

## Related

- [[🌲 Abrir e navegar a árvore (EN)|Open and navigate the tree]]
- [[🔀 GitFlow (Start a Finish) (EN)|GitFlow (Start to Finish)]]
- [[⏪ RestoreForm (EN)|RestoreForm]]
- [[⏪ Restore — central de voltar no tempo (EN)|Restore — central hub for going back in time]]
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
