---
tipo: arquivo-chave
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquivo, build, versionamento, deploy, powershell, nupkg]
arquivo: build.ps1
versao: 1.0.358
---

# build.ps1

> 🇧🇷 Lee esta página en portugués → [[🔧 build.ps1 (PT)|🔧 build.ps1]]
> 🇺🇸 Read this page in English → [[🔧 build.ps1 (EN)]]

Script de build, versionado y deploy del plugin.

**Ruta:** `build.ps1` (raíz del repo)

---

## Ciclo

```
build.ps1
  ├─ 1.  Lee la versión actual del .nuspec
  ├─ 1b. Detecta cambios (fuentes + textos) vs. el último .nupkg → sin cambios finaliza
  ├─ 1c. Cierra GitExtensions y los plugins antes de compilar
  ├─ 2.  Bump en el .nuspec  ← <version>
  ├─ 3.  Bump en el .csproj  ← <Version>
  ├─ 4.  Actualiza el enlace de NuGet y la "Versión actual" en el README.md
  ├─ 4b. Sella el encabezado (Versión/Actualizado) en los READMEs (md / pt-BR / en-US)
  ├─ 4c. Sella el cofre de Obsidian (notas que reflejan la versión)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia la DLL → C:\Program Files\GitExtensions\Plugins\  (requiere Admin)
  │       y actualiza tools\net9.0-windows\  (para el nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg en la raíz
  └─ —   Elimina los .nupkg de versiones anteriores
```

## Detalles

- **Esquema:** `major.minor.BUILD` — solo el `BUILD` se incrementa automáticamente. Fuente de la verdad: `.nuspec` / `.csproj`.
- **Detección incremental por timestamp:** compara las entradas del paquete contra el último `.nupkg` (no la DLL, a propósito — para evitar un bucle de rebuild). `-Force` empaqueta incluso sin cambios.
- **NU5101 filtrado:** la DLL queda directamente en `lib\` (grupo "any") a propósito; el aviso se suprime del `pack`.
- Sin Admin, el paso 6 (deploy) se omite con un aviso.

## Scripts auxiliares (`tools\`)

- `install.ps1` / `uninstall.ps1` — instala/elimina la DLL (Admin)
- `update-dll.ps1` — deploy rápido solo de la DLL, sin bump de versión

## Relacionado

- [[🏷️ Versionamento (ES)|Versionado y Build]]
- [[📦 Dependências (ES)|Dependencias]]
