---
tipo: projeto
criado: 2026-06-01
atualizado: 2026-06-09 (1.0.276: tags excluídas na árvore também são removidas do remoto; link About corrigido + seções Show Debug/Modo Developer; títulos das janelas padronizados com prefixo "ZimerfeldTree - " + nome da janela: "ZimerfeldTree - BranchHierarchy" / "ZimerfeldTree - GitFlow" / "ZimerfeldTree - Restore")
tags: [projeto, csharp, gitextensions, plugin, winforms]
status: ativo
linguagem: C#
versao: 1.0.276
repo: C:\GitExtensions\ZimerfeldTree
---

# 🌳 GitExtensions.ZimerfeldTree

> [!info] Esta nota espelha o `README.md` do repositório
> O conteúdo do README (funcionalidades, dependências, instalação, estrutura e limitações) vive aqui no cofre. Os **fluxos detalhados de cada janela** estão em [[Interface ZimerfeldTree — botões e fluxos]], [[Interface GitFlow — botões e fluxos]] e [[Interface Restore — botões e fluxos]].

## 🎯 Objetivo
Plugin para **[GitExtensions](https://gitextensions.github.io/)** que exibe as branches do repositório **hierarquicamente** em árvore (mostrando branches filhas), em vez da lista plana padrão. Tem ícone próprio "Árvore da Vida" desenhado/embutido (GDI+ / `Resources/ico.png`).

## 📂 Estrutura do projeto
```
C:\GitExtensions\ZimerfeldTree\
├─ src\GitExtensions.ZimerfeldTree\        # código do plugin
│   ├─ ZimerfeldTreePlugin.cs              # ponto de entrada MEF (IGitPlugin)
│   ├─ BranchHierarchyForm.cs              # janela principal: árvore hierárquica de branches
│   ├─ GitFlowForm.cs                      # janela Git Flow: start/publish/track/update/finish
│   ├─ RestoreForm.cs                      # janela Restore: restore de arquivo, cherry-pick, reset
│   ├─ BranchHierarchyService.cs           # lógica git: coleta, hierarquia, Git Flow
│   ├─ BranchNode.cs                       # modelos: classe BranchInfo + enum BranchType
│   ├─ NodeIcons.cs                        # ícones 16×16 da árvore (GDI+ + PNGs embutidos)
│   ├─ PluginIcon.cs                       # ícone do plugin/janela (Resources/ico.png)
│   ├─ Resources\                          # PNGs embutidos (ícones de nós, menu e plugin)
│   ├─ GitExtensions.ZimerfeldTree.csproj
│   └─ GitExtensions.ZimerfeldTree.nuspec  # metadados do pacote NuGet
├─ build.ps1                               # build + versionamento + deploy
├─ README.md                               # documentação rica
└─ OBSIDIAN\CLAUDE\                        # 🧠 este cofre de memória
```

## ⚙️ Stack técnica
- **Linguagem:** C# (`net9.0-windows`), `Nullable` + `ImplicitUsings`, `LangVersion=latest`
- **UI:** WinForms (`UseWindowsForms`)
- **Tipo de saída:** `Library` (DLL carregada pelo GitExtensions, não exe)
- **AssemblyName:** `GitExtensions.Plugins.ZimerfeldTree`
- **Namespace raiz:** `GitExtensions.ZimerfeldTree`
- **Plugin model:** MEF (`System.ComponentModel.Composition`) — ver [[Plugin MEF para GitExtensions]]
- **Referências externas** (de `C:\Program Files\GitExtensions\`, `Private=false`, não copiadas):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`

## 📄 Arquivos-fonte (`src\GitExtensions.ZimerfeldTree\`)
| Arquivo | Linhas | Papel |
|---------|-------:|-------|
| `BranchHierarchyForm.cs` | ~2066 | Janela principal não-modal (a maior parte da UI) |
| `BranchHierarchyService.cs` | ~831 | Executa comandos git e parseia a saída |
| `GitFlowForm.cs` | ~758 | Janela modal que dirige comandos `git flow` (git puro) |
| `RestoreForm.cs` | ~534 | Janela modal: restore de arquivo, cherry-pick, reset de branch |
| `NodeIcons.cs` | ~381 | Ícones 16×16 GDI+ + PNGs embutidos (ImageList) |
| `ZimerfeldTreePlugin.cs` | ~238 | Entry point MEF do plugin (IGitPlugin) |
| `BranchNode.cs` | ~41 | Modelos: classe `BranchInfo` + enum `BranchType` (Local/Remote/Tag) |
| `PluginIcon.cs` | ~33 | Ícone do plugin/janela (`Resources/ico.png`), carregado 1× e cacheado |
| `*.nuspec` / `*.csproj` | — | Manifestos NuGet/MSBuild (lidos pelo `build.ps1`) |

### 🖼️ Resources (`src\GitExtensions.ZimerfeldTree\Resources\`)
| Grupo | Arquivos | Uso |
|-------|----------|-----|
| Plugin/janela | `ico.png` | Ícone "Árvore da Vida" (menu Plugins + barra de título) |
| Seções da árvore | `local.png`, `remotes.png`, `tags.png` | Cabeçalhos LOCAL / REMOTES / TAGS |
| Nós de branch | `master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png` | Ícones por tipo de branch GitFlow |
| Remote / tag | `origin.png`, `remote-branch.png`, `tag.png` | Grupo de remote (foguete), branch remota, tag |
| Menu de contexto | `ctx-checkout.png`, `ctx-collapse.png`, `ctx-commit.png`, `ctx-delete.png`, `ctx-expand.png`, `ctx-gitflow.png`, `ctx-merge.png`, `ctx-new-branch.png`, `ctx-rebase.png`, `ctx-refresh.png`, `ctx-rename.png`, `ctx-restore.png` | Ícones do menu de contexto da árvore |

> Cada `<EmbeddedResource>` é **condicional à existência do arquivo** (`Condition="Exists(...)"`). Em runtime, `NodeIcons.LoadEmbedded` lê o recurso por `GitExtensions.ZimerfeldTree.Resources.<arquivo>` e redimensiona para 16×16. Se ausente/ilegível, cai no **glifo GDI+ de reserva** — o build nunca quebra por falta da imagem.

## ✨ Funcionalidades principais
- Janela **não-modal**, singleton por sessão, abre **centralizada** e redimensionável (`Sizable`), independente do GitExtensions. Título da barra: **`ZimerfeldTree - BranchHierarchy`** (auxiliares: `ZimerfeldTree - GitFlow`, `ZimerfeldTree - Restore`) — o prefixo **ZimerfeldTree** é sempre mantido, seguido do nome específico da janela. `BranchHierarchyForm` é só o nome interno da classe C#
- Árvore em 3 seções fixas: **LOCAL**, **REMOTES**, **TAGS**, com contadores `(N)` e status bar `Local: N | Remoto: N | Tags: N`
- LOCAL/REMOTES combinam **ancestralidade real** (parentesco por commits / GitFlow) **+ agrupamento por caminho** (`/`). Ex.: `feature/teste` → pasta `feature` → folha `teste`
- **Carregamento assíncrono** com overlay de progresso (0→100%), lista acumulativa dos 8 passos, botão Cancelar, formulário bloqueado durante o load; overlay fecha após 1 s no "Concluído."
- **Hierarquia otimizada:** um único `git log --all` constrói o grafo de commits em memória, pais via BFS → **O(commits)** em vez de O(N²×subprocesso)
- **Overlay só na 1ª exibição e nas recargas explícitas** — não aparece ao reativar após fechar GitFlow/Restore (árvore já atualizada ao vivo) nem no eco do próprio `NotifyRepoChanged`
- Seletor de **Working Directory** (combo lido de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) e **branch atual em negrito** + cor de destaque
- **Filtro em tempo real** em todas as seções (substring case-insensitive), preservando nós-pai com filhos correspondentes
- **Botões Pull / Push / Commit / Excluir / GitFlow / Restore** acima da árvore (quando há branch em checkout); contadores `↓N` / `↑N` / `(N)`
- **Seleção múltipla por checkbox** — cada branch (local/remota) e tag tem checkbox (seções e pastas não); marcar 2+ habilita exclusão em lote. O botão **Excluir** muda para `Excluir (N)` e o menu de contexto reduz para **Excluir + Atualizar**
- **Checkbox "Modo Developer"** (ao lado de Show Debug) — **desligado (padrão):** `main`/`master`/`develop` ficam **protegidas**, com checkbox bloqueado (não podem ser marcadas nem excluídas); **ligado:** libera a marcação/exclusão dessas branches específicas. Desativar o modo **desmarca automaticamente** qualquer main/master/develop marcada. Estado persistido em `ZimerfeldTree.uisettings.json`
- **Foco automático após Commit** — a janela retoma o foco e atualiza a árvore ao fechar a janela de Commit
- **Checkbox "Show Debug"** — tooltips `TYPE:`/`ID:` em todos os controles (e Handle da janela); estado persistido em `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`
- **Persistência de estado da árvore** (expande/recolhe) por Working Directory em `ZimerfeldTree.treestate.json` — caminho estável por nó (ex.: `LOCAL|master|develop|feature`), debounce 500 ms + save no fechamento, restaurado no `Shown` da 1ª abertura
- **Organização automática como GitFlow** — detecta hierarquia fora do padrão e auto-organiza; botão "Restaurar hierarquia real" / "Organizar como GitFlow"
- **Atualização automática** em checkout, troca de repositório, init/reabertura; botão **Atualizar** manual
- **Menu de contexto** com ícones embutidos (Commit, Checkout, Nova branch, Merge, Rebase, Renomear, Excluir, GitFlow…, Restore…, Expandir/Recolher, Atualizar)
- **Botão GitFlow Initialize** — aplica de uma vez as chaves `gitflow.*` padrão (ver [[git flow - chaves de config (CLI)]])
- **Restore / Cherry-Pick / Reset** (`RestoreForm`) — janela modal de restauração de histórico

> Detalhes controle-a-controle: [[Interface ZimerfeldTree — botões e fluxos]] · [[Interface GitFlow — botões e fluxos]] · [[Interface Restore — botões e fluxos]].

![[ScreenshotGitFlow.png]]
![[ScreenshotRestore.png]]

## 🔄 Comandos GitFlow → git puro

O plugin executa **apenas git nativo** — não depende do binário `git-flow` instalado.
Cada botão da janela GitFlow dispara a sequência abaixo:

### Start
| Tipo | Comando git |
|------|-------------|
| `feature`, `bugfix`, `release` | `git checkout -b <prefixo><nome> develop` |
| `hotfix`, `support` | `git checkout -b <prefixo><nome> main` |
| qualquer (based on marcado) | `git checkout -b <prefixo><nome> <base escolhida>` |

> **based on:** permite feature-filha-de-feature; nesse caso o plugin executa também `git commit --allow-empty -m "chore: start <prefixo><nome>"` para a hierarquia ficar visível (ver Limitações).
> **Nome padrão de release:** ao escolher tipo `release`, o nome é pré-preenchido com `yyyyMMddHHmm` (só se o campo estiver vazio).

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
git push <remote> --delete hotfix/<nome>              # (somente se a branch remota existir)
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
git push <remote> --delete release/<nome>             # (somente se a branch remota existir)
git push <remote> main
git push <remote> develop
git push <remote> refs/tags/<nome>
git checkout develop
```
> Ao concluir, a seção **TAGS** é expandida e o foco vai para a tag criada. Remote = `origin` (ou o primeiro configurado).

### Finish — support
```
git fetch <remote>                                     # (se No fetch desmarcado)
git checkout main
git merge --no-ff support/<nome>
git branch -d support/<nome>                           # (se Keep desmarcado)
git push <remote> --delete support/<nome>             # (somente se a branch remota existir)
```

> **Erros de merge** (conflito): o plugin para e exibe o resultado. O repositório fica em estado "merging" — resolver com `git merge --abort` ou resolver os conflitos e `git commit`.

## 🔌 Dependências

### Obrigatórias para uso
| Programa | Versão mínima | Função |
|----------|---------------|--------|
| **Git for Windows** | qualquer ([download](https://git-scm.com/download/win)) | Executa todos os comandos git. Na tela *"Adjusting your PATH"* escolher **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | App host que carrega o plugin; fornece diálogos nativos de Commit/Push/Pull. Instalador `.msi` instala o .NET 9 Desktop Runtime |
| **Plugin ZimerfeldTree** | — | A DLL em `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) é **incompatível** — o plugin requer `net9.0-windows`.

### Condicional — build / desenvolvimento
| Programa | Função |
|----------|--------|
| **.NET SDK 9** ([download](https://dotnet.microsoft.com/download/dotnet/9.0)) | Compilar `net9.0-windows` |
| **NuGet CLI** ([download](https://www.nuget.org/downloads)) | Gerar `.nupkg` (usado por `build.ps1`) |

Ver também [[Dependências do ZimerfeldTree]].

## 🛠️ Build / instalação
```powershell
# Build + empacota nupkg (gerencia versão major.minor.BUILD). Como Admin também copia o DLL.
.\build.ps1
# Scripts auxiliares em tools\
tools\install.ps1      # instala o plugin
tools\uninstall.ps1    # remove
tools\update-dll.ps1   # atualiza só a DLL
```
O `build.ps1`: (1) lê e incrementa `<version>` no nuspec; (2) sincroniza `<Version>` no csproj; (3) atualiza `README.md` e `FUNCIONALIDADES.md`; (4) builda em Release; (5) se Admin, copia o DLL para `C:\Program Files\GitExtensions\Plugins\`; (6) roda `nuget pack`.

**Instalação manual:** copiar `GitExtensions.Plugins.ZimerfeldTree.dll` para `C:\Program Files\GitExtensions\Plugins\` e reiniciar o GitExtensions.
**Desinstalação:** deletar essa DLL (não afeta o GitExtensions).

## ⛔ Limitações de hierarquia de branches
- **Agrupamento é por nome (`/`), não por parentesco de commits** para o eixo de pastas — `master` e `develop` aparecem como irmãos; para aninhar por nome use `/`.
- **Branch real não pode ser nó-pai de outra branch** — se `feature/login` existe, criar `feature/login/oauth` falha (`cannot lock ref … exists`), pois o ref seria arquivo **e** diretório. Solução: nomes irmãos (`feature/login-oauth`) ou agrupador sem branch real (`feature/login/base` + `feature/login/oauth`).
- **GitFlow não prevê feature-filha-de-feature** — todas as `feature/*` derivam de `develop` e são irmãs.
- **Duas branches no exato mesmo commit não formam pai-filho** — o BFS de ancestralidade nunca encontra uma como pai da outra; ambas viram raízes. Solução automática: commit vazio no Start com **based on**. Detalhe em [[Hierarquia de branches — branches no mesmo commit]].

## 🐛 Armadilhas conhecidas
> [!warning] MSB3277 (WindowsBase)
> DLLs do GitExtensions puxam WindowsBase 8.0 enquanto o ref pack net9 fornece 4.0. O runtime resolve a correta em load time → o csproj **rebaixa MSB3277 a mensagem** (`MSBuildWarningsAsMessages`). É benigno.

> [!warning] Git Flow mostrando "Init Gitflow"
> O GitExtensions grava config no formato interno dele, mas o git flow CLI espera outras chaves. Solução em [[git flow - chaves de config (CLI)]].

## 🔢 Versionamento
- Versão atual: **1.0.276** (README + csproj + nuspec em sincronia)
- Esquema: `major.minor.BUILD`, gerenciado pelo `build.ps1`
- ⚠️ Manter csproj e nuspec em sincronia

## 🎨 Ícones (NodeIcons.cs)
- Ícones 16×16 gerados em runtime via GDI+, com vários **PNGs embutidos** e fallback desenhado. Índices em `NodeIcons`: 0–4 genéricos, 5–7 seções, 8–15 GitFlow/folha.
- **Grupo de remote (`origin`)** usa `Resources\origin.png` (foguete) via `NodeIcons.Remote` — mapeado em `GetFolderIconIndex`.
- **Develop (índice 9)** usa `Resources\develop.png`, fallback `Wrench()`. Ver [[2026-06-01 - Ícone customizado do develop]].

## 🔗 Plugins integrados (mesmo autor)
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — gera automaticamente a mensagem de commit (Conventional Commits) resumindo os arquivos staged.

## 📜 Histórico de sessões
- [[2026-06-01 - Criação do cofre de neurônios]] — mapeamento inicial do projeto
- [[2026-06-01 - Ícone customizado do develop]] — develop passa a usar PNG embutido
- [[2026-06-02 - Checkout TAG, Origin e HEAD detached]] — destaque visual de TAG, diálogo "branch já existe" para Origin, filtro do pseudo-nó `(HEAD detached at …)`
- [[2026-06-06 - Hierarquia branches mesmo commit, commit automático no Start]]
- [[2026-06-06 - Push fix, double refresh, Voltar Versão menu]]
- [[2026-06-07 - Refresh, overlay, eco e botão Restore]]

## 🔗 Relacionado
- [[Interface ZimerfeldTree — botões e fluxos]]
- [[Interface GitFlow — botões e fluxos]]
- [[Interface Restore — botões e fluxos]]
- [[Plugin MEF para GitExtensions]]
- [[git flow - chaves de config (CLI)]]
- [[Dependências do ZimerfeldTree]]
- [[🔑 Fatos-Chave]]
