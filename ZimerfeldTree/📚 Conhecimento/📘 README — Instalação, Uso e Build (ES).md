---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-18
tags: [conhecimento, readme, instalacao, build, uso, gitflow, hierarquia, i18n]
fonte: README.md
versao: 1.0.361
---

# README — Instalación, Uso y Build

> 🇧🇷 Lee esta página en portugués → [[📘 README — Instalação, Uso e Build]]
> 🇺🇸 Read this page in English → [[📘 README — Instalação, Uso e Build (EN)]]

> Espejo fiel del `README.md` de la raíz del repositorio (y de las variantes `README.en-US.md` / `README.pt-BR.md`).
> Nota de proyecto: [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]. Flujos detallados en [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]], [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]] y [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]].
> `build.ps1` sella versión + fecha en los READMEs **y en esta nota** (frontmatter `versao:`/`atualizado:`) en cada build — volver a espejar el cuerpo cuando el README cambie de forma significativa.

Versión actual: **1.0.361**

Plugin para **[GitExtensions](https://gitextensions.github.io/)** que muestra las branches del repositorio **jerárquicamente en árbol** (mostrando branches hijas) en lugar de la lista plana por defecto, y permite usar la metodología **GitFlow** de forma visual, muy fácil, intuitiva y agradable de aplicar en proyectos de cualquier tamaño.

## ✨ Funcionalidades de alto nivel
- **Árbol jerárquico de branches** — secciones **LOCAL**, **REMOTES** y **TAGS** combinando la ascendencia real de commits con la agrupación por ruta `/`; la branch actual en negrita, contadores en vivo y filtro en tiempo real.
- **GitFlow con un clic** — start/publish/track/update/finish para feature, release, hotfix, bugfix y support, con una jerarquía flexible que permite incluso *feature hija de feature* (el finish se propaga en cascada hasta `develop`).
- **Pull / Push / Commit manuales** — botones con iconos de flecha (↓ azul / ↑ verde) y contadores adelante/atrás, verificación del remoto en segundo plano al abrir y un aviso que **bloquea el push cuando la branch está atrasada**.
- **Restore — centro de "viaje en el tiempo"** — una ventana dedicada que reúne todas las formas seguras de recuperar o deshacer historial: restaurar archivo/árbol/tag, cherry-pick, **revertir**, crear branch/tag a partir de cualquier commit, **recuperación vía reflog**, descartar cambios locales y un rebase avanzado para eliminar un commit — cada uno con una explicación incorporada y orientación para el trabajo en equipo.
- **Localizado (Inglés / Portugués)** — cada ventana elige su idioma de forma independiente y lo recuerda.
- **Carga asíncrona** — la ventana se abre de inmediato con un overlay de progreso (0→100 %) mientras los datos se leen en segundo plano; el constructor no ejecuta git.
- **Selección múltiple mediante checkbox** + **Modo Developer** que protege `main`/`master`/`develop` de la eliminación cuando está desactivado.

## 🔀 GitFlow → git puro

El plugin ejecuta **solo git nativo** — no depende de que el binario `git-flow` esté instalado. Cada botón de la ventana GitFlow dispara la secuencia de comandos git equivalente (start, publish, track, update y finish para cada tipo de branch). Detalles en [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]].

### Jerarquía flexible — feature hija de feature
El GitFlow clásico no contempla feature hija de feature (todas las `feature/*` derivan de `develop` y son hermanas). **ZimerfeldTree GitFlow** permite que una `feature/*` derive de `develop` **o de otra `feature/*`** por encima de ella (mediante *based on:* en Start); en ese caso el *finish feature* **cascada** los cambios hasta el nodo padre de forma sucesiva, hasta llegar a `develop`.

## ⛔ Limitaciones de jerarquía
- **Agrupación por nombre (`/`), no por parentesco de commits** para el eje de carpetas — `master` y `develop` aparecen como hermanos.
- **Una branch real no puede ser nodo padre de otra branch** — si `feature/login` existe, crear `feature/login/oauth` falla (la ref sería archivo **y** directorio a la vez). Solución: nombres hermanos o un agrupador sin branch real.
- **Dos branches en exactamente el mismo commit no forman relación padre-hija** — solución automática: commit vacío en el Start con *based on*. Ver [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]].

## 🔌 Dependencias

### Obligatorias para el uso
| Programa | Versión mínima | Función |
|----------|---------------|--------|
| **Git for Windows** | cualquiera | Ejecuta todos los comandos git (elegir "Git from command line and also from 3rd-party software" en el PATH). |
| **GitExtensions** | 4.x (.NET 9) | App host que carga el plugin; proporciona los diálogos nativos de Commit/Push/Pull. |
| **Plugin ZimerfeldTree** | — | La DLL en `C:\Program Files\GitExtensions\Plugins\`. |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) es **incompatible** — el plugin requiere `net9.0-windows`.

### Condicionales — build / desarrollo
| Programa | Función |
|----------|--------|
| **.NET SDK 9** | Compilar `net9.0-windows` |
| **NuGet CLI** | Generar el `.nupkg` (usado por `build.ps1`) |

Ver también [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]].

## 📦 Instalación
**Opción A — Gestor de Plugins de GitExtensions (recomendado):** en **Plugins → Plugin Manager**, busque `GitExtensions.ZimerfeldTree` en el feed de nuget.org e instálelo; reinicie y abra **Plugins → ZimerfeldTree**. No requiere PowerShell ni permisos de Administrador. Depende de empaquetar la DLL en la raíz `lib\` (grupo "any") + `<dependency id="GitExtensions.Extensibility" version="[0.4.0, 0.5.0)">` — el rango debe **contener** la versión que anuncia el Plugin Manager (v3.x → 0.4.0). Ver [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]].

**Opción B — PowerShell (como Administrador):**
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

**Opción C — Manual:** copie `GitExtensions.Plugins.ZimerfeldTree.dll` a `C:\Program Files\GitExtensions\Plugins\` y reinicie GitExtensions.

## 🗑️ Desinstalación
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\uninstall.ps1
```
La eliminación de la DLL no afecta a ninguna otra parte de GitExtensions.

## 🛠️ Build y versionado
En cada ejecución de `build.ps1`, el script:
1. Lee la versión actual del `.nuspec` y detecta cambios (fuentes + `*.md`) frente al último `.nupkg`.
2. Calcula la nueva versión (incrementa el `build` en +1 → `major.minor.build`).
3. Escribe la nueva versión y fecha **primero en los docs** (READMEs + bóveda de Obsidian).
4. Incrementa el `.nuspec` y el `.csproj`.
5. Compila en Release.
6. Copia la DLL a `C:\Program Files\GitExtensions\Plugins\` *(requiere Administrador)* y a `tools\net9.0-windows\`.
7. Genera `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` y elimina los `.nupkg` de versiones anteriores.

```powershell
cd C:\GitExtensions\ZimerfeldTree
.\build.ps1
```

**Despliegue rápido (sin incrementar la versión):**
```powershell
.\tools\update-dll.ps1
```

Ver [[🏷️ Versionamento (ES)|Versionado]].

## 🤝 Plugins relacionados
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — genera automáticamente el mensaje de commit (Conventional Commits) resumiendo los archivos en staging. Por **zimerfeld**. GitHub: [zimerfeld/GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg).
- **[GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)** — gestiona Git LFS (Large File Storage): rastrear, subir y bajar archivos binarios grandes directamente desde la interfaz de GitExtensions. Por **zimerfeld**.

## 💜 Apoya el proyecto
Ayuda a mantener este proyecto siempre actualizado: **[GitHub Sponsors → zimerfeld](https://github.com/sponsors/zimerfeld)** · **[Ko-fi → Buy me a coffee ☕](https://ko-fi.com/C0D621FCGD)**.

## 📄 Licencia
[CC BY-NC-ND 4.0](LICENSE.txt)

## 🔗 Relacionado
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[👁️ Visão Geral (ES)|Visión general]]
- [[🏷️ Versionamento (ES)|Versionado]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]]
- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]]
- [[🔑 Fatos-Chave (ES)|Datos clave]]
