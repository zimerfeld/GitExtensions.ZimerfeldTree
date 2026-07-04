---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [arquivo, build, versionamento, deploy, powershell, nupkg]
arquivo: build.ps1
versao: 1.0.358
---

# build.ps1

Script de build, versionamento e deploy do plugin.

**Caminho:** `build.ps1` (raiz do repo)

---

## Ciclo

```
build.ps1
  ├─ 1.  Lê versão atual do .nuspec
  ├─ 1b. Detecta mudanças (fontes + textos) vs. último .nupkg → sem mudanças encerra
  ├─ 1c. Fecha GitExtensions e plugins antes de compilar
  ├─ 2.  Bump no .nuspec  ← <version>
  ├─ 3.  Bump no .csproj  ← <Version>
  ├─ 4.  Atualiza link do NuGet e "Versão atual" no README.md
  ├─ 4b. Carimba cabeçalho (Versão/Atualizado) nos READMEs (md / pt-BR / en-US)
  ├─ 4c. Carimba o cofre Obsidian (notas que refletem a versão)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia DLL → C:\Program Files\GitExtensions\Plugins\  (requer Admin)
  │       e atualiza tools\net9.0-windows\  (para o nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg na raiz
  └─ —   Remove .nupkg de versões anteriores
```

## Detalhes

- **Esquema:** `major.minor.BUILD` — só o `BUILD` é incrementado automaticamente. Fonte da verdade: `.nuspec` / `.csproj`.
- **Detecção incremental por timestamp:** compara entradas do pacote contra o último `.nupkg` (não a DLL, de propósito — evita rebuild em loop). `-Force` empacota mesmo sem mudanças.
- **NU5101 filtrado:** a DLL fica direto em `lib\` (grupo "any") de propósito; o aviso é suprimido do `pack`.
- Sem Admin, o passo 6 (deploy) é pulado com aviso.

## Scripts auxiliares (`tools\`)

- `install.ps1` / `uninstall.ps1` — instala/remove a DLL (Admin)
- `update-dll.ps1` — deploy rápido só da DLL, sem bump de versão

## Relacionado

- [[🏷️ Versionamento|Versionamento e Build]]
- [[📦 Dependências]]
