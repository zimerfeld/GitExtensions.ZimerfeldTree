---
tipo: arquivo
tags: [arquivo, modelo, dados, enum]
arquivo: src/GitExtensions.ZimerfeldTree/BranchNode.cs
atualizado: 2026-07-04
---

# BranchNode.cs

Modelos de dados da árvore. ~41 linhas.

**Caminho:** `src/GitExtensions.ZimerfeldTree/BranchNode.cs`

---

## Conteúdo

- **`BranchInfo`** — classe com os dados de uma branch/tag (nome, tipo, se é a atual, contadores ahead/behind, ref pai etc.), usada por [[BranchHierarchyService]] ao montar a árvore.
- **`BranchType`** (enum) — `Local` · `Remote` · `Tag`. Dirige o ícone escolhido em [[NodeIcons]] e a seção (LOCAL / REMOTES / TAGS) onde o nó aparece.

## Relacionado

- [[BranchHierarchyService]]
- [[NodeIcons]]
- [[../Sistema/Arquitetura]]
