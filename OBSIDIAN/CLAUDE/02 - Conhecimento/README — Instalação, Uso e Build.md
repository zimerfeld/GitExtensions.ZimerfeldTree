---
tipo: conhecimento
criado: 2026-06-18
atualizado: 2026-06-18
tags: [conhecimento, readme, instalacao, build, uso, gitflow, hierarquia, i18n]
fonte: README.md
versao: 1.0.337
---

# README — Instalação, Uso e Build

> Espelho fiel do `README.md` da raiz do repositório (e das variantes `README.en-US.md` / `README.pt-BR.md`).
> Nota de projeto: [[GitExtensions.ZimerfeldTree]]. Fluxos detalhados em [[Interface ZimerfeldTree — botões e fluxos]], [[Interface GitFlow — botões e fluxos]] e [[Interface Restore — botões e fluxos]].
> O `build.ps1` carimba versão + data nos READMEs **e nesta nota** (frontmatter `versao:`/`atualizado:`) a cada build — reespelhar o corpo quando o README mudar de forma significativa.

Versão atual: **1.0.337**

Plugin para **[GitExtensions](https://gitextensions.github.io/)** que exibe as branches do repositório **hierarquicamente em árvore** (mostrando branches filhas) em vez da lista plana padrão, e disponibiliza o uso da metodologia **GitFlow** de maneira visual muito fácil, intuitiva e agradável de aplicar em projetos de qualquer tamanho.

## ✨ Funcionalidades em alto nível
- **Árvore hierárquica de branches** — seções **LOCAL**, **REMOTES** e **TAGS** combinando ancestralidade real de commits com agrupamento por caminho `/`; branch atual em negrito, contadores ao vivo e filtro em tempo real.
- **GitFlow num clique** — start/publish/track/update/finish para feature, release, hotfix, bugfix e support, com hierarquia flexível que permite até *feature filha de feature* (o finish cascateia até a `develop`).
- **Pull / Push / Commit à mão** — botões com ícones de seta (↓ azul / ↑ verde) e contadores adiante/atrás, verificação do remoto em segundo plano ao abrir e um aviso que **bloqueia o push quando a branch está atrás**.
- **Restore — central de "voltar no tempo"** — janela dedicada reunindo todas as formas seguras de recuperar ou desfazer histórico: restaurar arquivo/árvore/tag, cherry-pick, **reverter**, criar branch/tag a partir de qualquer commit, **recuperação via reflog**, descartar mudanças locais e um rebase avançado para remover um commit — cada um com explicação embutida e orientações de trabalho em equipe.
- **Localizado (Inglês / Português)** — cada janela escolhe seu idioma de forma independente e o memoriza.
- **Carregamento assíncrono** — a janela abre imediatamente com overlay de progresso (0→100%) enquanto os dados são lidos em background; o construtor não faz git.
- **Seleção múltipla por checkbox** + **Modo Developer** que protege `main`/`master`/`develop` da exclusão quando desligado.

## 🔀 GitFlow → git puro

O plugin executa **apenas git nativo** — não depende do binário `git-flow` instalado. Cada botão da janela GitFlow dispara a sequência de comandos git equivalente (start, publish, track, update e finish para cada tipo de branch). Detalhes em [[Interface GitFlow — botões e fluxos]].

### Hierarquia flexível — feature filha de feature
O GitFlow clássico não prevê feature filha de feature (todas as `feature/*` derivam de `develop` e são irmãs). O **ZimerfeldTree GitFlow** permite que uma `feature/*` derive de `develop` **ou de outra `feature/*`** acima dela (via *based on:* no Start); nesse caso o *finish feature* **cascateia** as mudanças até o nó pai sucessivamente, até chegar em `develop`.

## ⛔ Limitações de hierarquia
- **Agrupamento por nome (`/`), não por parentesco de commits** para o eixo de pastas — `master` e `develop` aparecem como irmãos.
- **Branch real não pode ser nó-pai de outra branch** — se `feature/login` existe, criar `feature/login/oauth` falha (o ref seria arquivo **e** diretório). Solução: nomes irmãos ou agrupador sem branch real.
- **Duas branches no exato mesmo commit não formam pai-filho** — solução automática: commit vazio no Start com *based on*. Ver [[Hierarquia de branches — branches no mesmo commit]].

## 🔌 Dependências

### Obrigatórias para uso
| Programa | Versão mínima | Função |
|----------|---------------|--------|
| **Git for Windows** | qualquer | Executa todos os comandos git (escolher "Git from command line and also from 3rd-party software" no PATH). |
| **GitExtensions** | 4.x (.NET 9) | App host que carrega o plugin; fornece diálogos nativos de Commit/Push/Pull. |
| **Plugin ZimerfeldTree** | — | A DLL em `C:\Program Files\GitExtensions\Plugins\`. |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) é **incompatível** — o plugin requer `net9.0-windows`.

### Condicional — build / desenvolvimento
| Programa | Função |
|----------|--------|
| **.NET SDK 9** | Compilar `net9.0-windows` |
| **NuGet CLI** | Gerar `.nupkg` (usado por `build.ps1`) |

Ver também [[Dependências do ZimerfeldTree]].

## 📦 Instalação
**Opção A — PowerShell (como Administrador):**
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

**Opção B — Manual:** copie `GitExtensions.Plugins.ZimerfeldTree.dll` para `C:\Program Files\GitExtensions\Plugins\` e reinicie o GitExtensions.

## 🗑️ Desinstalação
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\uninstall.ps1
```
A remoção da DLL não afeta nenhuma outra parte do GitExtensions.

## 🛠️ Build e versionamento
A cada execução do `build.ps1`, o script:
1. Lê a versão atual do `.nuspec` e detecta mudanças (fontes + `*.md`) vs. o último `.nupkg`.
2. Calcula a nova versão (incrementa o `build` em +1 → `major.minor.build`).
3. Escreve a nova versão e data **primeiro nos docs** (READMEs + cofre Obsidian).
4. Dá o _bump_ no `.nuspec` e no `.csproj`.
5. Compila em Release.
6. Copia a DLL para `C:\Program Files\GitExtensions\Plugins\` *(requer Admin)* e para `tools\net9.0-windows\`.
7. Gera `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` e remove `.nupkg` de versões anteriores.

```powershell
cd C:\GitExtensions\ZimerfeldTree
.\build.ps1
```

**Deploy rápido (sem incrementar versão):**
```powershell
.\tools\update-dll.ps1
```

Ver [[Versionamento e Build]].

## 🤝 Plugins relacionados
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — gera automaticamente a mensagem de commit (Conventional Commits) resumindo os arquivos staged. Por **zimerfeld**.

## 💜 Apoie o projeto
Ajude a manter este projeto sempre atualizado: **[GitHub Sponsors → zimerfeld](https://github.com/sponsors/zimerfeld)** · **[Ko-fi → Buy me a coffee ☕](https://ko-fi.com/C0D621FCGD)**.

## 📄 Licença
[MIT](LICENSE.txt)

## 🔗 Relacionado
- [[GitExtensions.ZimerfeldTree]]
- [[Visão Geral]]
- [[Versionamento e Build]]
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[Interface GitFlow — botões e fluxos]]
- [[Interface Restore — botões e fluxos]]
- [[Dependências do ZimerfeldTree]]
- [[🔑 Fatos-Chave]]
