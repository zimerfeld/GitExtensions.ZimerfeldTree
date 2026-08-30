---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-08-29
tags: [build, versão, nupkg, deploy]
versao: 1.0.362
---

# Versioning and Build

## Version scheme

`major.minor.build` — only the `build` is incremented automatically by `build.ps1`. Major and minor are changed manually.

Current version: **1.0.362** *(source of truth: `.nuspec` / `.csproj`)*

> [!note] Incremental detection by timestamp
> `build.ps1` only increments the version (and recompiles/packs) if some **package entry** is newer than the last generated `.nupkg`. Entries = sources (`*.cs`/`*.csproj`/`*.nuspec`/`*.resx`/`*.png`), **any `*.md`** in the repo, and the packaged texts (`LICENSE`, scripts under `tools\`). The comparison is made against the `.nupkg` (and not the DLL) on purpose — when only a text changes, dotnet's incremental build may not rewrite the DLL, which would trigger the detection in a loop. Use `-Force` to pack even without changes.

## build.ps1 cycle

```
build.ps1
  │
  ├─ 1.  Reads current version from the .nuspec
  ├─ 1b. Detects changes (sources + texts) vs. last .nupkg → no changes ends
  ├─ 1c. Closes GitExtensions and plugins before compiling
  ├─ 2.  Bumps the .nuspec  ← <version>
  ├─ 3.  Bumps the .csproj  ← <Version>
  ├─ 4.  Updates the NuGet link and "Current version" in README.md
  ├─ 4b. Stamps the header (Version/Updated) into the READMEs (md / pt-BR / en-US)
  ├─ 4c. Stamps the Obsidian vault (notes that reflect the current version)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copies DLL → C:\Program Files\GitExtensions\Plugins\  (requires Admin)
  │       and updates tools\net9.0-windows\  (for the nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg at the root
  └─ —   Removes .nupkg from previous versions
```

> **Intentional order:** the docs (READMEs + vault) are stamped **before** the _pack_ (step 7), so the `.nupkg` remains the most recent artifact — which keeps the timestamp-based change detection correct and avoids a rebuild loop.

> Requires the `nuget` CLI (resolved via PATH → `tools\nuget.exe` → automatic download) and **Administrator** permission for the deploy. Without Admin, step 6 is skipped with a warning.

## Versioned files

| File | Updated field |
|---|---|
| `GitExtensions.ZimerfeldTree.nuspec` | `<version>` |
| `GitExtensions.ZimerfeldTree.csproj` | `<Version>` |
| `README.md` / `README.pt-BR.md` / `README.en-US.md` | `**Version/Versão:**`, `**Updated/Atualizado em:**` and "Current version" |
| Obsidian vault (Project, README mirror, Versioning, Overview) | frontmatter `versao:`/`atualizado:` and the "Current version" line |

> `build.ps1` logs each stamped note in the format `Obsidian: <file> updated to <version> (<date>)` (section 4c, loop over `$obsidianDocs`).

## NU5101 (intentional)

The DLL goes directly into `lib\` in the nupkg on purpose: the GitExtensions Plugin Manager only extracts the `lib` group whose framework is in its moniker list (`net5.0..net10.0`, `any`, `netstandard2.0`). Root `lib\` = "any" group (extracted); a `net9.0-windows` subfolder would break the install. That is why the `NU5101` warning is **filtered** from the `pack` output.

## Fast deploy (without incrementing the version)

```powershell
.\tools\update-dll.ps1      # requires Admin
```

Only copies the compiled DLL to the plugins folder, without changing the version or generating a nupkg.

## Manual install / uninstall

```powershell
.\tools\install.ps1         # installs (requires Admin)
.\tools\uninstall.ps1       # removes (requires Admin)
```

Automatically locates the `C:\Program Files\GitExtensions\Plugins\` folder (or x86). Removing the DLL does not affect anything else in GitExtensions.

## Related

- [[🌳 GitExtensions.ZimerfeldTree (EN)|GitExtensions.ZimerfeldTree]]
- [[👁️ Visão Geral (EN)|Overview]]
- [[📘 README — Instalação, Uso e Build (EN)|README — Installation, Usage and Build]]
- [[📦 Dependências do ZimerfeldTree (EN)|ZimerfeldTree Dependencies]]
