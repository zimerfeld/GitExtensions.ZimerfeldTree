---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, restore, modal, recuperacao]
arquivo: src/GitExtensions.ZimerfeldTree/RestoreForm.cs
---

# RestoreForm.cs

> 🇧🇷 Lee esta página en portugués → [[⏪ RestoreForm (PT)|⏪ RestoreForm]]
> 🇺🇸 Read this page in English → [[⏪ RestoreForm (EN)]]

Ventana modal "volver en el tiempo" — 10 pestañas de recuperación/deshacer, de la más segura a la más destructiva. ~1473 líneas. Título: `ZimerfeldTree - Restore`.

**Ruta:** `src/GitExtensions.ZimerfeldTree/RestoreForm.cs`

---

## Las 10 pestañas (segura → destructiva)

| # | Pestaña | Acción git |
|---|---|---|
| 1 | Plan de Emergencia | branch ← tag |
| 2 | Restaurar Archivo | `git restore` (con **Examinar…** restringido a la raíz del repo) |
| 3 | Restaurar Árbol | restaura el árbol en un commit |
| 4 | Restaurar Tag | checkout/restore a partir de un tag |
| 5 | Cherry-Pick | `git cherry-pick` |
| 6 | **Revertir** | `git revert` (commit / merge `-m 1`) |
| 7 | Reset Branch | `git reset` |
| 8 | **Nueva Branch/Tag** | crea branch/tag (+ Inspeccionar detached) |
| 9 | **Recuperar (Reflog)** | `git reflog` → recuperar refs perdidas |
| 10 | **Descartar Locales** / **Rebase** | checkout / reset `--hard HEAD` / clean · rebase (elimina commit) |

## Notas

- Cada categoría trae **explicación integrada** y orientaciones de trabajo en equipo.
- **Acerca de Restore** = ventana desplazable con explicación por categoría.
- Ancho ~980 px.

## Relacionado

- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]] — abre Restore mediante el botón
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]]
- [[⏪ Restore (voltar no tempo) (ES)|Restore (volver en el tiempo)]]
- [[⏪ Restore — central de voltar no tempo (ES)|Restore — centro de volver en el tiempo]]
