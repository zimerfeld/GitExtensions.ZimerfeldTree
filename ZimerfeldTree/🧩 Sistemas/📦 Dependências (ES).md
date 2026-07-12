---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [sistema, dependencias, instalacao, gitextensions, git, gitflow]
versao: 1.0.358
---

# Dependencias

> 🇧🇷 Lee esta página en portugués → [[📦 Dependências]]
> 🇺🇸 Read this page in English → [[📦 Dependências (EN)]]

> [!abstract] Resumen
> Programas necesarios para **usar** y para **compilar** el plugin. El detalle paso a paso (descargas, verificación, procedimiento de instalación) vive en la nota de herramientas [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]] — esta nota es el resumen de sistema equivalente al de la bóveda hermana.

## Obligatorias para el uso

| Programa | Versión | Papel |
|---|---|---|
| **Git for Windows** | cualquiera ([descarga](https://git-scm.com/download/win)) | Ejecuta **todos** los comandos git del plugin. En la pantalla *"Adjusting your PATH"* elegir **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | App **host** que carga el plugin vía MEF; provee los diálogos nativos de Commit/Push/Pull. El `.msi` instala el .NET 9 Desktop Runtime |
| **Plugin ZimerfeldTree** | — | La DLL `GitExtensions.Plugins.ZimerfeldTree.dll` en `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) es **incompatible** — el plugin requiere `net9.0-windows`.

## Condicionales — build / desarrollo

| Programa | Papel |
|---|---|
| **.NET SDK 9** ([descarga](https://dotnet.microsoft.com/download/dotnet/9.0)) | Compilar `net9.0-windows` |
| **NuGet CLI** ([descarga](https://www.nuget.org/downloads)) | Generar el `.nupkg` (usado por `build.ps1`; resuelto vía PATH → `tools\nuget.exe` → descarga automática) |

## Referencias externas (no copiadas)

De `C:\Program Files\GitExtensions\`, con `Private=false` (el runtime del host las resuelve en tiempo de carga):
- `GitExtensions.Extensibility.dll`
- `GitUIPluginInterfaces.dll`
- `System.ComponentModel.Composition.dll`

## GitFlow sin dependencia externa

> [!info]
> Todos los comandos GitFlow (Start, Publish, Track, Update, Finish) usan **git puro**. El binario `git-flow` **no** necesita estar instalado. Ver [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]].

## Relacionado

- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]] — versión detallada, paso a paso
- [[🏗️ Arquitetura (ES)|Arquitectura]]
- [[👁️ Visão Geral (ES)|Visión General]]
- [[🔧 build.ps1 (ES)|build.ps1]]
