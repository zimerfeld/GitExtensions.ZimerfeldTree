---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [operacao, prod, release, nupkg, nuget, github]
---

# 🚀 Despliegue en Producción (Prod)

> 🇧🇷 Lee esta página en portugués → [[🚀 Deploy em Produção (Prod) (PT)|🚀 Deploy em Produção (Prod)]]
> 🇺🇸 Read this page in English → [[🚀 Deploy em Produção (Prod) (EN)]]

> [!abstract] 🎯 Objetivo
> Publicar una **release** del plugin: generar el `.nupkg` versionado y ponerlo a disposición de los usuarios (el feed de **nuget.org**, desde donde el Plugin Manager de GitExtensions instala, + una release en GitHub). Contenido derivado de [[🔧 build.ps1 (ES)|🔧 build.ps1]], [[🏷️ Versionamento (ES)|Versionado]] y [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]].

## ⚡ TL;DR — el comando único

```powershell
# en la raíz del repo, como Administrador
.\build.ps1
```

`build.ps1` es el generador del artefacto de producción: incrementa la versión, compila en Release, sella los docs y produce `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` en la raíz del repo (eliminando los `.nupkg` antiguos). Después, publica ese `.nupkg` (ver abajo).

## ⚙️ Qué hace el script (en orden)

1. Lee la versión actual del `.nuspec` y detecta cambios (fuentes + `*.md`) vs. el último `.nupkg` — sin cambios, termina (usa `-Force` para forzar).
2. Calcula la nueva versión (`major.minor.BUILD` — solo el `BUILD` se incrementa; major/minor son manuales).
3. Sella versión + fecha **primero en los docs**: READMEs (`md` / `pt-BR` / `en-US`) y notas de la bóveda de Obsidian.
4. Hace el bump en el `.nuspec` (`<version>`) y en el `.csproj` (`<Version>`).
5. `dotnet build -c Release`.
6. Copia la DLL a `C:\Program Files\GitExtensions\Plugins\` (requiere Admin) y a `tools\net9.0-windows\`.
7. `nuget pack` → `.nupkg` en la raíz; elimina los `.nupkg` de versiones anteriores.

## 📦 Publicación de la release

1. **nuget.org** — publicar el `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` en el feed de nuget.org (paquete `GitExtensions.ZimerfeldTree`). Es de ese feed de donde el **Plugin Manager** de GitExtensions instala el plugin (opción A recomendada de instalación del README).
2. **GitHub release** — publicar la release correspondiente en el repositorio del owner `zimerfeld`, adjuntando el `.nupkg` generado.
3. Comprobar en el README que el enlace de NuGet y la "Versión actual" fueron sellados por `build.ps1` (paso automático 4/4b).

> [!warning] ⚠️ Requisitos del nupkg (no modificar)
> - La DLL queda **directamente en `lib\`** (grupo "any") — el Plugin Manager solo extrae los grupos `lib` cuyo moniker está en su lista; una subcarpeta `net9.0-windows` rompería la instalación (por eso el aviso NU5101 se filtra, intencionalmente).
> - `<dependency id="GitExtensions.Extensibility" version="[0.4.0, 0.5.0)">` — el rango debe **contener** la versión que anuncia el Plugin Manager (v3.x → 0.4.0).

## 📐 Reglas que respeta

- **GitFlow** (regla global): finalizar la release actualizando `develop` **y** `main`, crear la **tag** y solo entonces publicar — nunca publicar directamente desde una release branch.
- **Versión**: la fuente de verdad es `.nuspec` / `.csproj`; los docs (READMEs + bóveda) siempre en sincronía vía `build.ps1`. Ver [[🏷️ Versionamento (ES)|Versionado]].
- **Adopción**: mantener actualizado el recuento de clones/descargas del repo `zimerfeld/GitExtensions.ZimerfeldTree` (regla global del portafolio).

## 🩹 Solución de problemas

- **El build termina con "sin cambios"** → `.\build.ps1 -Force`.
- **Paso de despliegue omitido** → ejecutar como Administrador (sin Admin, el paso 6 se omite con un aviso; el `.nupkg` se genera igualmente).
- **El Plugin Manager no encuentra/instala el paquete** → verificar los dos requisitos del nupkg en el callout de arriba (DLL en `lib\` raíz + rango de la dependency `GitExtensions.Extensibility`).
- **Usuario en GitExtensions 3.x** → incompatible; el plugin requiere GitExtensions 4.x (.NET 9).

## 🔗 Enlaces

- [[💻 Ambiente Local (Dev) (ES)|Entorno Local (Dev)]] — build e instalación local
- [[🔧 build.ps1 (ES)|🔧 build.ps1]] — la nota del archivo clave
- [[🏷️ Versionamento (ES)|Versionado]] — esquema de versión, NU5101, archivos versionados
- [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]] — opciones de instalación (Plugin Manager / script / manual)
- [[🌳 GitExtensions.ZimerfeldTree (ES)|🌳 GitExtensions.ZimerfeldTree]] — nota madre del proyecto
