---
tipo: arquivo
tags: [arquivo, mef, plugin, entrypoint, gitextensions]
arquivo: src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs
atualizado: 2026-07-04
---

# ZimerfeldTreePlugin.cs

Ponto de entrada MEF do plugin (`IGitPlugin`). ~238 linhas.

**Caminho:** `src/GitExtensions.ZimerfeldTree/ZimerfeldTreePlugin.cs`

---

## Papel

Classe exportada via MEF (`[Export(typeof(IGitPlugin))]`), herdando de `GitPluginBase`. É o objeto que o **GitExtensions** instancia e lista no menu **Plugins → ZimerfeldTree**. Ver [[../02 - Conhecimento/Plugin MEF para GitExtensions]].

## Membros principais

| Membro | Papel |
|---|---|
| `Execute(...)` | Abre (ou traz à frente) a **janela singleton** `BranchHierarchyForm`. Reaproveita a instância existente se já aberta. |
| `Register(IGitUICommands)` | Guarda `_commands` para poder abrir os diálogos nativos do host (Commit/Push/Pull) no working dir escolhido. |
| `Unregister(...)` | Limpa a referência a `_commands`. |
| Ícone | Usa `PluginIcon` (`Resources/ico.png`, "Árvore da Vida") — ver [[PluginIcon]]. |

## Notas

- O ícone e o título das janelas mantêm sempre o prefixo **`ZimerfeldTree - `** (`BranchHierarchy` / `GitFlow` / `Restore`).
- O working directory **não** vem do repositório ativo do host — vem do `cboRepo` da janela. Ver [[../Decisoes/Janela não-modal singleton]].

## Relacionado

- [[BranchHierarchyForm]]
- [[../Sistema/Arquitetura]]
- [[../02 - Conhecimento/Plugin MEF para GitExtensions]]
