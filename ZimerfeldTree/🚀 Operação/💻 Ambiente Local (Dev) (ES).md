---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [operacao, dev, build, install, powershell]
---

# 💻 Entorno Local (Dev)

> 🇧🇷 Lee esta página en portugués → [[💻 Ambiente Local (Dev) (PT)|💻 Ambiente Local (Dev)]]
> 🇺🇸 Read this page in English → [[💻 Ambiente Local (Dev) (EN)]]

> [!abstract] 🎯 Objetivo
> Compilar el plugin **ZimerfeldTree** e instalarlo en el GitExtensions local para desarrollar y probar. Contenido derivado de [[🔧 build.ps1 (ES)|🔧 build.ps1]], [[🏷️ Versionamento (ES)|Versionado]] y [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]].

## ⚡ TL;DR — el comando único

```powershell
# en la raíz del repo, como Administrador
.\build.ps1
```

Compila, incrementa la versión, instala la DLL en el GitExtensions local y genera el `.nupkg` — todo en un paso. Para iterar rápido sin tocar la versión:

```powershell
.\tools\update-dll.ps1      # solo copia la DLL compilada (requiere Admin)
```

## ⚙️ Qué hace el script (en orden)

```
build.ps1
  ├─ 1.  Lee la versión actual del .nuspec
  ├─ 1b. Detecta cambios (fuentes + textos) vs. el último .nupkg → sin cambios, termina
  ├─ 1c. Cierra GitExtensions y los plugins antes de compilar
  ├─ 2.  Bump en el .nuspec  ← <version>
  ├─ 3.  Bump en el .csproj  ← <Version>
  ├─ 4.  Actualiza el enlace de NuGet y la "Versión actual" en README.md
  ├─ 4b. Sella el encabezado (Versión/Actualizado) en los READMEs (md / pt-BR / en-US)
  ├─ 4c. Sella la bóveda de Obsidian (notas que reflejan la versión)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia la DLL → C:\Program Files\GitExtensions\Plugins\  (requiere Admin)
  │       y actualiza tools\net9.0-windows\  (para el nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg en la raíz
  └─ —   Elimina los .nupkg de versiones anteriores
```

## 🚩 Parámetros / flags

- `-Force` — empaqueta aunque no se detecten cambios (la detección incremental compara las entradas del paquete contra el último `.nupkg`, no contra la DLL — a propósito, para evitar rebuilds en bucle).

## 🧰 Scripts auxiliares (`tools\`)

| Script | Función |
|---|---|
| `install.ps1` | Instala la DLL en GitExtensions (Admin) |
| `uninstall.ps1` | Elimina la DLL (Admin) — no afecta nada más de GitExtensions |
| `update-dll.ps1` | Despliegue rápido solo de la DLL, sin bump de versión (Admin) |

## 📐 Reglas que respeta

- **Versionado** `major.minor.BUILD` — solo el `BUILD` se incrementa automáticamente; fuente de verdad: `.nuspec` / `.csproj`. Ver [[🏷️ Versionamento (ES)|Versionado]].
- **Docs sellados antes del pack** — los READMEs y la bóveda se actualizan antes del paso 7, manteniendo el `.nupkg` como artefacto más reciente (detección correcta por timestamp).
- **GitFlow** — desarrollo en feature branch (regla global de Renato); el build no interactúa con git.

## 🔧 Requisitos previos

- **.NET SDK 9** (compila `net9.0-windows`) y **NuGet CLI** (resuelto vía PATH → `tools\nuget.exe` → descarga automática).
- **GitExtensions 4.x** instalado en `C:\Program Files\GitExtensions\`.
- PowerShell **como Administrador** para el despliegue de la DLL.
- Detalles de instalación de cada dependencia: [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]].

## 🩹 Solución de problemas

- **Sin Admin** → el paso 6 (despliegue de la DLL) se omite con un aviso; ejecuta el terminal como Administrador.
- **"Sin cambios" y quieres empaquetar igualmente** → usa `.\build.ps1 -Force`.
- **Aviso NU5101 en el pack** → intencional y filtrado: la DLL queda directamente en `lib\` (grupo "any") porque el Plugin Manager de GitExtensions solo extrae ese grupo; una subcarpeta `net9.0-windows` rompería la instalación.
- **GitExtensions abierto** → el propio script cierra GitExtensions antes de compilar (paso 1c).
- **GitExtensions 3.x** → incompatible (.NET Framework 4.8); el plugin requiere `net9.0-windows`.

## 🔗 Enlaces

- [[🔧 build.ps1 (ES)|🔧 build.ps1]] — la nota del archivo clave
- [[🏷️ Versionamento (ES)|Versionado]] — esquema de versión y ciclo completo
- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]] — instalación paso a paso de las dependencias
- [[📘 README — Instalação, Uso e Build (ES)|README — Instalación, Uso y Build]] — espejo del README
- [[🚀 Deploy em Produção (Prod) (ES)|Despliegue en Producción (Prod)]] — publicación de la release
