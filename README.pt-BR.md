# GitExtensions.ZimerfeldTree

![Icone](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/icon-128.png)

[![NuGet version](https://img.shields.io/nuget/v/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=NuGet)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/) &nbsp; [![NuGet downloads](https://img.shields.io/nuget/dt/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=Downloads)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/)

Este plugin é construído e mantido no meu tempo livre. Se ele te poupa tempo gerenciando branches, um patrocínio ajuda a mantê-lo atualizado para as novas versões do GitExtensions. 💜

[![GitHub Sponsor](https://img.shields.io/badge/Sponsor-zimerfeld-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white)](https://github.com/sponsors/zimerfeld) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; [![Ko-fi](https://img.shields.io/badge/Ko--fi-Buy%20me%20a%20coffee-FF5E2B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/C0D621FCGD)

**Versão:** 1.0.347  
**Atualizado em:** 2026-06-25

Plugin para [GitExtensions](https://gitextensions.github.io/) que exibe branches **hierarquicamente** em estrutura de árvore, mostrando branches filhas.

![ZimerfeldTree - BranchHierarchy](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBranchHierarchy.png)

[English](README.en-US.md) | [Português](README.pt-BR.md)

[...More information](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree "More information about GitExtensions.ZimerfeldTree package")

---

## Funcionalidades

### Visualização hierárquica de branches

- Janela não-modal que permanece aberta em paralelo ao GitExtensions. O título da barra é **`ZimerfeldTree - BranchHierarchy`** (as janelas auxiliares são `ZimerfeldTree - GitFlow` e `ZimerfeldTree - Restore`) — o prefixo **ZimerfeldTree** é mantido sempre, seguido do nome específico da janela. `BranchHierarchyForm` é apenas o nome interno da classe C# (exibido no tooltip de debug)
- Árvore dividida em três seções fixas: **LOCAL**, **REMOTES** e **TAGS**
- **LOCAL e REMOTES** combinam **ancestralidade** (parentesco real por commits / organização GitFlow) com **agrupamento por caminho** (`/`): dentro de cada nível pai, nomes com `/` viram nós-pasta. Ex.: `feature/teste` aparece como pasta `feature` → folha `teste`, e `release/2026` como `release` → `2026`. Quando `feature/*` é filha de `develop`, fica `develop` → `feature` → `teste`
- **TAGS** também agrupa por `/` (sem ancestralidade)
- LOCAL, REMOTES e TAGS exibe `(nenhuma branch local encontrada)` quando não há branches
- A janela abre **centralizada na tela** (horizontal e vertical)
- Janela de tamanho **fixo** (não redimensionável pelo usuário, `FixedSingle`), com botões **Minimizar** e **Fechar** padrão do Windows; **Maximizar desabilitado** (maximizar redimensionaria a janela)
- A janela é **independente** do GitExtensions: minimizar o GitExtensions não afeta a janela BranchHierarchy
- **Carregamento assíncrono**: ao abrir, a janela exibe o esqueleto imediatamente e depois mostra um **painel de progresso centralizado** ("Carregando dados do repositório") com barra de porcentagem (0→100%) enquanto lê os dados do repositório em background; a árvore é populada apenas ao final
- **Montagem da hierarquia otimizada**: o cálculo de parentesco entre branches usa um único `git log --all` para construir o grafo de commits em memória e determina os pais via BFS — complexidade O(commits) em vez do anterior O(N² × subprocesso), eliminando o gargalo em repositórios com dezenas ou centenas de branches
- **Overlay nas recargas explícitas**: o painel de progresso aparece na **primeira** abertura da janela e nas recargas/mutações — botão Atualizar, checkout, nova branch, merge, rename, delete, GitFlow, Restore, Pull/Push/Commit, mudanças externas genuínas do GitExtensions e troca de repositório. **Não** aparece ao reativar a janela depois de fechar GitFlow/Restore (a árvore já foi atualizada ao vivo), nem no "eco" da própria notificação ao GitExtensions (suprimido), evitando flash desnecessário
- **Lista de passos (somente leitura)**: o overlay exibe uma lista acumulativa de cada etapa executada ("Carregando branches locais…", "Calculando hierarquia…", "Verificando alterações pendentes…", etc.) — cada passo é adicionado à lista conforme é iniciado, permitindo acompanhar o progresso em detalhe. A lista é dimensionada para exibir todos os 8 passos de uma vez, sem barra de rolagem vertical. A contagem de alterações pendentes (`Commit (N)`) é lida **nesse mesmo carregamento em background** e reaproveitada, sem um `git status` extra na thread de UI. Após o último passo ("Concluído."), o overlay permanece visível por **1 segundo** antes de fechar nas recargas, para o usuário conseguir ver a conclusão; na **primeira abertura** o overlay fecha assim que a árvore é populada (sem esse atraso), para a janela terminar de abrir o quanto antes
- **Botão Cancelar no overlay**: permite abortar o carregamento a qualquer momento (o cancelamento ocorre entre as etapas git, preservando os dados anteriores na árvore)
- **Formulário bloqueado durante carregamento**: todos os campos e botões ficam desabilitados enquanto o overlay está ativo e são reativados ao término (ou ao cancelar)
- **Botão "Fechar"** centralizado horizontalmente na parte inferior da janela (atalho: tecla **Esc**)

### Seletor de Working Directory e Branch

- **Linha "Working Directory:"** no topo da janela contém:
  - Label fixo `Working Directory:`
  - **ComboBox** (somente seleção) populado automaticamente com todos os repositórios listados no dashboard do GitExtensions (lido de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) e quando novo repositório é criado
  - Label `Branch: <nome>` mostrando a branch em checkout no repositório exibido
- Selecionar outro repositório no dropdown recarrega a árvore para aquele repositório
- A lista do combo é recarregada automaticamente sempre que o GitExtensions troca de repositório
- A branch atual aparece destacada com **texto em negrito** e cor de seleção do sistema (`[nome]`)
- Seções da árvore mostram contadores: `LOCAL (N)`, `REMOTES (N)`, `TAGS (N)`
- Status bar inferior mostra: `Local: N  |  Remoto: N  |  Tags: N`

### Filtro em tempo real

- Campo de pesquisa filtra branches em todas as seções simultaneamente
- Filtro preserva nós pai que possuem filhos correspondentes

### Botões Pull / Push / Commit / GitFlow / Restore

Exibidos acima da árvore quando há uma branch em checkout:

- **Pull** / **Pull ↓N** — executa `git pull --tags`: traz commits da branch rastreada **e** todas as tags do remoto, garantindo que tags de releases criadas em outras máquinas apareçam na seção TAGS; o botão mostra um **ícone de seta para baixo** (azul) que substitui o antigo caractere `↓`, e `↓N` é a quantidade de commits do remoto ainda não baixados
- **Push** / **Push ↑N** — abre o diálogo nativo de Push do GitExtensions (remote, URL, branch destino e opções avançadas); o botão mostra um **ícone de seta para cima** (verde) que substitui o antigo caractere `↑`, e `↑N` mostra quantos commits locais ainda não foram enviados ao remoto
  - Quando a branch em checkout está **atrás** do remoto (`↓N > 0`), o Push é **bloqueado** por um aviso ("sua branch está N commit(s) atrás — faça Baixar primeiro") que oferece fazer o Baixar na hora, evitando a rejeição `non-fast-forward`
- **Commit** / **Commit (N)** — abre a janela de Commit nativa do GitExtensions; o contador `(N)` só aparece quando há alterações pendentes; sem alterações o botão e o item do menu de contexto mostram apenas `Commit`
- **Verificação do remoto ao abrir** — ao abrir a janela, um `git fetch` da upstream da branch atual roda **em segundo plano** (a abertura permanece rápida e offline-safe) e atualiza os contadores Pull/Push; o label `Branch: <nome>` também ganha o sufixo `↓N` quando há commits a baixar
- Após cada Push, Pull ou Commit (seja pelos botões ou pela janela principal do GitExtensions), a árvore é **atualizada automaticamente** e os contadores dos botões (`↑N`, `↓N`, `(N)`) são recalculados
- **GitFlow** — abre a janela de operações GitFlow; disponível a qualquer momento, independentemente do estado do painel de aviso
- **Restore** — abre a janela **Restore** com três operações de restauração de histórico (ver seção abaixo)
- **Excluir** / **Excluir (N)** — exclui branches/tags selecionados; o texto reflete a quantidade de checkboxes marcados (ver seção abaixo)
- **Ícones nos botões** — cada botão exibe, **antes do texto**, o mesmo ícone usado pela ação correspondente no menu de contexto: **Pull** (`ctx-pull`, seta para baixo), **Push** (`ctx-push`, seta para cima), **Commit** (`ctx-commit`), **Excluir** (`ctx-delete`), **GitFlow** (`ctx-gitflow`), **Restore** (`ctx-restore`) e o botão de atualizar da barra de filtro (`ctx-refresh`, substitui o glifo `↺`). Os botões **GitFlow Initialize** e **Organizar como GitFlow** também usam o ícone `ctx-gitflow`. Se um ícone não puder ser carregado, o botão mantém apenas o texto

### Seleção múltipla e exclusão de branches

- Cada **branch (local e remota) e tag** exibe um **checkbox** à esquerda do nome. Seções (LOCAL/REMOTES/TAGS) e pastas de caminho (feature, release…) **não** têm checkbox e não são marcáveis.
- Marque **um ou mais** checkboxes para selecionar nós para exclusão em lote.
- O botão **Excluir** (acima da árvore, à direita de _Commit_) muda dinamicamente conforme a quantidade marcada: `Excluir` (nenhum) → `Excluir (N)`.
  - **2+ marcados**: exclui todos em lote, com **uma única confirmação** listando os itens.
  - **1 marcado**: exclui esse item.
  - **nenhum marcado**: exclui o **nó selecionado** na árvore.
  - Para branch local não totalmente mesclada, oferece **exclusão forçada**.
  - Ao excluir uma **tag**, ela é removida **localmente** (`git tag -d`) **e do remoto** (`git push <remote> --delete <tag>`); se a tag não existir no remoto, a remoção local ainda é considerada bem-sucedida.
- O **menu de contexto** acompanha: com **2+** checkboxes marcados, mostra apenas **Excluir (N)** e **Atualizar** (o item GitFlow foi removido do menu de contexto).
- Após excluir, a árvore é reconstruída e os checkboxes são limpos.

O fluxo completo de exclusão em lote:

**1. Antes — itens marcados** (botão mostra `Excluir (8)`):

![Antes da exclusão](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBeforeDelete.png)

**2. Confirmação única** listando todos os itens, com a opção **Excluir Remotamente?**:

![Confirmar exclusão](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotConfirmDelete.png)

**3. Durante a exclusão** — overlay de progresso com a lista de passos e o botão **Abortar Operação**:

![Durante a exclusão](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotDuringDelete.png)

**4. Depois** — a árvore é reconstruída já sem os itens excluídos e com os contadores atualizados:

![Depois da exclusão](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotAfterDelete.png)

#### Proteção de branches principais e "Modo Developer"

- As branches **main / master / develop** (locais e remotas) são **protegidas**: por padrão **não podem ser marcadas** para exclusão (nem excluídas pelo nó selecionado).
- O checkbox **Modo Developer**, na borda inferior da janela (ao lado de _Show Debug_), **libera** a marcação/exclusão dessas branches quando ativado.
- Ao **desativar** o Modo Developer, qualquer main/master/develop que estivesse marcada é **desmarcada automaticamente**.
- O estado do **Modo Developer** é **persistido** em `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json` (junto com _Show Debug_) e restaurado na abertura da janela.

### Foco automático após Commit

- Após fechar a janela de Commit do GitExtensions, a janela BranchHierarchy retoma o foco automaticamente
- A árvore é atualizada (reflecting o novo commit) e a janela é trazida ao topo sem ação manual

### Checkbox "Mostrar IDs" (debug de controles)

O checkbox **Show Debug**, localizado na borda inferior esquerda da janela BranchHierarchy, ativa tooltips de identificação em todos os controles do plugin:

- **Linha 1 do tooltip:** `TYPE: <tipo do controle>` — classe WinForms (ex.: `Button`, `TextBox`)
- **Linha 2 do tooltip:** `ID: <nome interno>` — campo Name do controle C#
- **Tooltip da própria janela:** exibe `TYPE: BranchHierarchyForm` e `Handle: 0x<HWND>` (visível ao passar o mouse sobre área livre da janela)
- **GitFlowForm** também exibe seu TYPE e Handle quando Show Debug está ativo
- Funciona nas três janelas: BranchHierarchy, GitFlow e Restore
- **Cada janela persiste e recarrega o próprio estado de Show Debug individualmente** — BranchHierarchy em `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`, GitFlow em `ZimerfeldTree.gitflowsettings.json` e Restore em `ZimerfeldRestore.settings.json`. Na primeira abertura de uma janela auxiliar (sem valor salvo), ela herda o estado da BranchHierarchy
- Útil para desenvolvimento e manutenção do plugin

### Seletor de Idioma

Um dropdown **Idioma** na borda inferior de cada janela troca o texto da UI ao vivo entre **Automático** (segue a cultura do SO), **Inglês** e **Português**:

- **Cada janela persiste e recarrega o próprio idioma individualmente**, então BranchHierarchy, GitFlow e Restore podem ser exibidas em idiomas diferentes ao mesmo tempo. BranchHierarchy é gravado em `%APPDATA%\GitExtensions\ZimerfeldTree.language.json`, GitFlow em `ZimerfeldTree.gitflowsettings.json` e Restore em `ZimerfeldRestore.settings.json`.
- A janela reabre no idioma em que foi deixada por último. Na primeira abertura de uma janela auxiliar (sem valor salvo), ela herda o idioma da BranchHierarchy como padrão.

### Checkbox "Modo Developer"

O checkbox **Modo Developer**, ao lado de _Show Debug_ na borda inferior da janela, controla a proteção das branches principais:

- **Desligado (padrão):** as branches **main** e **develop** (e `master`) são **protegidas** — seus checkboxes ficam **bloqueados**, não podendo ser marcadas nem excluídas (nem pelo nó selecionado). Isso evita a exclusão acidental das branches estruturais do repositório.
- **Ligado:** **libera** a marcação e a exclusão dessas branches específicas, para quem realmente precisa removê-las.
- Ao **desativar** o modo, qualquer main/master/develop que estivesse marcada é **desmarcada automaticamente**.
- O estado é **persistido** em `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json` (junto com _Show Debug_) e restaurado na abertura da janela.

### Persistência de estado da árvore

- O estado de **expansão/recolhimento** de cada nó é **salvo automaticamente** por Working Directory — incluindo os nós principais (LOCAL, REMOTES, TAGS), branches (master, develop…) e pastas (feature, release…)
- Cada nó é identificado por um **caminho estável** (a cadeia de ancestrais, ex.: `LOCAL|master|develop|feature`), que sobrevive a reconstruções da árvore
- O salvamento ocorre ao expandir/recolher (com **debounce de 500 ms**) e ao **fechar a janela**, gravando em `%APPDATA%\GitExtensions\ZimerfeldTree.treestate.json`
- Na **primeira abertura** da janela, o estado salvo é restaurado assim que a árvore é populada (ao final do carregamento assíncrono, sob o overlay), refletindo exatamente como estava na última sessão
- Primeira abertura de um repositório **novo** (sem estado salvo) usa o padrão: LOCAL totalmente expandido, REMOTES e TAGS com apenas a raiz expandida
- Durante filtro ativo, todos os nós são expandidos automaticamente para mostrar os resultados

### Organização automática como GitFlow

- O plugin verifica se a hierarquia real (por parentesco de commits) respeita as regras do GitFlow:
  `master`/`main` na raiz, `develop` filho de `master`, e branches `feature/*`, `release/*` e `hotfix/*` nos pais esperados
- Quando detecta que a hierarquia está **fora da condição de GitFlow**, ele **aplica automaticamente** a organização GitFlow na árvore e exibe o aviso correspondente
- Nesse estado, o botão do painel de aviso mostra **"Restaurar hierarquia real"** — clicar nele volta a exibir a ancestralidade real do git
- A escolha manual do botão é respeitada e só é reavaliada ao trocar de repositório (ou reabrir a janela)

### Atualização automática

- A árvore é recarregada automaticamente ao:
  - Trocar de branch (**checkout**)
  - Trocar de repositório na UI do GitExtensions
  - Inicializar/reabrir um repositório
- Botão **Atualizar** para recarga manual

### Menu de contexto (botão direito)

Cada item possui um ícone 16×16 embutido na DLL (gerado em `Resources/ctx-*.png`):

| Ícone                                                                                                                                                          | Item                    | Disponível para                                                                     |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------- | ----------------------------------------------------------------------------------- |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-pull.png" width="16" height="16">       | Baixar (N)              | Branch local — `N` = commits atrás; faz checkout da branch clicada e então o pull   |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-push.png" width="16" height="16">       | Enviar (N)              | Branch local — `N` = commits à frente; faz checkout da branch clicada e então o push (bloqueado se estiver atrás) |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-commit.png" width="16" height="16">     | Commit (N)              | Sempre — abre a janela de Commit do GitExtensions; `N` = nº de alterações pendentes |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-checkout.png" width="16" height="16">   | Checkout                | Local, remota, tag                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-new-branch.png" width="16" height="16"> | Nova branch daqui…      | Local, tag                                                                          |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-merge.png" width="16" height="16">      | Mesclar na branch atual | Local                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rebase.png" width="16" height="16">     | Rebase na branch atual  | Local                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rename.png" width="16" height="16">     | Renomear…               | Local                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-delete.png" width="16" height="16">     | Excluir…                | Local, remota, tag                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-gitflow.png" width="16" height="16">    | GitFlow…                | Branch (local/remota/tag)                                                           |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-restore.png" width="16" height="16">    | Restore…                | Quando branch atual ≠ `develop` — abre a janela Restore                             |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-expand.png" width="16" height="16">     | Expandir tudo           | Sempre                                                                              |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-collapse.png" width="16" height="16">   | Recolher tudo           | Sempre                                                                              |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-refresh.png" width="16" height="16">    | Atualizar               | Sempre                                                                              |

Os itens **Baixar/Enviar do menu de contexto agem na branch em que você clicou** (não no HEAD): a branch clicada é colocada em checkout primeiro e então o pull/push é executado sobre ela, e os contadores `(N)` refletem o atrás/à frente **daquela** branch. O Enviar sobre uma branch atrás é bloqueado pelo mesmo aviso "faça Baixar primeiro" do botão. Esses itens aparecem só para branches locais e ficam logo **antes do Commit**. O popup também exibe, no **topo**, um **cabeçalho com a branch em checkout** (`Branch: <nome>`).

O item **Commit** mostra entre parênteses a quantidade de mudanças pendentes na working tree (arquivos staged, modificados e não rastreados), recalculada toda vez que o menu é aberto. Ao clicar, abre a janela de Commit nativa do GitExtensions **no processo já em execução**, de modo que todos os plugins de Commit Templates (ex.: _Zimerfeld: Auto-resumo_) já estejam carregados e visíveis no dropdown. Quando o repositório exibido no BranchHierarchy divergir do repositório ativo no GitExtensions, a janela é aberta via novo processo como fallback.

### Botão GitFlow Initialize

O botão **GitFlow Initialize** fica na janela BranchHierarchy, abaixo do painel de aviso GitFlow (faixa com o botão "Organizar como GitFlow" / "Restaurar hierarquia real"), e está sempre visível. Ao clicar, aplica de uma vez as chaves de configuração padrão do GitFlow no repositório atual:

| Chave                       | Valor padrão |
| --------------------------- | ------------ |
| `gitflow.branch.main`       | `main`       |
| `gitflow.branch.develop`    | `develop`    |
| `gitflow.prefix.feature`    | `feature/`   |
| `gitflow.prefix.bugfix`     | `bugfix/`    |
| `gitflow.prefix.release`    | `release/`   |
| `gitflow.prefix.hotfix`     | `hotfix/`    |
| `gitflow.prefix.support`    | `support/`   |
| `gitflow.prefix.versiontag` | _(vazio)_    |

Equivale a executar `git config <chave> <valor>` para cada linha. Útil para inicializar um repositório novo no padrão GitFlow sem precisar rodar `git flow init` interativo. Em caso de sucesso completo, uma mensagem de confirmação é exibida; se algum comando falhar, os erros são listados.

## Estrutura do projeto

```
ZimerfeldTree/
├── src/
│   └── GitExtensions.ZimerfeldTree/
│       ├── ZimerfeldTreePlugin.cs             # Ponto de entrada MEF (IGitPlugin)
│       ├── BranchHierarchyForm.cs             # Janela principal: árvore hierárquica de branches
│       ├── GitFlowForm.cs                     # Janela Git Flow: start/publish/pull/finish de feature/release/hotfix
│       ├── RestoreForm.cs                     # Janela Restore: restore de arquivo, cherry-pick, reset
│       ├── BranchHierarchyService.cs          # Lógica git: coleta, hierarquia, Git Flow
│       ├── BranchNode.cs                      # Modelos: classe BranchInfo + enum BranchType
│       ├── NodeIcons.cs                       # Ícones 16×16 da árvore (GDI+ + PNGs embutidos)
│       ├── PluginIcon.cs                      # Ícone do plugin/janela (Resources/ico.png)
│       ├── Resources/                         # PNGs embutidos (ícones de nós, menu e plugin)
│       ├── GitExtensions.ZimerfeldTree.csproj
│       └── GitExtensions.ZimerfeldTree.nuspec # Metadados do pacote NuGet
├── build.ps1                                  # Script de build, versionamento e deploy
├── README.md                                  # Seletor de idioma
├── README.pt-BR.md                            # Documentação em português
└── README.en-US.md                            # Documentation in English
```

---

### Janela GitFlow

![Janela GitFlow](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotGitFlow.png)

- Ao fechar a janela GitFlow, a janela BranchHierarchy é reposicionada automaticamente ao **centro da tela**. O fechamento **não** dispara um novo refresh (a árvore já foi atualizada ao vivo) — exceto após um **Finish de release**, em que a árvore é recarregada uma vez para focar a nova **tag**. O GitExtensions **não** é trazido para frente ao fechar
- Após um **Start** bem-sucedido, o painel "Manage existing branches" é pré-selecionado automaticamente no mesmo **Type** e na branch recém-criada — válido para feature, release, hotfix, bugfix e support
- Após **qualquer botão** da janela GitFlow (Start, Publish, Track, Update, Finish) concluir com sucesso, a árvore da janela BranchHierarchy é **atualizada imediatamente** (mesmo com a janela GitFlow ainda aberta) e o **foco permanece na janela GitFlow** — o refresh roda por trás do diálogo modal sem roubar o foco
- **Checkout + revelar a branch afetada**: após cada botão, o plugin faz `git checkout` da branch afetada e, na árvore, **expande os nós da seção LOCAL até alcançá-la** e a seleciona. Para **Start/Publish/Track/Update** a branch afetada é a própria (`<prefixo><nome>`); para **Finish** (a branch é removida) o plugin revela a branch resultante atual (ex.: `develop`), sem refazer checkout
- O painel **Resultado** exibe a saída de cada comando `git` em fonte monoespaçada, com fundo bege (`#EFEBD8`) idêntico ao do console nativo do GitExtensions (janelas Push/Fetch)

### Regras de Start e Finish por tipo

O diagrama resume, para cada tipo de branch, a **base usada no Start**, o **branch criado** e o **destino do merge no Finish**:

![Regras de Start e Finish por tipo](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotStartFinish.png)

- **feature** — nasce de `develop` (ou de outra `feature/*`, opcional); finaliza em `develop` ou no branch pai (based-on)
- **bugfix** — nasce de uma `release/*` (escolha obrigatória); finaliza em `develop` ou no pai
- **release** — nasce de `develop` (base fixa); finaliza em `main` (`merge --no-ff` + tag) e em `develop`, com push de main/develop/tag
- **hotfix** — nasce de `main` (base fixa); finaliza em `main` (`merge --no-ff` + tag) e em `develop`
- **support** — nasce de uma **tag** de produção (escolha obrigatória); finaliza apenas em `main`, sem tag e sem tocar em `develop`
- Comum a todo Finish: fetch opcional, exclusão do branch local e remoto (exceto **Keep**) e religação dos filhos na árvore

O fluxo completo de comandos `git` de cada tipo, do Start ao Finish:

![Fluxo completo Start a Finish por tipo](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotFlowPerType.png)

### Hierarquia: como o nó é posicionado na árvore

O git guarda apenas o commit-tip de cada branch, não a origem. Para aninhar o novo branch sob a base, o Start usa um destes mecanismos:

![Hierarquia: commit vazio e based-on override](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotHierarchyBasedOn.png)

- **commit vazio** (base = develop/main, based-on marcado) — `git commit --allow-empty` faz o tip divergir; a ancestralidade real aninha o nó
- **based-on override** (base = `feature/*` custom, based-on marcado) — grava `.git/zimerfeld-basedon.json` (link puramente visual, história limpa)
- **regra GitFlow / path** (sem based-on) — `checkout -b` simples; o nó fica no tip da base e é agrupado pela regra GitFlow + prefixo
- No Finish, `RebaseBasedOnOnFinish` remove o link do branch finalizado e re-aponta os filhos para o branch destino, mantendo a árvore conectada

### Janela GitFlow — branch base no Start

No painel **Start branch** da janela GitFlow, além de tipo e nome, há a opção **based on:**:

- Por padrão o dropdown fica **desabilitado** e usa a base padrão do tipo:
  - `develop` para feature, bugfix e release
  - `main` para hotfix e support
- Ao marcar o checkbox **based on:**, o dropdown é habilitado e lista as branches locais, permitindo iniciar a nova branch a partir de outra — por exemplo, uma **feature filha de outra feature pai**
- O comando executado é: `git checkout -b <prefixo><nome> <base>`
- **Nome padrão de release**: ao selecionar o tipo **release**, o campo de nome é preenchido automaticamente com a convenção `yyyyMMddHHmm` (ex.: `202605311230`), gerando branches como `release/202605311230`; o preenchimento só ocorre quando o campo está vazio, nunca sobrescrevendo digitação manual

### Janela GitFlow — painel "Manage existing branches"

O plugin executa **git nativo** diretamente — **não requer o binário `git-flow` instalado**. Cada botão dispara a sequência de comandos abaixo:

- **Publish** — `git push --set-upstream <remote> <prefixo><nome>`: envia a branch para o remoto e define o upstream local
- **Track** — `git fetch <remote>` + `git checkout -b <prefixo><nome> --track <remote>/<prefixo><nome>`: cria uma branch local rastreando a branch remota correspondente (útil para branches iniciadas por outra pessoa)
- **Update** — `git fetch <remote>` + `git checkout <branch>` + `git merge <remote>/<pai>`: traz as mudanças da branch **pai** (develop ou main) para a branch. Com **No fetch** marcado, o merge é feito contra a referência local
- **Finish** — mescla de volta, exclui a branch local (se **Keep** desmarcado) e **remove a branch do remoto** (se existir); o checkbox **No fetch** omite o fetch inicial
  - **feature / bugfix**: `git checkout develop` → `git merge --no-ff` → `git branch -d` → `git push <remote> --delete`
  - **release / hotfix**: `git checkout main` → `git merge --no-ff` → `git tag -a <nome> -m "<nome>"` → `git checkout develop` → `git merge --no-ff` → `git branch -d` → `git push <remote> --delete`
  - **support**: `git checkout main` → `git merge --no-ff` → `git branch -d` → `git push <remote> --delete`
  - A remoção remota só ocorre se a branch existir no remoto (verificado com `ls-remote`); caso contrário, uma nota é exibida no painel de resultado
  - A janela GitFlow mantém o foco após cada comando executado

**Finish de `release` — fluxo completo automático** (quando **No fetch** não está marcado):

| Passo | Comando                                                                                                               |
| ----- | --------------------------------------------------------------------------------------------------------------------- |
| 1     | `git fetch <remote>`                                                                                                  |
| 2     | `git checkout main` → `git merge --no-ff release/<nome>`                                                              |
| 3     | `git tag -a <nome> -m "<nome>"`                                                                                       |
| 4     | `git checkout develop` → `git merge --no-ff release/<nome>`                                                           |
| 5     | `git branch -d release/<nome>` _(se Keep desmarcado)_                                                                 |
| 6     | `git push <remote> --delete release/<nome>` _(somente se a branch remota ainda existir — verificado com `ls-remote`)_ |
| 7     | `git push <remote> main`                                                                                              |
| 8     | `git push <remote> develop`                                                                                           |
| 9     | `git push <remote> refs/tags/<nome>`                                                                                  |
| 10    | `git checkout develop`                                                                                                |

Ao concluir com sucesso, a seção **TAGS** da árvore é expandida e o foco vai para a tag criada.

O remote usado é `origin` (ou o primeiro configurado quando `origin` não existe). Se um passo de push falhar, o fluxo para naquele ponto.

- O dropdown de branch lista **apenas as branches locais** do tipo selecionado; é recarregado após cada operação
- Ao abrir a janela, se a branch em **checkout** corresponder a um tipo gitflow (ex.: `feature/manage`), o dropdown de tipo e de branch já vêm pré-selecionados

#### Tratamento de erros

Quando um comando git falha, o resultado é exibido na janela e um aviso é mostrado. Se o erro indicar uma **branch de destino ausente** (ex.: `does not exist`, `not found`), a mensagem orienta a verificar as branches existentes e a configuração `gitflow.branch.*`. Se ocorrer conflito de merge, o repositório fica em estado "merging" — resolver manualmente com `git merge --abort` ou resolver os conflitos e `git commit`.

### Janela Restore

![Janela Restore](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestore.png)

Abre ao clicar em **Restore** — janela modal posicionada ao lado de BranchHierarchy. É a central de "voltar no tempo" do código: reúne **todas** as formas de recuperar, desfazer ou descartar um estado do repositório, cada uma em sua própria **aba**. As abas estão organizadas **da mais segura para a mais destrutiva**, e os botões em **vermelho** são irreversíveis e pedem confirmação.

> 💡 Todo o racional — incluindo a categorização por segurança e as recomendações de **trabalho em equipe** — está no link **Sobre o Restore** (canto superior direito), que abre uma janela rolável com a explicação completa.

#### 🟢 Seguras (não reescrevem o histórico)

| Aba | Comando git | O que faz |
| --- | --- | --- |
| **Plano de Emergência** | `checkout <tag> -- .` / `reset --hard <tag>` | Leva uma branch ao estado de uma **tag** (release): restaurar (staged, histórico intacto) ou resetar (move o ponteiro). |
| **Restaurar Arquivo** | `checkout <hash> -- "<arquivo>"` | Recupera **um arquivo** de um commit antigo, deixando-o staged. O botão **Procurar…** abre o explorador de arquivos do Windows **já na pasta do projeto** e **recusa** qualquer arquivo fora da raiz do repositório (o caminho é convertido para relativo, com `/`). |
| **Restaurar Árvore** | `checkout <hash> -- .` | Recupera **toda** a árvore rastreada de um commit qualquer (não só de uma tag), como mudanças staged; histórico intacto. |
| **Cherry-Pick** | `cherry-pick <hash>` | Aplica um ou mais commits sobre a branch atual. Aceita hash simples ou intervalo `<antigo>..<recente>`. |
| **Reverter** | `revert <hash>` / `revert -m 1 <hash>` | Cria um **novo commit** que desfaz um commit anterior **sem reescrever histórico** — o jeito correto de desfazer algo **já enviado** ao remoto. O segundo botão reverte um **merge** inteiro (`-m 1`). |
| **Nova Branch/Tag** | `branch <nome> <hash>` / `tag <nome> <hash>` | Cria uma branch ou tag apontando para um commit antigo — "bifurca" o passado sem tocar em nenhuma branch existente. |

#### 🔵 Inspecionar (apenas leitura)

| Aba | Comando git | O que faz |
| --- | --- | --- |
| **Nova Branch/Tag → Inspecionar** | `checkout <hash>` | Abre o código exatamente como estava naquele commit, em **detached HEAD**. Nenhuma branch é movida; volta-se fazendo checkout de uma branch. |

#### 🟡 Recuperação

| Aba | Comando git | O que faz |
| --- | --- | --- |
| **Recuperar (Reflog)** | `branch <nome> <entrada>` / `reset --hard <entrada>` | Lista **todos os movimentos do HEAD** (commit, reset, rebase, checkout, merge). Permite recriar uma branch numa entrada (recuperar branch deletada / commit "perdido") ou resetar a branch atual para ela — a rede de segurança para um `reset --hard` que deu errado. |

#### 🟠 Descartar mudanças locais (working tree)

| Aba | Comando git | O que faz |
| --- | --- | --- |
| **Descartar Locais** | `checkout -- .` / `reset --hard HEAD` / `clean -fd` | Joga fora alterações **não commitadas** (não mexe no histórico): descartar não staged, descartar tudo (staged + unstaged), ou remover arquivos não rastreados. |

#### 🔴 Reescrevem o histórico (avançado)

| Aba | Comando git | O que faz |
| --- | --- | --- |
| **Reset Branch** | `reset --mixed/--soft/--hard <hash>` | Move o ponteiro de uma branch para um commit anterior. Se a branch escolhida não for a atual, o plugin faz `checkout <branch>`, aplica o reset e retorna à branch original automaticamente. |
| **Rebase** | `rebase --onto <hash>^ <hash>` | **Remove um commit específico do histórico**, reaplicando os posteriores. Em caso de conflito, o resultado avisa para resolver (`rebase --continue`) ou usar **Abortar Rebase**. |

Modos do **Reset Branch**:

| Modo      | Efeito                                                                            |
| --------- | --------------------------------------------------------------------------------- |
| `--mixed` | Desfaz commits; mudanças voltam como **unstaged** (padrão)                        |
| `--soft`  | Desfaz commits; mudanças voltam como **staged**                                   |
| `--hard`  | Desfaz commits e **DESCARTA** todas as mudanças — irreversível (pede confirmação) |

> ⚠️ **Nunca** reescreva (Reset --hard, Rebase) ou descarte o histórico de uma branch que outras pessoas já têm — isso quebra o repositório dos colegas.

#### 👥 Trabalho em equipe (no Sobre o Restore)

- **Vários devs na mesma branch (ex.: `main`):** para desfazer algo **já enviado**, use **Reverter** (não Reset --hard); faça `git pull` (de preferência `--rebase`) **antes** de enviar para evitar a rejeição *non-fast-forward*; Reset --hard/Rebase/Descartar só são seguros em trabalho **local** que ninguém mais tem.
- **Várias branches a mesclar na `develop`:** use **Cherry-Pick** para trazer commits específicos; **Reverter Merge (-m 1)** para desfazer um merge problemático preservando o resto; resolva conflitos com calma (**Abortar Rebase** / `git merge --abort` volta ao estado anterior); crie uma **Nova Branch/Tag** a partir de um commit para isolar ou retomar um trabalho.

#### Comportamento da janela

- Janela **modal**, posicionada ao lado de BranchHierarchy, ambas centralizadas na tela (mesmo comportamento da janela GitFlow). A janela foi **alargada** (980 px) e usa abas em **múltiplas linhas** para que **todas** fiquem visíveis ao mesmo tempo
- Os dropdowns de commit hash (em todas as abas que pedem um commit) listam os commits recentes como `(YYYY-MM-dd HH:mm:ss) [branch] hash  →  mensagem`, do mais recente para o mais antigo; cada lista é limitada à largura do campo, dentro da margem direita. O dropdown do **Reflog** lista as entradas `HEAD@{n}` do mesmo jeito
- Cada dropdown inicia na opção **Selecione...** / **Select...** e **não** é persistido, evitando reutilizar silenciosamente um hash antigo
- Os dois combos de **branch** (Plano de Emergência e Reset Branch) vêm **pré-selecionados com a branch em checkout** ao abrir (fallback: `develop` → `main` → `master`)
- O resultado de cada comando `git` é exibido em tempo real no painel **Resultado** (fonte monoespaçada, fundo bege `#EFEBD8` igual ao do console nativo do GitExtensions, scroll automático para o fim)
- Após cada operação bem-sucedida, a árvore de BranchHierarchy é **atualizada em background** sem perder o foco da janela Restore
- **Nenhum dropdown é persistido** — todos reabrem no padrão a cada vez. Apenas os campos que não são combo (o caminho do arquivo e o modo de reset) são lembrados entre aberturas, em `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json` (junto do idioma e do Show Debug desta janela)
- O link **Sobre o Restore** abre uma janela **rolável** com a explicação completa de cada aba, a categorização por segurança e as recomendações de trabalho em equipe
- Fechar a janela (botão **Fechar** ou tecla **Esc**) salva os valores automaticamente; o fechamento **não** dispara refresh extra (a árvore já foi atualizada ao vivo) nem traz o GitExtensions para frente

### Ícones

- No **menu Plugins** do GitExtensions (16 × 16 px)
- Na **barra de título** da janela do plugin e na barra de tarefas do Windows

O ícone (Árvore da Vida) é o PNG 16 × 16 embutido [`Resources/ico.png`](src/GitExtensions.ZimerfeldTree/Resources/ico.png), carregado uma vez por [`PluginIcon.cs`](src/GitExtensions.ZimerfeldTree/PluginIcon.cs) e cacheado durante a vida do processo. Não há dependências externas.

### Ícones por tipo de branch

Cada nó da árvore recebe um ícone 16 × 16 px gerado em tempo de execução via GDI+ em [`NodeIcons.cs`](src/GitExtensions.ZimerfeldTree/NodeIcons.cs). Os tipos GitFlow têm ícones próprios:

| Imagem                                                                                                                                                  | Branch                | Ícone                                                                                                  |
| ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------ |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/master.png" width="16" height="16">  | `master` / `main`     | **imagem personalizada embutida** (escudo dourado como reserva)                                        |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/develop.png" width="16" height="16"> | `develop`             | **imagem personalizada embutida** (chave + martelo como reserva)                                       |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/feature.png" width="16" height="16"> | nó-pasta "feature"    | **imagem personalizada embutida** (galho de branch; folha verde como reserva)                          |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/folha.png" width="16" height="16">   | `feature/*` (sub-nós) | **imagem personalizada embutida** `folha.png` (folha verde como reserva)                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/bugfix.png" width="16" height="16">  | `bugfix/*`            | **imagem personalizada embutida** (joaninha vermelha como reserva)                                     |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/release.png" width="16" height="16"> | `release/*`           | **imagem personalizada embutida** (pacote/caixa marrom como reserva)                                   |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/hotfix.png" width="16" height="16">  | `hotfix/*`            | **imagem personalizada embutida** (extintor de incêndio vermelho como reserva)                         |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/support.png" width="16" height="16"> | `support/*`           | **imagem personalizada embutida** (maleta de primeiros socorros branca com cruz vermelha como reserva) |

Branches locais genéricas usam garfo laranja e nós de caminho usam pasta âmbar. As **seções raiz** (LOCAL, REMOTES, TAGS), o **grupo de remote** (ex.: `origin`), as **branches remotas** e as **tags** também usam imagens personalizadas embutidas (ver abaixo).

#### Ícones personalizados (recursos embutidos)

Vários nós usam **imagens PNG embutidas na DLL**, declaradas como `<EmbeddedResource>` condicionais no `.csproj`. Em tempo de execução, `NodeIcons.LoadEmbedded` lê o recurso pelo nome `GitExtensions.ZimerfeldTree.Resources.<arquivo>` e o redimensiona para 16 × 16 px com interpolação de alta qualidade.

| Nó                               | Arquivo                       | Reserva (glifo desenhado)        |
| -------------------------------- | ----------------------------- | -------------------------------- |
| seção **LOCAL**                  | `Resources/local.png`         | monitor azul-aço                 |
| seção **REMOTES**                | `Resources/remotes.png`       | nuvem azul-escura                |
| seção **TAGS**                   | `Resources/tags.png`          | etiqueta/fita roxa               |
| grupo de remote (`origin`)       | `Resources/origin.png`        | nuvem azul                       |
| branch remota (filho de REMOTES) | `Resources/remote-branch.png` | garfo verde                      |
| tag (filho de TAGS)              | `Resources/tag.png`           | etiqueta teal                    |
| `master` / `main`                | `Resources/master.png`        | escudo dourado                   |
| `develop`                        | `Resources/develop.png`       | chave de boca + martelo cruzados |
| nó-pasta "feature"               | `Resources/feature.png`       | galho de branch                  |
| `feature/*` (sub-nós)            | `Resources/folha.png`         | folha verde                      |
| `release/*`                      | `Resources/release.png`       | pacote/caixa marrom              |
| `bugfix/*`                       | `Resources/bugfix.png`        | joaninha vermelha                |
| `hotfix/*`                       | `Resources/hotfix.png`        | extintor de incêndio vermelho    |
| `support/*`                      | `Resources/support.png`       | maleta branca com cruz vermelha  |

- O plugin permanece **autocontido**: as imagens viajam dentro da DLL, sem depender de arquivos externos na máquina do usuário.
- Cada `<EmbeddedResource>` é **condicional à existência do arquivo** (`Condition="Exists(...)"`); se o PNG não existir no build, o recurso não é embutido e o nó usa o glifo desenhado de reserva — o build nunca quebra por falta da imagem.
- Se o recurso estiver **ausente ou ilegível** em tempo de execução, o ícone cai automaticamente na reserva, preservando o comportamento anterior.
- Para trocar/adicionar uma imagem: coloque o PNG 16 × 16 em `src/GitExtensions.ZimerfeldTree/Resources/<arquivo>.png` e refaça o build.

### Atalhos de teclado e mouse

- **Duplo clique** em qualquer branch → checkout da branch selecionada
- **Enter** → checkout da branch selecionada
- **Botão direito** → seleciona o nó e abre o menu de contexto

### Janela não-modal persistente

- A janela permanece aberta enquanto o GitExtensions está em uso
- Fechar a janela a destrói — necessário reabrir para recarregar dados
- Singleton: uma única instância por sessão do GitExtensions
- **Foco persistente após ações**: qualquer ação executada na janela (Pull, Push, Commit, Checkout, Nova branch, Merge, Rebase, Renomear, Excluir) devolve o foco à janela BranchHierarchy ao concluir. Como a janela é independente (sem owner), notificar o GitExtensions para atualizar sua UI traria a janela principal do GitExtensions para frente; o plugin reativa a BranchHierarchy logo em seguida. A **única exceção é o botão GitFlow**: ele abre a janela GitFlow (modal), que mantém o próprio foco enquanto estiver aberta

## Dependências

### Obrigatórias para uso

| Programa                 | Versão mínima | Download                                                           | Função                                                                               |
| ------------------------ | ------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------ |
| **Git for Windows**      | qualquer      | https://git-scm.com/download/win                                   | Executa todos os comandos git (branch, checkout, pull, push, commit, tag…)           |
| **GitExtensions**        | 4.x (.NET 9)  | https://github.com/gitextensions/gitextensions/releases            | Aplicação host que carrega o plugin; fornece diálogos nativos de Commit, Push e Pull |
| **Plugin ZimerfeldTree** | —             | `C:\Program Files\GitExtensions\Plugins\` (build local ou release) | O plugin em si                                                                       |

**Instalação do Git for Windows:** baixar o instalador `.exe` e, na tela _"Adjusting your PATH"_, selecionar **"Git from command line and also from 3rd-party software"**.

**Instalação do GitExtensions:** baixar o instalador `.msi` da release 4.x; ele instala o .NET 9 Desktop Runtime automaticamente.

> **Atenção:** GitExtensions 3.x (`.NET Framework 4.8`) é incompatível — o plugin requer `net9.0-windows`.

---

---

### Condicional — apenas para build / desenvolvimento

| Programa       | Download                                         | Função                                 |
| -------------- | ------------------------------------------------ | -------------------------------------- |
| **.NET SDK 9** | https://dotnet.microsoft.com/download/dotnet/9.0 | Compilar `net9.0-windows`              |
| **NuGet CLI**  | https://www.nuget.org/downloads                  | Gerar `.nupkg` (usado por `build.ps1`) |

---

## Instalação

### Opção A — Via PowerShell (recomendado)

Execute o PowerShell **como Administrador**:

```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

![Instalação via install.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotInstall.png)

> O script **fecha o GitExtensions automaticamente** se estiver aberto (o app mantém o DLL do plugin bloqueado): tenta o fechamento normal e, se não responder em 10 s, encerra à força.

### Opção B — Manual

Copie `GitExtensions.Plugins.ZimerfeldTree.dll` para:

```
C:\Program Files\GitExtensions\Plugins\
```

Reinicie o GitExtensions.

---

## Desinstalação

```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\uninstall.ps1
```

![Desinstalação via uninstall.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUninstall.png)

> O script **fecha o GitExtensions automaticamente** se estiver aberto (o app mantém o DLL do plugin bloqueado): tenta o fechamento normal e, se não responder em 10 s, encerra à força.

Como alternativa, delete manualmente o arquivo:

```
C:\Program Files\GitExtensions\Plugins\GitExtensions.Plugins.ZimerfeldTree.dll
```

A remoção da DLL não afeta nenhuma outra parte do GitExtensions.

---

## Atualização da DLL

Para atualizar apenas a DLL já instalada (sem reinstalar), execute o PowerShell **como Administrador**:

```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\update-dll.ps1
```

![Atualização via update-dll.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUpdate.png)

> O script **fecha o GitExtensions automaticamente** se estiver aberto (o app mantém o DLL do plugin bloqueado): tenta o fechamento normal e, se não responder em 10 s, encerra à força.

Reinicie o GitExtensions para aplicar a nova DLL.

---

## Build

```powershell
# Incrementa versão, compila e gera .nupkg
# Execute como Administrador para também copiar o DLL para Plugins\
pwsh C:\NUGET\ZimerfeldTree\build.ps1
```

O script:

1. Lê a versão atual do `.nuspec` e incrementa o `build` (major.minor.**build**)
2. Atualiza `.nuspec` e `.csproj` com a nova versão
3. Compila em modo Release (`net9.0-windows`)
4. Se for Administrador, copia o DLL para `C:\Program Files\GitExtensions\Plugins\`
5. Empacota o `.nupkg` em `C:\NUGET\ZimerfeldTree\`

Build concluído com sucesso (versão incrementada, DLL copiada e `.nupkg` gerado):

![Build bem-sucedido](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBuild.png)

Quando **nenhuma mudança** é detectada nos fontes, o script mantém a versão e ignora build/pack:

![Build sem mudanças](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotNoBuild.png)

---

## Hierarquia de branches — limitações

### A hierarquia é por nome, não por parentesco de commits

O plugin agrupa branches usando o separador `/` do nome — **não** pelo histórico de commits do git. `master` e `develop` são pai e filho porque develop for gerada a partir de master `/`:

```
LOCAL
├── develop      ← irmão
└── master       ← irmão
```

Para que uma branch apareça como filha de outra, o nome deve conter `/`:

```
LOCAL
└── feature/
    ├── login    ← feature/login
    └── pagamento ← feature/pagamento
```

### Branch real não pode ser nó pai de outra branch

O git armazena refs como arquivos no sistema de arquivos. Se `feature/login` já existe como branch, tentrar criar `feature/login/oauth` resulta em erro:

```
fatal: cannot lock ref 'refs/heads/feature/login/oauth':
'refs/heads/feature/login' exists; cannot create 'refs/heads/feature/login/oauth'
```

Isso ocorre porque `feature/login` seria simultaneamente um **arquivo** (a branch) e um **diretório** (pai de `oauth`), o que é impossível no sistema de arquivos.

**Solução:** use prefixos distintos ou nomes irmãos:

| Intenção                  | Nomes que funcionam                            |
| ------------------------- | ---------------------------------------------- |
| Sub-tarefas de login      | `feature/login-oauth`, `feature/login-session` |
| Agrupador sem branch real | `feature/login/base` + `feature/login/oauth`   |

### Hierarquia flexível do GitFlow — feature filha de feature

O GitFlow conhecido **não prevê feature filha de feature**. O GitFlow define uma hierarquia fixa onde todas as branches `feature/*` derivam de `develop` e são **irmãs** entre si. Sub-features são geralmente tratadas com commits separados na mesma branch ou com branches irmãs de prefixo comum.

Porém o **ZimerfeldTree GitFlow** permite uma hierarquia **flexível** onde as branches `feature/*` podem tanto derivar de `develop` quanto de uma outra `feature/*` acima dela (use **based on:** em GitFlow → Start). Nesse caso o *finish feature* deve obrigatoriamente **cascatear** todas as mudanças para a branch `feature/*` nó pai sucessivamente, aplicando *finish feature* novamente até chegar em `develop`.

### Duas branches no mesmo commit não formam hierarquia pai-filho

O algoritmo de ancestralidade usa BFS a partir dos **pais** do commit-tip de cada branch. Quando duas branches apontam para o **exato mesmo commit** (ex.: branch recém-criada sem commits próprios), nenhuma pode ser encontrada como ancestral da outra — ambas aparecem como raízes independentes.

```
# Situação sem hierarquia — ambas no commit c19d7dc
feature/gridsolo   → c19d7dc
feature/mododebug  → c19d7dc   ← BFS de gridsolo nunca a encontra como pai

# Situação com hierarquia — gridsolo um commit à frente
feature/gridsolo   → cea86c1   ← BFS encontra mododebug no pai imediato
feature/mododebug  → c19d7dc
```

Isso não é uma limitação do plugin, mas do git: ele não registra de onde uma branch foi criada, apenas para qual commit ela aponta.

**Solução automática:** ao usar o checkbox **based on:** na janela GitFlow → Start, o plugin executa automaticamente um commit vazio na branch recém-criada:

```
git commit --allow-empty -m "chore: start <prefixo><nome>"
```

Isso garante que a nova branch diverge imediatamente de sua base e a hierarquia fica visível na árvore sem ação manual.

---

## Uso

1. Abra um repositório no GitExtensions
2. Vá em **Plugins → ZimerfeldTree**
3. A janela de hierarquia fica aberta ao lado — navega, filtra, faz checkout sem sair dela

---

## Plugins integrados

### [GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)

**por:** zimerfeld

Plugin para GitExtensions que gera automaticamente uma mensagem de commit resumindo em uma frase as mudanças nos arquivos staged, usando o formato Conventional Commits (`feat` / `fix` / `docs` / `test` / `chore`).
