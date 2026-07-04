---
tipo: sistema
tags: [sistema, dependencias, instalacao, gitextensions, git, gitflow]
atualizado: 2026-07-04
versao: 1.0.358
---

# Dependências

> [!abstract] Resumo
> Programas necessários para **usar** e para **compilar** o plugin. O detalhamento passo-a-passo (downloads, verificação, procedimento de instalação) vive na nota de ferramentas [[Dependências do ZimerfeldTree]] — esta nota é o resumo de sistema equivalente à do vault irmão.

## Obrigatórias para uso

| Programa | Versão | Papel |
|---|---|---|
| **Git for Windows** | qualquer ([download](https://git-scm.com/download/win)) | Executa **todos** os comandos git do plugin. Na tela *"Adjusting your PATH"* escolher **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | App **host** que carrega o plugin via MEF; fornece os diálogos nativos de Commit/Push/Pull. O `.msi` instala o .NET 9 Desktop Runtime |
| **Plugin ZimerfeldTree** | — | A DLL `GitExtensions.Plugins.ZimerfeldTree.dll` em `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) é **incompatível** — o plugin requer `net9.0-windows`.

## Condicionais — build / desenvolvimento

| Programa | Papel |
|---|---|
| **.NET SDK 9** ([download](https://dotnet.microsoft.com/download/dotnet/9.0)) | Compilar `net9.0-windows` |
| **NuGet CLI** ([download](https://www.nuget.org/downloads)) | Gerar `.nupkg` (usado por `build.ps1`; resolvido via PATH → `tools\nuget.exe` → download automático) |

## Referências externas (não copiadas)

De `C:\Program Files\GitExtensions\`, com `Private=false` (o runtime do host as resolve em load time):
- `GitExtensions.Extensibility.dll`
- `GitUIPluginInterfaces.dll`
- `System.ComponentModel.Composition.dll`

## GitFlow sem dependência externa

> [!info]
> Todos os comandos GitFlow (Start, Publish, Track, Update, Finish) usam **git puro**. O binário `git-flow` **não** precisa estar instalado. Ver [[GitFlow em git puro]].

## Relacionado

- [[Dependências do ZimerfeldTree]] — versão detalhada, passo-a-passo
- [[Arquitetura]]
- [[Visão Geral]]
- [[../Arquivos-Chave/build.ps1]]
