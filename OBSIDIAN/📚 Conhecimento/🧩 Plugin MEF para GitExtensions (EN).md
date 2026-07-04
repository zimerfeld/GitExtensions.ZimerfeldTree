---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
tags: [conhecimento, csharp, gitextensions, mef, plugin]
---

# MEF plugin for GitExtensions

## Summary
GitExtensions loads plugins via **MEF** (Managed Extensibility Framework). The entry point is an exported class that implements the plugin interface from `GitExtensions.Extensibility`.

## Key points
- Use `System.ComponentModel.Composition` (the `[Export]` attribute).
- The project compiles as a **`Library`** (DLL), `net9.0-windows`, WinForms enabled.
- Reference the GitExtensions assemblies from `C:\Program Files\GitExtensions\` with **`<Private>false</Private>`** (do not copy to the output — the host already has them):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`
- The AssemblyName must match what install.ps1 / nuspec expect.

## Pitfall — MSB3277
Host DLLs pull in WindowsBase 8.0 vs the net9 ref pack (4.0). Resolved at runtime → downgrade the warning:
```xml
<MSBuildWarningsAsMessages>MSB3277</MSBuildWarningsAsMessages>
```

## 🔗 Related
- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
