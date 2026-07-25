---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, icone, plugin, resources]
arquivo: src/GitExtensions.ZimerfeldTree/PluginIcon.cs
---

# PluginIcon.cs

Ícone do plugin/janela — "Árvore da Vida". ~33 linhas.

**Caminho:** `src/GitExtensions.ZimerfeldTree/PluginIcon.cs`

---

## Papel

Carrega `Resources/ico.png` **uma única vez** e o mantém em cache, servindo o ícone para:
- o item de menu **Plugins → ZimerfeldTree** no host, e
- a **barra de título** das três janelas (`BranchHierarchy` / `GitFlow` / `Restore`).

Diferente de [[🎨 NodeIcons (PT)|🎨 NodeIcons]] (ícones 16×16 dos nós da árvore), este é o ícone único e maior de identidade do plugin.

## Relacionado

- [[🌳 ZimerfeldTreePlugin (PT)|🌳 ZimerfeldTreePlugin]]
- [[🎨 NodeIcons (PT)|🎨 NodeIcons]]
- [[🏗️ Arquitetura (PT)|🏗️ Arquitetura]]
