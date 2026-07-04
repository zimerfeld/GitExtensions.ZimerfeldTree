---
tipo: conhecimento
criado: 2026-06-01
tags: [conhecimento, csharp, gitextensions, mef, plugin]
---

# Plugin MEF para GitExtensions

## Resumo
GitExtensions carrega plugins via **MEF** (Managed Extensibility Framework). O entry point é uma classe exportada que implementa a interface de plugin da `GitExtensions.Extensibility`.

## Pontos-chave
- Usar `System.ComponentModel.Composition` (atributo `[Export]`).
- Projeto compila como **`Library`** (DLL), `net9.0-windows`, WinForms habilitado.
- Referenciar os assemblies do GitExtensions de `C:\Program Files\GitExtensions\` com **`<Private>false</Private>`** (não copiar para a saída — o host já os tem):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`
- O AssemblyName precisa bater com o que o install.ps1 / nuspec esperam.

## Armadilha — MSB3277
DLLs do host puxam WindowsBase 8.0 vs ref pack net9 (4.0). Resolvido em runtime → rebaixar a warning:
```xml
<MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>
```

## 🔗 Relacionado
- [[GitExtensions.ZimerfeldTree]]
