---
tipo: projeto
criado: 2026-06-01
atualizado: 2026-06-01
tags: [projeto, csharp, gitextensions, plugin, winforms]
status: ativo
linguagem: C#
versao: 1.0.99
repo: C:\NUGET\ZimerfeldTree
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
| `BranchHierarchyForm.cs` | ~1700 | Janela principal não-modal (a maior parte da UI) |
| `BranchHierarchyService.cs` | ~738 | Executa comandos git e parseia a saída |
| `GitFlowForm.cs` | ~707 | Janela modal que dirige comandos `git flow` |
| `NodeIcons.cs` | ~407 | Ícones 16×16 GDI+ gerados em runtime (ImageList) |
| `TreeOfLifeIcon.cs` | ~181 | Ícone "Árvore da Vida" desenhado em GDI+ |
| `ZimerfeldTreePlugin.cs` | ~131 | Entry point MEF do plugin |
| `BranchNode.cs` | ~51 | Modelos de dados (enum `BranchType`: Local/Remote/Tag) |
| `*.nuspec` | 84 | Manifesto NuGet (lido pelo build.ps1) |

## ✨ Funcionalidades principais
- Janela **não-modal**, independente do GitExtensions, abre **centralizada** e redimensionável
- Árvore em 3 seções fixas: **LOCAL**, **REMOTES**, **TAGS**
- LOCAL/REMOTES combinam **ancestralidade real** (parentesco por commits / GitFlow) **+ agrupamento por caminho** (`/`). Ex.: `feature/teste` → pasta `feature` → folha `teste`
- **Carregamento assíncrono** com overlay de progresso (0→100%), lista de passos acumulativa, botão Cancelar, formulário bloqueado durante load
- **Hierarquia otimizada:** um único `git log --all` constrói o grafo de commits em memória, pais via BFS → **O(commits)** em vez de O(N²×subprocesso)
- Seletor de **Working Directory** (combo lido de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) e branch atual em negrito
- Integração **Git Flow** (ver [[git flow - chaves de config (CLI)]])

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
- Versão atual no README: **1.0.99**; csproj `<Version>1.0.100`, `AssemblyVersion 1.0.99.0`
- Esquema: `major.minor.BUILD`, gerenciado pelo `build.ps1`
- ⚠️ Manter csproj e nuspec em sincronia

## 🎨 Ícones (NodeIcons.cs)
- Ícones 16×16 gerados em runtime via GDI+. Índices em `NodeIcons`: 0–4 genéricos, 5–7 seções, 8–14 GitFlow.
- **Develop (índice 9)** agora usa **imagem PNG embutida** `Resources\develop.png` via `LoadEmbedded(...)`, com fallback para `Wrench()`. Para trocar: substituir o PNG e rebuildar. Ver [[2026-06-01 - Ícone customizado do develop]].

## 📜 Histórico de sessões
- [[2026-06-01 - Criação do cofre de neurônios]] — mapeamento inicial do projeto
- [[2026-06-01 - Ícone customizado do develop]] — develop passa a usar PNG embutido (aguardando arquivo)

## 🔗 Relacionado
- [[Plugin MEF para GitExtensions]]
- [[git flow - chaves de config (CLI)]]
- [[🔑 Fatos-Chave]]
