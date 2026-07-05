---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, restore, recuperacao, ux]
status: aceita
criado: 2026-06-07
---

# ADR — Restore as the single hub for "going back in time"

## Context
Git's undo/recover operations (restore, revert, reset, cherry-pick, reflog, rebase, discard) are powerful but **scary and scattered**, with very different risks among them — from trivial (restoring a file) to destructive (reset --hard, rebase). The user rarely remembers the right command or its degree of danger.

## Decision
Concentrate everything in a single **`RestoreForm`** window with **10 tabs ordered from the safest to the most destructive**, each category with an **embedded explanation** and teamwork guidance, plus a scrollable **"About Restore"** tab. Order: Emergency Plan → Restore File/Tree/Tag → Cherry-Pick → Revert → Reset → New Branch/Tag → Reflog → Discard Local → Rebase.

## Consequences
**Positive:** a single place to recover from mistakes; the risk-based order educates and reduces accidents; the **Reflog** provides a safety net even after destructive operations.

**Trade-offs:** a large window (~980 px, 10 tabs) — mitigated by the risk-based ordering and the per-category explanations.

## 🔗 Related
- [[⏪ RestoreForm (EN)|RestoreForm]]
- [[🛡️ Modo Developer protege main-develop (EN)|Developer Mode protects main/develop]]
- [[⏪ Interface Restore — botões e fluxos (EN)|Restore Interface — buttons and flows]]
- [[⏪ Restore (voltar no tempo) (EN)|Restore (go back in time)]]
