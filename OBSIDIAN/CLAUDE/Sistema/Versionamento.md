---
tipo: sistema
tags: [build, versão, nupkg, deploy]
atualizado: 2026-06-24
versao: 1.0.343
---

# Versionamento e Build

## Esquema de versão

`major.minor.build` — somente o `build` é incrementado automaticamente pelo `build.ps1`. Major e minor são alterados manualmente.

Versão atual: **1.0.343** *(fonte da verdade: `.nuspec` / `.csproj`)*

> [!note] Detecção incremental por timestamp
> O `build.ps1` só incrementa a versão (e recompila/empacota) se alguma **entrada do pacote** for mais nova que o último `.nupkg` gerado. Entradas = fontes (`*.cs`/`*.csproj`/`*.nuspec`/`*.resx`/`*.png`), **qualquer `*.md`** do repositório e os textos empacotados (`LICENSE`, scripts de `tools\`). A comparação é feita contra o `.nupkg` (e não a DLL) de propósito — quando só um texto muda, o build incremental do dotnet pode não regravar a DLL, o que dispararia a detecção em loop. Use `-Force` para empacotar mesmo sem mudanças.

## Ciclo build.ps1

```
build.ps1
  │
  ├─ 1.  Lê versão atual do .nuspec
  ├─ 1b. Detecta mudanças (fontes + textos) vs. último .nupkg → sem mudanças encerra
  ├─ 1c. Fecha GitExtensions e plugins antes de compilar
  ├─ 2.  Bump no .nuspec  ← <version>
  ├─ 3.  Bump no .csproj  ← <Version>
  ├─ 4.  Atualiza link do NuGet e "Versão atual" no README.md
  ├─ 4b. Carimba cabeçalho (Versão/Atualizado) nos READMEs (md / pt-BR / en-US)
  ├─ 4c. Carimba o cofre Obsidian (notas que refletem a versão atual)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia DLL → C:\Program Files\GitExtensions\Plugins\  (requer Admin)
  │       e atualiza tools\net9.0-windows\  (para o nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg na raiz
  └─ —   Remove .nupkg de versões anteriores
```

> **Ordem proposital:** os docs (READMEs + cofre) são carimbados **antes** do _pack_ (passo 7), então o `.nupkg` continua sendo o artefato mais recente — o que mantém a detecção de mudanças por timestamp correta e evita rebuild em loop.

> Requer `nuget` CLI (resolvido via PATH → `tools\nuget.exe` → download automático) e permissão de **Administrador** para o deploy. Sem Admin, o passo 6 é pulado com aviso.

## Arquivos versionados

| Arquivo | Campo atualizado |
|---|---|
| `GitExtensions.ZimerfeldTree.nuspec` | `<version>` |
| `GitExtensions.ZimerfeldTree.csproj` | `<Version>` |
| `README.md` / `README.pt-BR.md` / `README.en-US.md` | `**Version/Versão:**`, `**Updated/Atualizado em:**` e "Versão atual" |
| Cofre Obsidian (Projeto, README espelho, Versionamento, Visão Geral) | frontmatter `versao:`/`atualizado:` e a linha "Versão atual" |

> O `build.ps1` registra cada nota carimbada no formato `Obsidian: <arquivo> atualizado para <versão> (<data>)` (seção 4c, laço sobre `$obsidianDocs`).

## NU5101 (intencional)

A DLL fica diretamente em `lib\` no nupkg de propósito: o GitExtensions Plugin Manager só extrai o grupo `lib` cujo framework está na sua lista de monikers (`net5.0..net10.0`, `any`, `netstandard2.0`). `lib\` raiz = grupo "any" (extraído); uma subpasta `net9.0-windows` quebraria a instalação. Por isso o aviso `NU5101` é **filtrado** do output do `pack`.

## Deploy rápido (sem incrementar versão)

```powershell
.\tools\update-dll.ps1      # requer Admin
```

Só copia o DLL compilado para a pasta de plugins, sem alterar versão ou gerar nupkg.

## Instalação / Desinstalação manual

```powershell
.\tools\install.ps1         # instala (requer Admin)
.\tools\uninstall.ps1       # remove (requer Admin)
```

Localiza automaticamente a pasta `C:\Program Files\GitExtensions\Plugins\` (ou x86). A remoção da DLL não afeta nada mais do GitExtensions.

## Relacionado

- [[GitExtensions.ZimerfeldTree]]
- [[Visão Geral]]
- [[README — Instalação, Uso e Build]]
- [[Dependências do ZimerfeldTree]]
