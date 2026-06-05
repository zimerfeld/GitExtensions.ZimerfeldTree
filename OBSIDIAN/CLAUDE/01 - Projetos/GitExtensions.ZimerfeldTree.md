---
tipo: projeto
criado: 2026-06-01
atualizado: 2026-06-05 (git-flow CLI removido)
tags: [projeto, csharp, gitextensions, plugin, winforms]
status: ativo
linguagem: C#
versao: 1.0.170
repo: C:\GitExtensions\ZimerfeldTree
---

# 🌳 GitExtensions.ZimerfeldTree

## 🎯 Objetivo
Plugin para **[GitExtensions](https://gitextensions.github.io/)** que exibe as branches do repositório **hierarquicamente** em árvore (mostrando branches filhas), em vez da lista plana padrão. Tem ícone próprio "Árvore da Vida" desenhado em GDI+.

## 📂 Estrutura do repositório
```
C:\NUGET\ZimerfeldTree\
├─ src\GitExtensions.ZimerfeldTree\   # código do plugin (.csproj)
├─ tools\                             # nuget.exe, install/uninstall/update .ps1
│   └─ net9.0-windows\               # saída do build (DLL)
├─ OBSIDIAN\CLAUDE\                   # 🧠 este cofre de memória
├─ build.ps1                          # build + empacota nupkg, gerencia versão
├─ README.md                          # documentação rica (~280 linhas)
├─ GitFlowFix.txt                     # fix das chaves git config do git flow CLI
└─ GitExtensions.ZimerfeldTree.1.0.100.nupkg
```

## ⚙️ Stack técnica
- **Linguagem:** C# (`net9.0-windows`), `Nullable` + `ImplicitUsings` habilitados, `LangVersion=latest`
- **UI:** WinForms (`UseWindowsForms`)
- **Tipo de saída:** `Library` (DLL carregada pelo GitExtensions, não exe)
- **AssemblyName:** `GitExtensions.Plugins.ZimerfeldTree`
- **Namespace raiz:** `GitExtensions.ZimerfeldTree`
- **Plugin model:** MEF (`System.ComponentModel.Composition`)
- **Referências externas** (de `C:\Program Files\GitExtensions\`, `Private=false`, não copiadas):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`

## 📄 Arquivos-fonte (`src\GitExtensions.ZimerfeldTree\`)
| Arquivo | Linhas | Papel |
|---------|-------:|-------|
| `BranchHierarchyForm.cs` | ~2000 | Janela principal não-modal (a maior parte da UI) |
| `BranchHierarchyService.cs` | ~706 | Executa comandos git e parseia a saída |
| `GitFlowForm.cs` | ~703 | Janela modal que dirige comandos `git flow` |
| `RestoreForm.cs` | ~523 | Janela modal para restore de arquivo, cherry-pick e reset de branch |
| `NodeIcons.cs` | ~381 | Ícones 16×16 GDI+ gerados em runtime (ImageList) |
| `ZimerfeldTreePlugin.cs` | ~234 | Entry point MEF do plugin |
| `TreeOfLifeIcon.cs` | ~147 | Ícone "Árvore da Vida" desenhado em GDI+ |
| `BranchNode.cs` | ~41 | Modelos de dados (enum `BranchType`: Local/Remote/Tag) |
| `*.nuspec` | — | Manifesto NuGet (lido pelo build.ps1) |

### 🖼️ Resources (`src\GitExtensions.ZimerfeldTree\Resources\`)
| Grupo | Arquivos | Uso |
|-------|----------|-----|
| Seções da árvore | `local.png`, `remotes.png`, `tags.png` | Cabeçalhos LOCAL / REMOTES / TAGS |
| Nós de branch | `folha.png`, `master.png`, `feature.png`, `release.png`, `develop.png`, `origin.png` | Ícones por tipo de branch |
| Tag | `tag.png` | Nó de tag |
| Menu de contexto | `ctx-checkout.png`, `ctx-collapse.png`, `ctx-commit.png`, `ctx-delete.png`, `ctx-expand.png`, `ctx-gitflow.png`, `ctx-merge.png`, `ctx-new-branch.png`, `ctx-rebase.png`, `ctx-refresh.png`, `ctx-rename.png` | Ícones do menu de contexto da árvore |

## ✨ Funcionalidades principais
- Janela **não-modal**, dependente do GitExtensions, abre **centralizada** e redimensionável
- Árvore em 3 seções fixas: **LOCAL**, **REMOTES**, **TAGS**
- LOCAL/REMOTES combinam **ancestralidade real** (parentesco por commits / GitFlow) **+ agrupamento por caminho** (`/`). Ex.: `feature/teste` → pasta `feature` → folha `teste`
- **Carregamento assíncrono** com overlay de progresso (0→100%), lista de passos acumulativa, botão Cancelar, formulário bloqueado durante load
- **Hierarquia otimizada:** um único `git log --all` constrói o grafo de commits em memória, pais via BFS → **O(commits)** em vez de O(N²×subprocesso)
- Seletor de **Working Directory** (combo lido de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) e branch atual em negrito
- Integração **Git Flow** (ver [[git flow - chaves de config (CLI)]])
- **Checkout de TAG com destaque visual** — detecta `HEAD` apontando para tag via `git describe --exact-match --tags HEAD`; tag aparece com `[colchetes]`, negrito e cor de destaque (igual a branch local em checkout)
- **Checkout de branch Origin — branch local já existente** — ao tentar `git checkout -b <local> --track <remota>` e a branch local já existir, exibe diálogo com 3 opções: _Reset local_ / _Create custom name_ / _Checkout detached_; replica comportamento nativo do GitExtensions
- **Filtro do pseudo-nó `(HEAD detached at …)`** — `git branch --format=%(refname:short)` emite essa entrada em detached HEAD; é filtrada antes de popular a seção LOCAL, evitando erro `pathspec did not match` ao tentar checkout
- **Git Flow sem dependência de CLI** — todos os botões da janela GitFlow (Start, Publish, Track, Update, Finish) executam sequências de **git puro**; o binário `git-flow` não precisa estar instalado (ver [[#🔄 Comandos GitFlow → git puro]])
- **Restore / Cherry-Pick / Reset** (`RestoreForm`) — janela modal acessível via menu de contexto; permite restaurar um arquivo do estado de um commit (`git checkout <hash> -- <arquivo>`), aplicar cherry-pick e resetar uma branch (--mixed / --soft / --hard); persiste os últimos valores usados em `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`

## 🔄 Comandos GitFlow → git puro

O plugin executa **apenas git nativo** — não depende do binário `git-flow` instalado.
Cada botão da janela GitFlow dispara a sequência abaixo:

### Start
| Tipo | Comando git |
|------|-------------|
| `feature`, `bugfix`, `release` | `git checkout -b <prefixo><nome> develop` |
| `hotfix`, `support` | `git checkout -b <prefixo><nome> main` |
| qualquer (based on marcado) | `git checkout -b <prefixo><nome> <base escolhida>` |

### Publish
```
git push --set-upstream <remote> <prefixo><nome>
```

### Track
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout -b <prefixo><nome> --track <remote>/<prefixo><nome>
```

### Update
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout <prefixo><nome>
git merge <remote>/<pai>                               # (ou git merge <pai> se No fetch)
```
> Pai = `develop` para feature/bugfix/release; `main` para hotfix/support

### Finish — feature / bugfix
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout develop
git merge --no-ff <prefixo><nome>
git branch -d <prefixo><nome>                          # (se Keep desmarcado)
git push <remote> --delete <prefixo><nome>             # (somente se a branch remota existir)
```

### Finish — hotfix
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout main
git merge --no-ff hotfix/<nome>
git tag -a <nome> -m "<nome>"
git checkout develop
git merge --no-ff hotfix/<nome>
git branch -d hotfix/<nome>                            # (se Keep desmarcado)
git push <remote> --delete hotfix/<nome>               # (somente se a branch remota existir)
```

### Finish — release (fluxo completo automático)
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout main
git merge --no-ff release/<nome>
git tag -a <nome> -m "<nome>"
git checkout develop
git merge --no-ff release/<nome>
git branch -d release/<nome>                           # (se Keep desmarcado)
git push <remote> --delete release/<nome>              # (somente se a branch remota existir)
git push <remote> main
git push <remote> develop
git push <remote> refs/tags/<nome>
git checkout develop
```

### Finish — support
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout main
git merge --no-ff support/<nome>
git branch -d support/<nome>                           # (se Keep desmarcado)
git push <remote> --delete support/<nome>              # (somente se a branch remota existir)
```

> **Erros de merge** (conflito): o plugin para e exibe o resultado. O repositório fica em estado "merging" — resolver manualmente com `git merge --abort` ou resolver os conflitos e `git commit`.

## 🛠️ Build / instalação
```powershell
# Build + empacota nupkg (gerencia versão major.minor.BUILD)
.\build.ps1
# Scripts auxiliares em tools\
tools\install.ps1      # instala o plugin
tools\uninstall.ps1    # remove
tools\update-dll.ps1   # atualiza só a DLL
```
> O `build.ps1` incrementa `<version>` no nuspec, sincroniza `<Version>` no csproj, builda em Release e roda `nuget pack`.

## 🐛 Armadilhas conhecidas
> [!warning] MSB3277 (WindowsBase)
> DLLs do GitExtensions puxam WindowsBase 8.0 enquanto o ref pack net9 fornece 4.0. O runtime resolve a correta em load time → o csproj **rebaixa MSB3277 a mensagem** (`MSBuildWarningsAsMessages`). É benigno.

> [!warning] Git Flow mostrando "Init Gitflow"
> O GitExtensions grava config no formato interno dele, mas o plugin usa o **git flow CLI** que espera outras chaves. Solução em [[git flow - chaves de config (CLI)]].

## 🔢 Versionamento
- Versão atual: **1.0.170** (README + csproj + nuspec em sincronia)
- Esquema: `major.minor.BUILD`, gerenciado pelo `build.ps1`
- ⚠️ Manter csproj e nuspec em sincronia

## 🎨 Ícones (NodeIcons.cs)
- Ícones 16×16 gerados em runtime via GDI+. Índices em `NodeIcons`: 0–4 genéricos, 5–7 seções, 8–14 GitFlow.
- **Develop (índice 9)** agora usa **imagem PNG embutida** `Resources\develop.png` via `LoadEmbedded(...)`, com fallback para `Wrench()`. Para trocar: substituir o PNG e rebuildar. Ver [[2026-06-01 - Ícone customizado do develop]].

## 📜 Histórico de sessões
- [[2026-06-01 - Criação do cofre de neurônios]] — mapeamento inicial do projeto
- [[2026-06-01 - Ícone customizado do develop]] — develop passa a usar PNG embutido (aguardando arquivo)
- [[2026-06-02 - Checkout TAG, Origin e HEAD detached]] — destaque visual de TAG em checkout, diálogo "branch já existe" para Origin, filtro do pseudo-nó `(HEAD detached at …)` no LOCAL

## 🔗 Relacionado
- [[Plugin MEF para GitExtensions]]
- [[git flow - chaves de config (CLI)]]
- [[🔑 Fatos-Chave]]
