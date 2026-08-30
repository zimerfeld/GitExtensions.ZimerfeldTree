# GitExtensions.ZimerfeldTree

![Icone](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/icon-128.png)

[![NuGet version](https://img.shields.io/nuget/v/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=NuGet)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/) &nbsp; [![NuGet downloads](https://img.shields.io/nuget/dt/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=Downloads)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/)

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) This plugin is built and maintained in my free time. If it saves you time managing branches, a sponsorship helps keep it updated for new GitExtensions versions. 💜

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) Este plugin é construído e mantido no meu tempo livre. Se ele te poupa tempo gerenciando branches, um patrocínio ajuda a mantê-lo atualizado para as novas versões do GitExtensions. 💜

> 🇪🇸 Este plugin se construye y se mantiene en mi tiempo libre. Si te ahorra tiempo gestionando branches, un patrocinio ayuda a mantenerlo actualizado para las nuevas versiones de GitExtensions. 💜

[![GitHub Sponsor](https://img.shields.io/badge/Sponsor-zimerfeld-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white)](https://github.com/sponsors/zimerfeld) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; [![Ko-fi](https://img.shields.io/badge/Ko--fi-Buy%20me%20a%20coffee-FF5E2B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/C0D621FCGD)

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) **Version:** 1.0.363 — **Updated:** 2026-08-29

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) **Versão:** 1.0.363 — **Atualizado em:** 2026-08-29

> 🇪🇸 **Versión:** 1.0.361 — **Actualizado:** 2026-07-04

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) Plugin for [GitExtensions](https://gitextensions.github.io/) that displays branches as a hierarchical tree and it makes the GitFlow methodology available in a very easy, intuitive, and pleasant visual way to apply to projects of any size.

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) Plugin para [GitExtensions](https://gitextensions.github.io/) que exibe branches em uma arvore hierarquica e disponibiliza o uso da metodologia GitFlow de maneira visual muito fácil, intuitiva e agradável de aplicar em projetos de qualquer tamanho.

> 🇪🇸 Plugin para [GitExtensions](https://gitextensions.github.io/) que muestra las branches en un árbol jerárquico y pone a disposición el uso de la metodología GitFlow de una manera visual muy fácil, intuitiva y agradable de aplicar en proyectos de cualquier tamaño.

![ZimerfeldTree - BranchHierarchy](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBranchHierarchy.png)

### GitFlow flexible hierarchy — feature under feature / Hierarquia flexível do GitFlow — feature filha de feature

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) Classic GitFlow does not provide for a feature branch as a child of another feature. GitFlow defines a fixed hierarchy where all `feature/*` branches derive from `develop` and are siblings of one another. Sub-features are usually handled with separate commits on the same branch or with sibling branches sharing a common prefix. **ZimerfeldTree GitFlow**, however, allows a flexible hierarchy where `feature/*` branches can derive either from `develop` or from another `feature/*` above them. In that case, finishing a feature must necessarily **cascade** all its changes up to the parent `feature/*` node, successively re-applying *finish feature* until it reaches `develop`.

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) O GitFlow conhecido não prevê feature filha de feature. O GitFlow define uma hierarquia fixa onde todas as branches `feature/*` derivam de `develop` e são irmãs entre si. Sub-features são geralmente tratadas com commits separados na mesma branch ou com branches irmãs de prefixo comum. Porém o **ZimerfeldTree GitFlow** permite uma hierarquia flexível onde as branches `feature/*` podem tanto derivar de `develop` quanto de uma outra `feature/*` acima dela. Nesse caso o *finish feature* deve obrigatoriamente **cascatear** todas as mudanças para a branch `feature/*` nó pai sucessivamente, aplicando *finish feature* novamente até chegar em `develop`.

> 🇪🇸 El GitFlow clásico no contempla una feature hija de otra feature. GitFlow define una jerarquía fija donde todas las branches `feature/*` derivan de `develop` y son hermanas entre sí. Las sub-features suelen tratarse con commits separados en la misma branch o con branches hermanas de prefijo común. Sin embargo, el **ZimerfeldTree GitFlow** permite una jerarquía flexible donde las branches `feature/*` pueden derivar tanto de `develop` como de otra `feature/*` por encima de ella. En ese caso, el *finish feature* debe obligatoriamente **propagar en cascada** todos los cambios hacia la branch `feature/*` nodo padre sucesivamente, aplicando *finish feature* de nuevo hasta llegar a `develop`.

GitFlow — Start and Finish rules per branch type / Regras de Start e Finish por tipo de branch:

![Start and Finish rules per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotStartFinish.png)

Full command flow per type / Fluxo completo de comandos por tipo:

![Full Start to Finish flow per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotFlowPerType.png)

Tree hierarchy — empty commit vs based-on override / Hierarquia na árvore — commit vazio vs based-on override:

![Hierarchy: empty commit and based-on override](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotHierarchyBasedOn.png)

## Highlights / Destaques

**![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) English**

- **Hierarchical branch tree** — LOCAL, REMOTES and TAGS sections combining real commit ancestry with `/` path grouping; current branch in bold, live counters and a real-time filter.
- **One-click GitFlow** — start/publish/track/update/finish for feature, release, hotfix, bugfix and support, with a flexible hierarchy that even allows a *feature under a feature* (finish cascades up to `develop`).
- **Pull / Push / Commit at hand** — arrow-icon buttons with ahead/behind counters, a background remote check on open, and a guard that, when the branch is behind, offers to **pull with rebase and then push automatically** (replaying your commits on top of the remote ones, no merge commit) — clearing the `non-fast-forward` rejection in one click. The Commit `(N)` counter updates **live** as you edit files (a working-directory watcher refreshes it silently).
- **Restore — your "go back in time" hub** — a dedicated window gathering **every** safe way to recover or undo history: restore a file/tree/tag, cherry-pick, **revert** (the safe undo for shared branches), create a branch/tag from any commit, **reflog recovery** of lost commits or deleted branches, discard local changes, and an advanced rebase to drop a commit — each with a clear, built-in explanation and teamwork guidance.
- **Localized (English / Portuguese)** — every window picks its language independently and remembers it.

**![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) Português**

- **Árvore hierárquica de branches** — seções LOCAL, REMOTES e TAGS combinando ancestralidade real de commits com agrupamento por caminho `/`; branch atual em negrito, contadores ao vivo e filtro em tempo real.
- **GitFlow num clique** — start/publish/track/update/finish para feature, release, hotfix, bugfix e support, com hierarquia flexível que permite até *feature filha de feature* (o finish cascateia até a `develop`).
- **Pull / Push / Commit à mão** — botões com ícones de seta e contadores adiante/atrás, verificação do remoto em segundo plano ao abrir e um aviso que, quando a branch está atrás, oferece **Baixar com rebase e então enviar automaticamente** (reaplicando seus commits por cima dos remotos, sem commit de merge) — resolvendo a rejeição `non-fast-forward` em um clique. O contador `(N)` do Commit atualiza **ao vivo** conforme você edita arquivos (um watcher do working directory o atualiza silenciosamente).
- **Restore — sua central de "voltar no tempo"** — uma janela dedicada reunindo **todas** as formas seguras de recuperar ou desfazer histórico: restaurar arquivo/árvore/tag, cherry-pick, **reverter** (o desfazer seguro para branches compartilhadas), criar branch/tag a partir de qualquer commit, **recuperação via reflog** de commits perdidos ou branches deletadas, descartar mudanças locais e um rebase avançado para remover um commit — cada um com explicação embutida e orientações de trabalho em equipe.
- **Localizado (Inglês / Português / Espanhol)** — cada janela escolhe seu idioma de forma independente e o memoriza.

**🇪🇸 Español**

- **Árbol jerárquico de branches** — secciones LOCAL, REMOTES y TAGS que combinan la ascendencia real de commits con la agrupación por ruta `/`; branch actual en negrita, contadores en vivo y filtro en tiempo real.
- **GitFlow con un clic** — start/publish/track/update/finish para feature, release, hotfix, bugfix y support, con una jerarquía flexible que permite incluso una *feature hija de feature* (el finish se propaga en cascada hasta `develop`).
- **Pull / Push / Commit a mano** — botones con iconos de flecha y contadores adelante/atrás, verificación del remoto en segundo plano al abrir y un aviso que, cuando la branch está por detrás, ofrece **traer con rebase y luego enviar automáticamente** (reaplicando tus commits sobre los remotos, sin commit de merge) — resolviendo el rechazo `non-fast-forward` en un clic. El contador `(N)` de Commit se actualiza **en vivo** a medida que editas archivos (un watcher del directorio de trabajo lo actualiza en silencio).
- **Restore — tu central de "volver atrás en el tiempo"** — una ventana dedicada que reúne **todas** las formas seguras de recuperar o deshacer el historial: restaurar archivo/árbol/tag, cherry-pick, **revertir** (el deshacer seguro para branches compartidas), crear branch/tag a partir de cualquier commit, **recuperación vía reflog** de commits perdidos o branches eliminadas, descartar cambios locales y un rebase avanzado para eliminar un commit — cada uno con explicación integrada y orientaciones de trabajo en equipo.
- **Localizado (Inglés / Portugués / Español)** — cada ventana elige su idioma de forma independiente y lo recuerda.

## Languages / Idiomas

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) **Technical details, features, screenshots and installation** are available in the language-specific versions. **Click here:**

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) **Detalhes técnicos, funcionalidades, printscreens e instalação** encontram-se disponíveis nas versões por idioma. **Clique aqui:**

> 🇪🇸 **Detalles técnicos, funcionalidades, capturas de pantalla e instalación** están disponibles en las versiones por idioma. **Haz clic aquí:**

- [🇧🇷 Português (Brasil)](https://github.com/zimerfeld/ZimerfeldTree/blob/main/README.pt-BR.md)
- [🇺🇸 English (United States)](https://github.com/zimerfeld/ZimerfeldTree/blob/main/README.en-US.md)
- [🇪🇸 Español (España)](https://github.com/zimerfeld/ZimerfeldTree/blob/main/README.es-ES.md)

## Package / Pacote

- [NuGet package](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/)
- [GitHub repository](https://github.com/zimerfeld/ZimerfeldTree)

## NuGet

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) This README is intentionally short so it works both on GitHub and NuGet. Use the language links above for the full documentation.

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) Este README é propositalmente curto para funcionar tanto no GitHub quanto no NuGet. Use os links de idioma acima para acessar a documentação completa.

> 🇪🇸 Este README es deliberadamente corto para funcionar tanto en GitHub como en NuGet. Usa los enlaces de idioma de arriba para acceder a la documentación completa.

## Integrated plugins / Plugins integrados

> ![EN](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotGB.png) Other GitExtensions plugins by zimerfeld that work great alongside ZimerfeldTree:

> ![PT](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/screenshotBR.png) Outros plugins do GitExtensions feitos por zimerfeld que funcionam muito bem ao lado do ZimerfeldTree:

> 🇪🇸 Otros plugins de GitExtensions hechos por zimerfeld que funcionan muy bien junto a ZimerfeldTree:

- **[GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg)** — auto-generated Conventional Commits messages / mensagens de commit no padrão Conventional Commits geradas automaticamente.
- **[GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)** — Git LFS (Large File Storage) management inside GitExtensions / gerenciamento do Git LFS (Large File Storage) dentro do GitExtensions.

## License / Licença

Copyright © 2026 Renato Zimerfeld — **CC BY-NC-ND 4.0** (see / veja [`LICENSE.txt`](LICENSE.txt)).
