---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [build, versão, nupkg, deploy]
versao: 1.0.361
---

# Versionado y Build

> 🇧🇷 Lee esta página en portugués → [[🏷️ Versionamento (PT)|🏷️ Versionamento]]
> 🇺🇸 Read this page in English → [[🏷️ Versionamento (EN)]]

## Esquema de versión

`major.minor.build` — solo el `build` se incrementa automáticamente mediante `build.ps1`. Major y minor se cambian manualmente.

Versión actual: **1.0.361** *(fuente de la verdad: `.nuspec` / `.csproj`)*

> [!note] Detección incremental por timestamp
> `build.ps1` solo incrementa la versión (y recompila/empaqueta) si alguna **entrada del paquete** es más reciente que el último `.nupkg` generado. Entradas = fuentes (`*.cs`/`*.csproj`/`*.nuspec`/`*.resx`/`*.png`), **cualquier `*.md`** del repositorio y los textos empaquetados (`LICENSE`, scripts de `tools\`). La comparación se hace contra el `.nupkg` (y no la DLL) a propósito — cuando solo cambia un texto, el build incremental de dotnet puede no regrabar la DLL, lo que dispararía la detección en bucle. Usa `-Force` para empaquetar incluso sin cambios.

## Ciclo build.ps1

```
build.ps1
  │
  ├─ 1.  Lee la versión actual del .nuspec
  ├─ 1b. Detecta cambios (fuentes + textos) vs. el último .nupkg → sin cambios termina
  ├─ 1c. Cierra GitExtensions y los plugins antes de compilar
  ├─ 2.  Bump en el .nuspec  ← <version>
  ├─ 3.  Bump en el .csproj  ← <Version>
  ├─ 4.  Actualiza el enlace de NuGet y "Versión actual" en README.md
  ├─ 4b. Sella la cabecera (Versión/Actualizado) en los READMEs (md / pt-BR / en-US)
  ├─ 4c. Sella la bóveda de Obsidian (notas que reflejan la versión actual)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia la DLL → C:\Program Files\GitExtensions\Plugins\  (requiere Admin)
  │       y actualiza tools\net9.0-windows\  (para el nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg en la raíz
  └─ —   Elimina los .nupkg de versiones anteriores
```

> **Orden intencional:** los docs (READMEs + bóveda) se sellan **antes** del _pack_ (paso 7), así el `.nupkg` sigue siendo el artefacto más reciente — lo que mantiene correcta la detección de cambios por timestamp y evita un rebuild en bucle.

> Requiere la CLI de `nuget` (resuelta vía PATH → `tools\nuget.exe` → descarga automática) y permiso de **Administrador** para el deploy. Sin Admin, el paso 6 se salta con un aviso.

## Archivos versionados

| Archivo | Campo actualizado |
|---|---|
| `GitExtensions.ZimerfeldTree.nuspec` | `<version>` |
| `GitExtensions.ZimerfeldTree.csproj` | `<Version>` |
| `README.md` / `README.pt-BR.md` / `README.en-US.md` | `**Version/Versão:**`, `**Updated/Atualizado em:**` y "Versión actual" |
| Bóveda de Obsidian (Proyecto, README espejo, Versionado, Visión General) | frontmatter `versao:`/`atualizado:` y la línea "Versión actual" |

> `build.ps1` registra cada nota sellada con el formato `Obsidian: <archivo> actualizado a <versión> (<fecha>)` (sección 4c, bucle sobre `$obsidianDocs`).

## NU5101 (intencional)

La DLL queda directamente en `lib\` en el nupkg a propósito: el Plugin Manager de GitExtensions solo extrae el grupo `lib` cuyo framework está en su lista de monikers (`net5.0..net10.0`, `any`, `netstandard2.0`). `lib\` raíz = grupo "any" (extraído); una subcarpeta `net9.0-windows` rompería la instalación. Por eso el aviso `NU5101` se **filtra** de la salida del `pack`.

## Deploy rápido (sin incrementar la versión)

```powershell
.\tools\update-dll.ps1      # requiere Admin
```

Solo copia la DLL compilada a la carpeta de plugins, sin cambiar la versión ni generar un nupkg.

## Instalación / Desinstalación manual

```powershell
.\tools\install.ps1         # instala (requiere Admin)
.\tools\uninstall.ps1       # elimina (requiere Admin)
```

Localiza automáticamente la carpeta `C:\Program Files\GitExtensions\Plugins\` (o x86). La eliminación de la DLL no afecta a nada más de GitExtensions.

## Relacionado

- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[👁️ Visão Geral (ES)|Visión General]]
- [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]]
- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]]
