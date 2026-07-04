---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [decisao, adr, ui, seguranca, branches]
status: aceita
criado: 2026-06-16
---

# ADR — "Developer Mode" to protect main / master / develop

## Context
The window allows multiple selection by checkbox and **batch deletion** of branches. Long-lived branches (`main`/`master`/`develop`) are the most dangerous to delete by accident.

## Decision
Add a **"Developer Mode"** checkbox (next to "Show Debug"):
- **Off (default):** `main`/`master`/`develop` are **protected** — checkbox blocked, they cannot be checked or deleted.
- **On:** unlocks the checking/deletion of those specific branches.

Turning the mode off **automatically unchecks** any main/master/develop that was checked. State persisted in `ZimerfeldTree.uisettings.json`.

## Consequences
**Positive:** protects by default the state that reflects production; unlocking is explicit and deliberate.

**Trade-offs:** one extra step for those who really need to touch those branches (acceptable — that is precisely the intent).

## 🔗 Related
- [[🪟 BranchHierarchyForm (EN)|BranchHierarchyForm]]
- [[⏪ Restore — central de voltar no tempo (EN)|Restore — central hub for going back in time]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)|ZimerfeldTree Interface — buttons and flows]]
