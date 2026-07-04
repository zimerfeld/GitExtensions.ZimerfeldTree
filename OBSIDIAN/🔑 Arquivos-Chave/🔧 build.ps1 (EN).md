---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [arquivo, build, versionamento, deploy, powershell, nupkg]
arquivo: build.ps1
versao: 1.0.358
---

# build.ps1

Build, versioning, and deploy script for the plugin.

**Path:** `build.ps1` (repo root)

---

## Cycle

```
build.ps1
  ├─ 1.  Reads current version from the .nuspec
  ├─ 1b. Detects changes (sources + texts) vs. last .nupkg → no changes ends
  ├─ 1c. Closes GitExtensions and plugins before compiling
  ├─ 2.  Bumps the .nuspec  ← <version>
  ├─ 3.  Bumps the .csproj  ← <Version>
  ├─ 4.  Updates the NuGet link and "Current version" in README.md
  ├─ 4b. Stamps the header (Version/Updated) into the READMEs (md / pt-BR / en-US)
  ├─ 4c. Stamps the Obsidian vault (notes that reflect the version)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copies DLL → C:\Program Files\GitExtensions\Plugins\  (requires Admin)
  │       and updates tools\net9.0-windows\  (for the nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg at the root
  └─ —   Removes .nupkg from previous versions
```

## Details

- **Scheme:** `major.minor.BUILD` — only the `BUILD` is incremented automatically. Source of truth: `.nuspec` / `.csproj`.
- **Incremental detection by timestamp:** compares package entries against the last `.nupkg` (not the DLL, on purpose — to avoid a rebuild loop). `-Force` packs even without changes.
- **NU5101 filtered:** the DLL goes directly into `lib\` (the "any" group) on purpose; the warning is suppressed from `pack`.
- Without Admin, step 6 (deploy) is skipped with a warning.

## Auxiliary scripts (`tools\`)

- `install.ps1` / `uninstall.ps1` — installs/removes the DLL (Admin)
- `update-dll.ps1` — fast deploy of the DLL only, without a version bump

## Related

- [[🏷️ Versionamento (EN)|Versioning and Build]]
- [[📦 Dependências (EN)|Dependencies]]
