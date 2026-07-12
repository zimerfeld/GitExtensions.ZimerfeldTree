---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
tags: [conhecimento, csharp, gitextensions, mef, plugin]
---

# Plugin MEF para GitExtensions

> 🇧🇷 Lee esta página en portugués → [[🧩 Plugin MEF para GitExtensions]]
> 🇺🇸 Read this page in English → [[🧩 Plugin MEF para GitExtensions (EN)]]

## Resumen
GitExtensions carga plugins vía **MEF** (Managed Extensibility Framework). El punto de entrada es una clase exportada que implementa la interfaz de plugin de `GitExtensions.Extensibility`.

## Puntos clave
- Usar `System.ComponentModel.Composition` (el atributo `[Export]`).
- El proyecto compila como **`Library`** (DLL), `net9.0-windows`, con WinForms habilitado.
- Referenciar los ensamblados de GitExtensions desde `C:\Program Files\GitExtensions\` con **`<Private>false</Private>`** (no copiar a la salida — el host ya los tiene):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`
- El AssemblyName debe coincidir con lo que esperan install.ps1 / nuspec.

## Trampa — MSB3277
Las DLL del host traen WindowsBase 8.0 frente al ref pack de net9 (4.0). Se resuelve en tiempo de ejecución → rebajar el warning:
```xml
<MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>
```

## 🔗 Relacionado
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
