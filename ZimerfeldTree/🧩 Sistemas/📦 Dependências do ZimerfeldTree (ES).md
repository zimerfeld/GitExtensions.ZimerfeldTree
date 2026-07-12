---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-02
tags: [ferramenta, dependencias, instalacao, zimerfeldtree, gitextensions, git, gitflow]
---

# 🧩 Dependencias de ZimerfeldTree

> 🇧🇷 Lee esta página en portugués → [[📦 Dependências do ZimerfeldTree]]
> 🇺🇸 Read this page in English → [[📦 Dependências do ZimerfeldTree (EN)]]

> [!abstract] Resumen
> Lista completa de programas y plugins necesarios para ejecutar todas las funcionalidades del plugin **ZimerfeldTree**. Incluye la URL de descarga y el procedimiento de instalación para cada elemento.

---

## 1. Git for Windows

| Campo    | Valor |
|----------|-------|
| Papel    | Ejecuta **todos** los comandos git usados por el plugin (branch, checkout, pull, push, commit, tag, describe, flow…) |
| Obligatorio | ✅ Sí — sin git el plugin no funciona |
| Descarga | https://git-scm.com/download/win |

### Instalación
1. Descargar el instalador `.exe` (64-bit) y ejecutarlo.
2. En la pantalla **"Adjusting your PATH environment"** seleccionar **"Git from command line and also from 3rd-party software"** (permite que GitExtensions llame a `git` sin ruta completa).
3. Resto de opciones: por defecto.
4. Verificar: `git --version` en la terminal.

---

## 2. GitExtensions

| Campo    | Valor |
|----------|-------|
| Papel    | Aplicación **host** que carga el plugin vía MEF; provee los diálogos nativos de Commit (`StartCommitDialog`), Push (`StartPushDialog`) y Pull |
| Obligatorio | ✅ Sí — el plugin es una DLL cargada por GitExtensions |
| Versión mínima | 4.x (runtime .NET 9) |
| Descarga | https://github.com/gitextensions/gitextensions/releases |
| Sitio oficial | https://gitextensions.github.io/ |

### Instalación
1. Descargar el instalador `.msi` o `.exe` de la última release 4.x.
2. Ejecutar el instalador — verifica e instala el **.NET 9 Desktop Runtime** automáticamente si no está presente.
3. Tras instalar, el ejecutable queda en `C:\Program Files\GitExtensions\GitExtensions.exe`.
4. Verificar: abrir GitExtensions e ir a **Help → About**.

> [!warning] Versión
> Las versiones 3.x usan .NET Framework 4.8 y son incompatibles con el plugin (compilado para `net9.0-windows`).

---

## 3. Plugin ZimerfeldTree

| Campo    | Valor |
|----------|-------|
| Papel    | El plugin en sí — DLL cargada por GitExtensions en la carpeta `Plugins\` |
| Obligatorio | ✅ Sí |
| Repositorio | https://github.com/zimerfeld/ZimerfeldTree |
| DLL de destino | `C:\Program Files\GitExtensions\Plugins\GitExtensions.Plugins.ZimerfeldTree.dll` |

### Instalación (opción 1 — script automático como Admin)
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

### Instalación (opción 2 — manual)
1. Copiar `GitExtensions.Plugins.ZimerfeldTree.dll` a:
   ```
   C:\Program Files\GitExtensions\Plugins\
   ```
2. Reiniciar GitExtensions.
3. Verificar: menú **Plugins → ZimerfeldTree**.

### Build desde el código fuente
```powershell
# Requiere .NET SDK 9 y NuGet CLI (ver puntos 5 y 6 más abajo)
# Ejecutar como Administrador para el deploy automático
pwsh C:\GitExtensions\ZimerfeldTree\build.ps1
```

---

## 4. .NET SDK 9 *(solo para build/desarrollo)*

| Campo    | Valor |
|----------|-------|
| Papel    | Compilar el proyecto `GitExtensions.ZimerfeldTree.csproj` (`net9.0-windows`) |
| Obligatorio | ⚠️ Condicional — solo para compilar el código fuente |
| Descarga | https://dotnet.microsoft.com/download/dotnet/9.0 |

### Instalación
1. Descargar el instalador del **.NET 9 SDK** (no confundir con el Runtime).
2. Ejecutar el instalador.
3. Verificar: `dotnet --version` (debe devolver `9.x.x`).

---

## 5. NuGet CLI *(solo para build/desarrollo)*

| Campo    | Valor |
|----------|-------|
| Papel    | Generar el paquete `.nupkg` (usado por `build.ps1`) |
| Obligatorio | ⚠️ Condicional — solo para generar el paquete NuGet |
| Descarga | https://www.nuget.org/downloads |

### Instalación
1. Descargar `nuget.exe` (última versión estable).
2. Colocarlo en una carpeta del `PATH` (ej.: `C:\Program Files\NuGet\`).
3. Verificar: `nuget` en la terminal.

---

## Resumen rápido

> [!info] GitFlow sin dependencia externa
> Todos los comandos GitFlow (Start, Publish, Track, Update, Finish) usan **git puro**. El binario `git-flow` no necesita estar instalado.

| # | Programa / Plugin     | Obligatorio para uso | Para GitFlow | Para build |
|---|-----------------------|:--------------------:|:------------:|:----------:|
| 1 | Git for Windows       | ✅                   | ✅           | ✅         |
| 2 | GitExtensions 4.x     | ✅                   | ✅           | ✅         |
| 3 | Plugin ZimerfeldTree  | ✅                   | ✅           | —          |
| 4 | .NET SDK 9            | —                    | —            | ✅         |
| 5 | NuGet CLI             | —                    | —            | ✅         |

---

## 🔗 Relacionado
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz de GitFlow — botones y flujos]]
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
