---
tipo: sistema
tags: [sistema, overview, plugin, gitextensions, winforms, gitflow]
atualizado: 2026-07-01
versao: 1.0.358
---

# Visão Geral

## O que é

Plugin para **[GitExtensions](https://gitextensions.github.io/)** (Windows) que exibe as branches do repositório **hierarquicamente em árvore** (mostrando branches filhas) em vez da lista plana padrão, e disponibiliza o uso da metodologia **GitFlow** de maneira visual, fácil e intuitiva. Tem ícone próprio "Árvore da Vida" desenhado/embutido (GDI+ / `Resources/ico.png`). Nota de projeto: [[GitExtensions.ZimerfeldTree]].

## Stack

| Item | Valor |
|---|---|
| Linguagem | C# (.NET 9), `Nullable` + `ImplicitUsings`, `LangVersion=latest` |
| Target | `net9.0-windows` |
| UI Framework | Windows Forms (`UseWindowsForms`) |
| Tipo de saída | `Library` (DLL carregada pelo GitExtensions, não exe) |
| Assembly de saída | `GitExtensions.Plugins.ZimerfeldTree.dll` |
| Namespace raiz | `GitExtensions.ZimerfeldTree` |
| Plugin model | MEF (`System.ComponentModel.Composition`) — ver [[Plugin MEF para GitExtensions]] |
| Versão atual | **1.0.358** |
| Idiomas | Inglês / Português (por janela, persistido individualmente) |
| Autor | Zimerfeld |

> **Referências externas** (de `C:\Program Files\GitExtensions\`, `Private=false`, não copiadas): `GitExtensions.Extensibility.dll`, `GitUIPluginInterfaces.dll`, `System.ComponentModel.Composition.dll`.

## As três janelas

### 1. Branch Hierarchy (`BranchHierarchyForm`)
Janela **não-modal**, singleton por sessão, abre centralizada e redimensionável, independente do GitExtensions. Árvore em 3 seções fixas — **LOCAL**, **REMOTES**, **TAGS** — com contadores `(N)`, branch atual em negrito, filtro em tempo real e botões **Pull / Push / Commit / Excluir / GitFlow / Restore** acima da árvore. Carregamento assíncrono com overlay de progresso na 1ª abertura. Detalhes controle-a-controle: [[Interface ZimerfeldTree — botões e fluxos]].

### 2. GitFlow (`GitFlowForm`)
Janela modal que dirige os comandos `git flow` usando **apenas git nativo** (não depende do binário `git-flow` instalado): start/publish/track/update/finish para feature, bugfix, release, hotfix e support. Permite **hierarquia flexível** (feature filha de feature, via *based on:*). Detalhes: [[Interface GitFlow — botões e fluxos]] e [[git flow - chaves de config (CLI)]].

### 3. Restore (`RestoreForm`)
Central de "voltar no tempo" com 10 abas, da mais segura à mais destrutiva: Plano de Emergência, Restaurar Arquivo/Árvore/Tag, Cherry-Pick, **Reverter**, Reset Branch, **Nova Branch/Tag**, **Recuperar (Reflog)**, **Descartar Locais** e **Rebase**. Cada categoria com explicação embutida e orientações de trabalho em equipe. Detalhes: [[Interface Restore — botões e fluxos]].

## Hierarquia flexível do GitFlow — feature filha de feature

O GitFlow clássico não prevê feature filha de feature (todas as `feature/*` derivam de `develop` e são irmãs). O **ZimerfeldTree GitFlow** permite uma hierarquia flexível onde uma `feature/*` pode derivar de `develop` **ou de outra `feature/*`** acima dela (via *based on:* no Start). Consequência: o *finish feature* deve **cascatear** as mudanças até o nó pai sucessivamente, reaplicando *finish feature* até chegar em `develop`.

## Localização (Inglês / Português)

Cada janela escolhe seu idioma de forma **independente** e o memoriza. A janela principal usa `I18n.SetLanguage` global (persistido em `ZimerfeldTree.language.json`); GitFlow e Restore têm seu próprio seletor persistido nos respectivos arquivos de settings.

## Relacionado

- [[GitExtensions.ZimerfeldTree]]
- [[Versionamento e Build]]
- [[README — Instalação, Uso e Build]]
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[Interface GitFlow — botões e fluxos]]
- [[Interface Restore — botões e fluxos]]
- [[🔑 Fatos-Chave]]
