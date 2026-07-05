---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, ui, winforms, restore, modal, recuperacao]
arquivo: src/GitExtensions.ZimerfeldTree/RestoreForm.cs
---

# RestoreForm.cs

Janela modal "voltar no tempo" — 10 abas de recuperação/desfazer, da mais segura à mais destrutiva. ~1473 linhas. Título: `ZimerfeldTree - Restore`.

**Caminho:** `src/GitExtensions.ZimerfeldTree/RestoreForm.cs`

---

## As 10 abas (segura → destrutiva)

| # | Aba | Ação git |
|---|---|---|
| 1 | Plano de Emergência | branch ← tag |
| 2 | Restaurar Arquivo | `git restore` (com **Procurar…** restrito à raiz do repo) |
| 3 | Restaurar Árvore | restaura a árvore num commit |
| 4 | Restaurar Tag | checkout/restore a partir de tag |
| 5 | Cherry-Pick | `git cherry-pick` |
| 6 | **Reverter** | `git revert` (commit / merge `-m 1`) |
| 7 | Reset Branch | `git reset` |
| 8 | **Nova Branch/Tag** | cria branch/tag (+ Inspecionar detached) |
| 9 | **Recuperar (Reflog)** | `git reflog` → recuperar refs perdidas |
| 10 | **Descartar Locais** / **Rebase** | checkout / reset `--hard HEAD` / clean · rebase (remove commit) |

## Notas

- Cada categoria traz **explicação embutida** e orientações de trabalho em equipe.
- **Sobre o Restore** = janela rolável com explicação por categoria.
- Largura ~980 px.

## Relacionado

- [[⚙️ BranchHierarchyService]]
- [[🪟 BranchHierarchyForm]] — abre o Restore pelo botão
- [[⏪ Interface Restore — botões e fluxos]]
- [[⏪ Restore (voltar no tempo)]]
- [[⏪ Restore — central de voltar no tempo]]
