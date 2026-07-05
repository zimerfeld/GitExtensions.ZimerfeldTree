---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
tags: [operacao, prod, release, nupkg, nuget, github]
---

# 🚀 Production Deploy (Prod)

> [!abstract] 🎯 Goal
> Publish a **release** of the plugin: generate the versioned `.nupkg` and make it available to users (the **nuget.org** feed, from which the GitExtensions Plugin Manager installs, + a GitHub release). Content derived from [[🔧 build.ps1 (EN)|🔧 build.ps1]], [[🏷️ Versionamento (EN)|Versioning]] and [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]].

## ⚡ TL;DR — the single command

```powershell
# at the repo root, as Administrator
.\build.ps1
```

`build.ps1` is the production-artifact generator: it bumps the version, builds in Release, stamps the docs and produces `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` at the repo root (removing older `.nupkg` files). Then publish that `.nupkg` (see below).

## ⚙️ What the script does (in order)

1. Reads the current version from the `.nuspec` and detects changes (sources + `*.md`) vs. the last `.nupkg` — if nothing changed, it exits (use `-Force` to override).
2. Computes the new version (`major.minor.BUILD` — only `BUILD` auto-increments; major/minor are manual).
3. Stamps version + date **in the docs first**: READMEs (`md` / `pt-BR` / `en-US`) and Obsidian vault notes.
4. Bumps the `.nuspec` (`<version>`) and the `.csproj` (`<Version>`).
5. `dotnet build -c Release`.
6. Copies the DLL to `C:\Program Files\GitExtensions\Plugins\` (requires Admin) and to `tools\net9.0-windows\`.
7. `nuget pack` → `.nupkg` at the root; removes `.nupkg` files of previous versions.

## 📦 Publishing the release

1. **nuget.org** — publish `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` to the nuget.org feed (package `GitExtensions.ZimerfeldTree`). That feed is where the GitExtensions **Plugin Manager** installs the plugin from (recommended install option A in the README).
2. **GitHub release** — publish the matching release in the `zimerfeld` owner repository, attaching the generated `.nupkg`.
3. Check the README to confirm the NuGet link and "Current version" were stamped by `build.ps1` (automatic steps 4/4b).

> [!warning] ⚠️ nupkg requirements (do not change)
> - The DLL sits **directly in `lib\`** (the "any" group) — the Plugin Manager only extracts `lib` groups whose moniker is on its list; a `net9.0-windows` subfolder would break the installation (hence the NU5101 warning is filtered, intentionally).
> - `<dependency id="GitExtensions.Extensibility" version="[0.4.0, 0.5.0)">` — the range must **contain** the version the Plugin Manager advertises (v3.x → 0.4.0).

## 📐 Rules it follows

- **GitFlow** (global rule): finish the release by updating `develop` **and** `main`, create the **tag** and only then publish — never publish straight from a release branch.
- **Version**: source of truth is `.nuspec` / `.csproj`; docs (READMEs + vault) always in sync via `build.ps1`. See [[🏷️ Versionamento (EN)|Versioning]].
- **Adoption**: keep the clone/download counts of the `zimerfeld/GitExtensions.ZimerfeldTree` repo up to date (global portfolio rule).

## 🩹 Troubleshooting

- **Build exits with "no changes"** → `.\build.ps1 -Force`.
- **Deploy step skipped** → run as Administrator (without Admin, step 6 is skipped with a warning; the `.nupkg` is still generated).
- **Plugin Manager cannot find/install the package** → check the two nupkg requirements in the callout above (DLL in root `lib\` + `GitExtensions.Extensibility` dependency range).
- **User on GitExtensions 3.x** → incompatible; the plugin requires GitExtensions 4.x (.NET 9).

## 🔗 Links

- [[💻 Ambiente Local (Dev) (EN)|Local Environment (Dev)]] — local build and install
- [[🔧 build.ps1 (EN)|🔧 build.ps1]] — the key-file note
- [[🏷️ Versionamento (EN)|Versioning]] — version scheme, NU5101, versioned files
- [[📘 README — Instalação, Uso e Build (EN)|README — Install, Usage and Build]] — install options (Plugin Manager / script / manual)
- [[🌳 GitExtensions.ZimerfeldTree (EN)|🌳 GitExtensions.ZimerfeldTree]] — project mother note
