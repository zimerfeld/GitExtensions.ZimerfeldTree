---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, icones, gdi, resources, imagelist]
arquivo: src/GitExtensions.ZimerfeldTree/NodeIcons.cs
---

# NodeIcons.cs

Ícones 16×16 da árvore — PNGs embutidos com fallback desenhado em GDI+. ~381 linhas.

**Caminho:** `src/GitExtensions.ZimerfeldTree/NodeIcons.cs`

---

## Como funciona

- Gera/carrega os ícones 16×16 do `ImageList` da árvore. Índices: **0–4** genéricos, **5–7** seções, **8–15** GitFlow/folha.
- **`LoadEmbedded`** lê o recurso por `GitExtensions.ZimerfeldTree.Resources.<arquivo>` e redimensiona para 16×16. Se ausente/ilegível, cai no **glifo GDI+ de reserva** → o build nunca quebra por falta de imagem (cada `<EmbeddedResource>` é `Condition="Exists(...)"`).
- **Grupo de remote (`origin`)** usa `Resources\origin.png` (foguete) via `NodeIcons.Remote`, mapeado em `GetFolderIconIndex`.
- **Develop (índice 9)** usa `Resources\develop.png`, fallback `Wrench()`.

## PNGs de nós (Resources/)

`master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png`, `origin.png`, `remote-branch.png`, `tag.png` + os `ctx-*.png` do menu de contexto. `ctx-pull`/`ctx-push` (seta ↓ azul / ↑ verde) também alimentam os botões Pull/Push.

## Relacionado

- [[🌿 BranchNode (PT)|🌿 BranchNode]]
- [[🖼️ PluginIcon (PT)|🖼️ PluginIcon]]
- [[🪟 BranchHierarchyForm (PT)|🪟 BranchHierarchyForm]]
