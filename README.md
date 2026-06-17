# GitExtensions.ZimerfeldTree

![Icone](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/icon-128.png)

- Help keep this project always updated 💜
- Ajude a manter este projeto sempre atualizado 💜

[![GitHub Sponsor](https://img.shields.io/badge/Sponsor-zimerfeld-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white)](https://github.com/sponsors/zimerfeld) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; [![Ko-fi](https://img.shields.io/badge/Ko--fi-Buy%20me%20a%20coffee-FF5E2B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/C0D621FCGD)

**Version:** 1.0.331  
**Updated:** 2026-06-16

**Versão:** 1.0.331  
**Atualizado em:** 2026-06-16

Plugin for [GitExtensions](https://gitextensions.github.io/) that displays branches as a hierarchical tree and it makes the GitFlow methodology available in a very easy, intuitive, and pleasant visual way to apply to projects of any size.

Plugin para [GitExtensions](https://gitextensions.github.io/) que exibe branches em uma arvore hierarquica e disponibiliza o uso da metodologia GitFlow de maneira visual muito fácil, intuitiva e agradável de aplicar em projetos de qualquer tamanho.

![ZimerfeldTree - BranchHierarchy](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBranchHierarchy.png)

### GitFlow flexible hierarchy — feature under feature

Classic GitFlow does not provide for a feature branch as a child of another feature. GitFlow defines a fixed hierarchy where all `feature/*` branches derive from `develop` and are siblings of one another. Sub-features are usually handled with separate commits on the same branch or with sibling branches sharing a common prefix. **ZimerfeldTree GitFlow**, however, allows a flexible hierarchy where `feature/*` branches can derive either from `develop` or from another `feature/*` above them. In that case, finishing a feature must necessarily **cascade** all its changes up to the parent `feature/*` node, successively re-applying *finish feature* until it reaches `develop`.

### Hierarquia flexível do GitFlow — feature filha de feature

O GitFlow conhecido não prevê feature filha de feature. O GitFlow define uma hierarquia fixa onde todas as branches `feature/*` derivam de `develop` e são irmãs entre si. Sub-features são geralmente tratadas com commits separados na mesma branch ou com branches irmãs de prefixo comum. Porém o **ZimerfeldTree GitFlow** permite uma hierarquia flexível onde as branches `feature/*` podem tanto derivar de `develop` quanto de uma outra `feature/*` acima dela. Nesse caso o *finish feature* deve obrigatoriamente **cascatear** todas as mudanças para a branch `feature/*` nó pai sucessivamente, aplicando *finish feature* novamente até chegar em `develop`.

GitFlow — Start and Finish rules per branch type / Regras de Start e Finish por tipo de branch:

![Start and Finish rules per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotStartFinish.png)

Full command flow per type / Fluxo completo de comandos por tipo:

![Full Start to Finish flow per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotFlowPerType.png)

Tree hierarchy — empty commit vs based-on override / Hierarquia na árvore — commit vazio vs based-on override:

![Hierarchy: empty commit and based-on override](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotHierarchyBasedOn.png)

## Languages / Idiomas

- [English (United States)](https://github.com/zimerfeld/ZimerfeldTree/blob/main/README.en-US.md)
- [Português (Brasil)](https://github.com/zimerfeld/ZimerfeldTree/blob/main/README.pt-BR.md)

## Package / Pacote

- [NuGet package](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/)
- [GitHub repository](https://github.com/zimerfeld/ZimerfeldTree)

## NuGet

This README is intentionally short so it works both on GitHub and NuGet. Use the language links above for the full documentation.

Este README é propositalmente curto para funcionar tanto no GitHub quanto no NuGet. Use os links de idioma acima para acessar a documentação completa.

## License / Licença

[MIT](LICENSE.txt)
