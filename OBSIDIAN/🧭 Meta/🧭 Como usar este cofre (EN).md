---
tipo: meta
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
tags: [meta, protocolo]
---

# 🧭 How to use this vault (Claude's protocol)

> [!important] Memory protocol
> At the **start** of each session, read: [[🏠 Home (EN)|Home]], [[📌 Backlog (EN)|Backlog]], [[🔑 Fatos-Chave (EN)|Key Facts]] and the parent note [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]].
> At the **end** of each session, update the [[📌 Backlog (EN)|Backlog]] and the affected notes.

## ✍️ When to record memory
| Situation | Where to record |
|----------|-------------|
| Discovered project structure/behavior | `🧩 Sistemas/` (or the parent note in `💼 Negócio/`) |
| Learned a reusable concept or pattern | `📚 Conhecimento/` |
| Made an architecture decision | `⚖️ Decisões/` |
| Renato's preference or context | `🧭 Meta/👤 Renato.md` |
| Finished a work stage | update `📌 Backlog.md` (root) |
| Tool configuration | `🧭 Meta/` |

## 🔗 Writing rules
1. **Always use frontmatter** (`tipo`, `criado`, `atualizado`, `tags`).
2. **Interlink** with `[[wikilinks]]` — the vault's value is in the connections.
3. **Atomicity**: one idea per note when possible.
4. **Dates in ISO** `YYYY-MM-DD`.
5. Use **callouts** (`> [!note]`, `> [!warning]`) for highlights.
6. No secrets/passwords in the vault.

## 🧩 Recommended plugins (optional)
- **Dataview** — optional; the current [[🏠 Home (EN)|Home]] does not depend on it.
- **Templater** — advanced templates.
Without them the vault works normally; only the `dataview` blocks turn into plain text.
