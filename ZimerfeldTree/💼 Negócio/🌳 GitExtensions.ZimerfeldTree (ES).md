---
tipo: negocio
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-07-01 (push atrasado: el aviso ahora ofrece **Bajar con rebase y luego enviar automáticamente** — `git pull --rebase` reaplica los commits locales por encima de los remotos, sin merge, dejando la branch en fast-forward; método de servicio `PullRebase` + `DoPullRebaseThenPush`) | 2026-06-27 (contador de Commit en vivo: un FileSystemWatcher en la carpeta del working directory actualiza el `(N)` del botón Commit silenciosamente, con debounce e ignorando `.git`) | 2026-06-26 (financiación: FUNDING.yml con github+ko_fi, badges de NuGet versión/descargas y frase "por qué donar" en los READMEs) | 2026-06-18 (doc: GitFlow flexible — feature hija de feature; finish en cascada hasta develop. 1.0.323: iconos Pull/Push en los botones y el menú; verificación del remoto al abrir vía fetch de la branch actual; el menú Pull/Push actúa en la branch clicada; un aviso bloquea el push cuando la branch está atrasada; encabezado con la branch en checkout en el menú contextual)
tags: [projeto, csharp, gitextensions, plugin, winforms]
status: ativo
linguagem: C#
versao: 1.0.361
repo: C:\GitExtensions\GitExtensions.ZimerfeldTree
---

# 🌳 GitExtensions.ZimerfeldTree

> 🇧🇷 Lee esta página en portugués → [[🌳 GitExtensions.ZimerfeldTree]]
> 🇺🇸 Read this page in English → [[🌳 GitExtensions.ZimerfeldTree (EN)]]

> [!info] Esta nota es un espejo del `README.md` del repositorio
> El contenido del README (funcionalidades, dependencias, instalación, estructura y limitaciones) vive aquí, en la bóveda. Los **flujos detallados de cada ventana** están en [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]], [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]] y [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]].

## 💜 Apoya el proyecto / Financiación
Canales de donación (botón **Sponsor** + badges en la parte superior de los READMEs):
- **GitHub Sponsors:** [zimerfeld](https://github.com/sponsors/zimerfeld) · **Ko-fi:** [C0D621FCGD ☕](https://ko-fi.com/C0D621FCGD)
- **`.github/FUNDING.yml`:** declara `github: zimerfeld` **y** `ko_fi: C0D621FCGD` (muestra el botón nativo Sponsor con ambos).
- **Prueba social en el README:** badges de versión + **descargas de NuGet** (`shields.io/nuget/v` y `/dt` del paquete `GitExtensions.ZimerfeldTree`) + una frase corta de "por qué donar" (mantenimiento en tiempo libre + compatibilidad con las nuevas versiones de GitExtensions).

## 🎯 Objetivo
Plugin para **[GitExtensions](https://gitextensions.github.io/)** que muestra las branches del repositorio **jerárquicamente** en árbol (mostrando branches hijas), en lugar de la lista plana por defecto. Tiene su propio icono "Árbol de la Vida" dibujado/embebido (GDI+ / `Resources/ico.png`).

## 📂 Estructura del proyecto
```
C:\GitExtensions\ZimerfeldTree\
├─ src\GitExtensions.ZimerfeldTree\        # código del plugin
│   ├─ ZimerfeldTreePlugin.cs              # punto de entrada MEF (IGitPlugin)
│   ├─ BranchHierarchyForm.cs              # ventana principal: árbol jerárquico de branches
│   ├─ GitFlowForm.cs                      # ventana Git Flow: start/publish/track/update/finish
│   ├─ RestoreForm.cs                      # ventana Restore: 10 pestañas de "viajar en el tiempo" (restore/revert/reset/reflog/rebase…)
│   ├─ BranchHierarchyService.cs           # lógica git: recolección, jerarquía, Git Flow
│   ├─ BranchNode.cs                       # modelos: clase BranchInfo + enum BranchType
│   ├─ NodeIcons.cs                        # iconos 16×16 del árbol (GDI+ + PNGs embebidos)
│   ├─ PluginIcon.cs                       # icono del plugin/ventana (Resources/ico.png)
│   ├─ Resources\                          # PNGs embebidos (iconos de nodos, menú y plugin)
│   ├─ GitExtensions.ZimerfeldTree.csproj
│   └─ GitExtensions.ZimerfeldTree.nuspec  # metadatos del paquete NuGet
├─ build.ps1                               # build + versionado + deploy
├─ README.md                               # documentación completa
└─ ZimerfeldTree\                          # 🧠 esta bóveda de memoria
```

## ⚙️ Stack técnico
- **Lenguaje:** C# (`net9.0-windows`), `Nullable` + `ImplicitUsings`, `LangVersion=latest`
- **UI:** WinForms (`UseWindowsForms`)
- **Tipo de salida:** `Library` (DLL cargada por GitExtensions, no un exe)
- **AssemblyName:** `GitExtensions.Plugins.ZimerfeldTree`
- **Namespace raíz:** `GitExtensions.ZimerfeldTree`
- **Modelo de plugin:** MEF (`System.ComponentModel.Composition`) — ver [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
- **Referencias externas** (de `C:\Program Files\GitExtensions\`, `Private=false`, no copiadas):
  - `GitExtensions.Extensibility.dll`
  - `GitUIPluginInterfaces.dll`
  - `System.ComponentModel.Composition.dll`

## 📄 Archivos fuente (`src\GitExtensions.ZimerfeldTree\`)
| Archivo | Líneas | Rol |
|---------|-------:|-------|
| `BranchHierarchyForm.cs` | ~2066 | Ventana principal no modal (la mayor parte de la UI) |
| `BranchHierarchyService.cs` | ~831 | Ejecuta comandos git y parsea la salida |
| `GitFlowForm.cs` | ~758 | Ventana modal que dirige los comandos `git flow` (git puro) |
| `RestoreForm.cs` | ~1473 | Ventana modal: 10 pestañas de recuperación/deshacer (restore archivo/árbol/tag, cherry-pick, revert, reset, nueva branch/tag, reflog, descartar, rebase) |
| `NodeIcons.cs` | ~381 | Iconos 16×16 GDI+ + PNGs embebidos (ImageList) |
| `ZimerfeldTreePlugin.cs` | ~238 | Punto de entrada MEF del plugin (IGitPlugin) |
| `BranchNode.cs` | ~41 | Modelos: clase `BranchInfo` + enum `BranchType` (Local/Remote/Tag) |
| `PluginIcon.cs` | ~33 | Icono del plugin/ventana (`Resources/ico.png`), cargado 1 vez y cacheado |
| `*.nuspec` / `*.csproj` | — | Manifiestos NuGet/MSBuild (leídos por `build.ps1`) |

### 🖼️ Resources (`src\GitExtensions.ZimerfeldTree\Resources\`)
| Grupo | Archivos | Uso |
|-------|----------|-----|
| Plugin/ventana | `ico.png` | Icono "Árbol de la Vida" (menú Plugins + barra de título) |
| Secciones del árbol | `local.png`, `remotes.png`, `tags.png` | Encabezados LOCAL / REMOTES / TAGS |
| Nodos de branch | `master.png`, `develop.png`, `feature.png`, `folha.png`, `release.png` | Iconos por tipo de branch GitFlow |
| Remote / tag | `origin.png`, `remote-branch.png`, `tag.png` | Grupo de remote (cohete), branch remota, tag |
| Menú contextual | `ctx-checkout.png`, `ctx-collapse.png`, `ctx-commit.png`, `ctx-delete.png`, `ctx-expand.png`, `ctx-gitflow.png`, `ctx-merge.png`, `ctx-new-branch.png`, `ctx-pull.png`, `ctx-push.png`, `ctx-rebase.png`, `ctx-refresh.png`, `ctx-rename.png`, `ctx-restore.png` | Iconos del menú contextual del árbol. `ctx-pull` (flecha ↓ azul) / `ctx-push` (flecha ↑ verde) también se usan en los botones Pull/Push — generados vía Pillow (ver `tools\make_pull_push_icons.py`) |

> Cada `<EmbeddedResource>` es **condicional a la existencia del archivo** (`Condition="Exists(...)"`). En runtime, `NodeIcons.LoadEmbedded` lee el recurso por `GitExtensions.ZimerfeldTree.Resources.<archivo>` y lo redimensiona a 16×16. Si falta o es ilegible, cae al **glifo GDI+ de reserva** — el build nunca se rompe por falta de la imagen.

## ✨ Funcionalidades principales
- Ventana **no modal**, singleton por sesión, se abre **centrada** y es redimensionable (`Sizable`), independiente de GitExtensions. Título de la barra: **`ZimerfeldTree - BranchHierarchy`** (auxiliares: `ZimerfeldTree - GitFlow`, `ZimerfeldTree - Restore`) — el prefijo **ZimerfeldTree** se mantiene siempre, seguido del nombre específico de la ventana. `BranchHierarchyForm` es solo el nombre interno de la clase C#
- Árbol en 3 secciones fijas: **LOCAL**, **REMOTES**, **TAGS**, con contadores `(N)` y barra de estado `Local: N | Remoto: N | Tags: N`
- LOCAL/REMOTES combinan **ancestralidad real** (parentesco por commits / GitFlow) **+ agrupación por ruta** (`/`). Ej.: `feature/teste` → carpeta `feature` → hoja `teste`
- **Carga asíncrona**: la ventana se abre inmediatamente con los controles renderizados pero vacíos (sin datos calculados) + un overlay de progreso (0→100%), una lista acumulativa de los 8 pasos, botón Cancelar, formulario bloqueado durante la carga. El constructor **no** ejecuta git; todo se lee en segundo plano (`Task.Run`) disparado por `Shown` (`FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true, finalDelay:false)`). En las recargas el overlay se cierra 1 s después de "Completado."; en la **1ª apertura** se cierra en cuanto el árbol se puebla (sin el retraso)
- **Jerarquía optimizada:** un único `git log --all` construye el grafo de commits en memoria, padres vía BFS → **O(commits)** en lugar de O(N²×subproceso)
- **Overlay solo en la 1ª visualización y en recargas explícitas** — no aparece al reactivar tras cerrar GitFlow/Restore (el árbol ya está actualizado en vivo) ni en el eco del propio `NotifyRepoChanged`
- Selector de **Working Directory** (combo leído de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings`) y **branch actual en negrita** + color de resalte
- **Filtro en tiempo real** en todas las secciones (subcadena sin distinguir mayúsculas/minúsculas), preservando nodos padre con hijos coincidentes
- **Botones Pull / Push / Commit / Eliminar / GitFlow / Restore** por encima del árbol (cuando hay una branch en checkout); contadores `↓N` / `↑N` / `(N)`. **Pull/Push muestran iconos de flecha** (↓ azul / ↑ verde) en lugar de los antiguos caracteres `↓`/`↑`. Actúan sobre **HEAD**
- **Contador de Commit en vivo** — un `FileSystemWatcher` sobre la carpeta del working directory (con subcarpetas) actualiza el `(N)` del botón Commit **silenciosamente** al crear/editar/borrar archivos, sin reconstruir el árbol ni mostrar overlay. Las ráfagas se agrupan con un debounce de 600 ms → un único `git status` en segundo plano; los cambios en `.git` se ignoran (evita eco; `.gitignore`/`.gitattributes` cuentan). Se reapunta al cambiar de repo. Detalle en [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
- **Verificación del remoto al abrir** — un `git fetch` del upstream de la branch actual se ejecuta en segundo plano después de que aparece la ventana (seguro sin conexión al abrir); corrige los contadores Pull/Push y añade `↓N` a la etiqueta `Branch:`
- **Push protegido contra divergencia** — si la branch a enviar está **atrasada** respecto al remoto, un aviso ofrece **Bajar con rebase y luego enviar automáticamente** (`git pull --rebase` reaplica los commits locales por encima de los remotos, sin commit de merge → branch en fast-forward → push aceptado); un rebase con conflicto se reporta y el push se omite, evitando el rechazo `non-fast-forward`
- **Selección múltiple por checkbox** — cada branch (local/remota) y tag tiene un checkbox (las secciones y carpetas no); marcar 2 o más habilita la eliminación en lote. El botón **Eliminar** cambia a `Eliminar (N)` y el menú contextual se reduce a **Eliminar + Actualizar**
- **Checkbox "Modo Developer"** (junto a Show Debug) — **apagado (por defecto):** `main`/`master`/`develop` quedan **protegidas**, con el checkbox bloqueado (no se pueden marcar ni eliminar); **encendido:** habilita marcar/eliminar esas branches específicas. Desactivar el modo **desmarca automáticamente** cualquier main/master/develop marcada. El estado se persiste en `ZimerfeldTree.uisettings.json`
- **Foco automático tras Commit** — la ventana recupera el foco y actualiza el árbol al cerrarse la ventana de Commit
- **Checkbox "Show Debug"** — tooltips `TYPE:`/`ID:` en todos los controles (y el Handle de la ventana); el estado se persiste en `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`
- **Persistencia del estado del árbol** (expandir/colapsar) por Working Directory en `ZimerfeldTree.treestate.json` — una ruta estable por nodo (ej.: `LOCAL|master|develop|feature`), debounce de 500 ms + guardado al cerrar, restaurado en el `Shown` de la 1ª apertura
- **Organización automática como GitFlow** — detecta jerarquía fuera del estándar y se auto-organiza; botón "Restaurar jerarquía real" / "Organizar como GitFlow"
- **Actualización automática** en checkout, cambio de repositorio, init/reapertura; botón **Actualizar** manual
- **Menú contextual** con iconos embebidos (Pull, Push, Commit, Checkout, Nueva branch, Merge, Rebase, Renombrar, Eliminar, GitFlow…, Restore…, Expandir/Colapsar, Actualizar) + **encabezado en la parte superior** con la branch en checkout. **Pull/Push actúan sobre la branch clicada** (haciéndole checkout primero), con contadores propios
- **Botón GitFlow Initialize** — aplica de una vez las claves `gitflow.*` estándar (ver [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]])
- **Restore** (`RestoreForm`) — el centro de "viajar en el tiempo" (980 px, 10 pestañas, de la más segura a la más destructiva): Plan de Emergencia (branch←tag), Restaurar Archivo (con **Examinar…** restringido a la raíz del repo), Restaurar Árbol, Cherry-Pick, **Revertir** (commit / merge -m 1), Reset Branch, **Nueva Branch/Tag** (+Inspeccionar detached), **Recuperar (Reflog)**, **Descartar Locales** (checkout/reset --hard HEAD/clean), **Rebase** (elimina commit). **Acerca de Restore** = ventana desplazable con explicación por categoría + trabajo en equipo

> Detalles control a control: [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]] · [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]] · [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]].

![[ScreenshotGitFlow.png]]

## 🔄 Comandos GitFlow → git puro

El plugin ejecuta **solo git nativo** — no depende de que esté instalado el binario `git-flow`.
Cada botón de la ventana GitFlow dispara la siguiente secuencia:

### Start
| Tipo | Comando git |
|------|-------------|
| `feature`, `release` | `git checkout -b <prefijo><nombre> develop` |
| `bugfix` | `git checkout -b <prefijo><nombre> <release/*>` — **base release obligatoria** |
| `hotfix`, `support` | `git checkout -b <prefijo><nombre> main` |
| cualquiera (con based on marcado) | `git checkout -b <prefijo><nombre> <base elegida>` |

> **Regla del bugfix:** un bugfix **solo puede existir vinculado a una release**. `DoStart` bloquea el Start si no hay ninguna release o si la base elegida no es una `release/*`; la base release escribe un *based-on override* → el bugfix queda **anidado bajo la release** en el árbol (también vía `BuildGitFlowParentMap`, que usa la ancestralidad real para encontrar la release padre). Un bugfix fuera de esta regla se convierte en una **violación** (`violLocalBugfix`) que dispara la auto-organización GitFlow.
> **based on:** permite feature-hija-de-feature; en ese caso el plugin también ejecuta `git commit --allow-empty -m "chore: start <prefijo><nombre>"` para que la jerarquía sea visible (ver Limitaciones).
> **Nombre por defecto de release/hotfix:** al elegir el tipo `release` o `hotfix`, el nombre se rellena previamente con `yyyyMMddHHmm` (solo si el campo está vacío).

### Publish
```
git push --set-upstream <remote> <prefijo><nombre>
```

### Track
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout -b <prefijo><nombre> --track <remote>/<prefijo><nombre>
```

### Update
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout <prefijo><nombre>
git merge <remote>/<padre>                             # (o git merge <padre> si No fetch)
```
> Padre = `develop` para feature/release; `main` para hotfix/support; **la release (padre)** para bugfix (a partir del ref local; con fallback a develop)

### Finish — feature / bugfix
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout <destino>                                 # feature: develop o padre based-on
                                                       # bugfix: release (padre based-on), o develop si la release no existe
git merge --no-ff <prefijo><nombre>
git branch -d <prefijo><nombre>                        # (si Keep está desmarcado)
git push <remote> --delete <prefijo><nombre>            # (solo si la branch remota existe)
```

### Finish — hotfix
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout main
git merge --no-ff hotfix/<nombre>
git tag -a <nombre> -m "<nombre>"
git checkout develop
git merge --no-ff hotfix/<nombre>
git branch -d hotfix/<nombre>                          # (si Keep está desmarcado)
git push <remote> --delete hotfix/<nombre>             # (solo si la branch remota existe)
```

### Finish — release (flujo completo automático)
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout main
git merge --no-ff release/<nombre>
git tag -a <nombre> -m "<nombre>"
git checkout develop
git merge --no-ff release/<nombre>
git branch -d release/<nombre>                          # (si Keep está desmarcado)
git push <remote> --delete release/<nombre>            # (solo si la branch remota existe)
git push <remote> main
git push <remote> develop
git push <remote> refs/tags/<nombre>
git checkout develop
```
> Al finalizar, la sección **TAGS** se expande y el foco va a la tag creada. Remote = `origin` (o el primero configurado).

### Finish — support
```
git fetch <remote>                                     # (si No fetch está desmarcado)
git checkout main
git merge --no-ff support/<nombre>
git branch -d support/<nombre>                          # (si Keep está desmarcado)
git push <remote> --delete support/<nombre>            # (solo si la branch remota existe)
```

> **Errores de merge** (conflicto): el plugin se detiene y muestra el resultado. El repositorio queda en estado "merging" — resolver con `git merge --abort` o resolver los conflictos y `git commit`.

## 🔌 Dependencias

### Obligatorias para el uso
| Programa | Versión mínima | Función |
|----------|---------------|--------|
| **Git for Windows** | cualquiera ([descarga](https://git-scm.com/download/win)) | Ejecuta todos los comandos git. En la pantalla *"Adjusting your PATH"* elegir **"Git from command line and also from 3rd-party software"** |
| **GitExtensions** | 4.x (.NET 9) ([releases](https://github.com/gitextensions/gitextensions/releases)) | App host que carga el plugin; provee los diálogos nativos de Commit/Push/Pull. El instalador `.msi` instala el .NET 9 Desktop Runtime |
| **Plugin ZimerfeldTree** | — | La DLL en `C:\Program Files\GitExtensions\Plugins\` |

> [!warning] GitExtensions 3.x (.NET Framework 4.8) es **incompatible** — el plugin requiere `net9.0-windows`.

### Condicional — build / desarrollo
| Programa | Función |
|----------|--------|
| **.NET SDK 9** ([descarga](https://dotnet.microsoft.com/download/dotnet/9.0)) | Compilar `net9.0-windows` |
| **NuGet CLI** ([descarga](https://www.nuget.org/downloads)) | Generar el `.nupkg` (usado por `build.ps1`) |

Ver también [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]].

## 🛠️ Build / instalación
```powershell
# Build + empaqueta el nupkg (gestiona la versión major.minor.BUILD). Como Admin también copia la DLL.
.\build.ps1
# Scripts auxiliares en tools\
tools\install.ps1      # instala el plugin
tools\uninstall.ps1    # lo elimina
tools\update-dll.ps1   # actualiza solo la DLL
```
`build.ps1`: (1) lee e incrementa `<version>` en el nuspec; (2) sincroniza `<Version>` en el csproj; (3) actualiza `README.md`; (4) compila en Release; (5) si es Admin, copia la DLL a `C:\Program Files\GitExtensions\Plugins\`; (6) ejecuta `nuget pack`.

Build completado con éxito (versión incrementada, DLL copiada y `.nupkg` generado):

![[ScreenshotBuild.png]]

Cuando **no se detecta ningún cambio** en las fuentes, el script mantiene la versión y omite build/pack:

![[ScreenshotNoBuild.png]]

**Instalación manual:** copiar `GitExtensions.Plugins.ZimerfeldTree.dll` a `C:\Program Files\GitExtensions\Plugins\` y reiniciar GitExtensions.

`tools\install.ps1` (como Admin):

![[ScreenshotInstall.png]]

**Desinstalación:** borrar esa DLL (no afecta a GitExtensions). Vía `tools\uninstall.ps1`:

![[ScreenshotUninstall.png]]

**Actualizar solo la DLL:** `tools\update-dll.ps1` (como Admin) — copia la DLL recién compilada a `Plugins\` sin reinstalar:

![[ScreenshotUpdate.png]]

## ⛔ Limitaciones de la jerarquía de branches
- **La agrupación es por nombre (`/`), no por parentesco de commits** para el eje de carpetas — `master` y `develop` aparecen como hermanas; para anidar por nombre usa `/`.
- **Una branch real no puede ser nodo padre de otra branch** — si existe `feature/login`, crear `feature/login/oauth` falla (`cannot lock ref … exists`), ya que el ref sería a la vez archivo **y** directorio. Solución: nombres hermanos (`feature/login-oauth`) o un agrupador sin branch real (`feature/login/base` + `feature/login/oauth`).
- **GitFlow flexible — feature bajo feature** — El GitFlow clásico no contempla una branch feature como hija de otra feature (todas las `feature/*` derivan de `develop` como hermanas). **ZimerfeldTree GitFlow** rompe esa rigidez: una `feature/*` puede derivar de `develop` **o de otra `feature/*` por encima de ella** (vía **based on:** en el Start). Consecuencia: finalizar una feature así debe **propagarse en cascada** hasta el nodo `feature/*` padre, reaplicando sucesivamente *finish feature* hasta llegar a `develop`.
- **Dos branches en exactamente el mismo commit no forman relación padre-hijo** — el BFS de ancestralidad nunca encuentra a una como padre de la otra; ambas se convierten en raíces. Solución automática: commit vacío en el Start con **based on**. Detalle en [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]].

## 🐛 Trampas conocidas
> [!warning] MSB3277 (WindowsBase)
> Las DLLs de GitExtensions traen WindowsBase 8.0 mientras que el ref pack de net9 provee 4.0. El runtime resuelve la correcta en tiempo de carga → el csproj **rebaja MSB3277 a mensaje** (`MSBuildWarningsAsMessages`). Es benigno.

> [!warning] Git Flow mostrando "Init Gitflow"
> GitExtensions escribe la config en su propio formato interno, pero el git flow CLI espera otras claves. Solución en [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]].

## 🔢 Versionado
- Versión actual: **1.0.361** (README + csproj + nuspec + bóveda en sincronía)
- Esquema: `major.minor.BUILD`, gestionado por `build.ps1`
- ⚠️ Mantener el csproj y el nuspec en sincronía

## 🎨 Iconos (NodeIcons.cs)
- Iconos 16×16 generados en runtime vía GDI+, con varios **PNGs embebidos** y un fallback dibujado. Índices en `NodeIcons`: 0–4 genéricos, 5–7 secciones, 8–15 GitFlow/hoja.
- **El grupo de remote (`origin`)** usa `Resources\origin.png` (cohete) vía `NodeIcons.Remote` — mapeado en `GetFolderIconIndex`.
- **Develop (índice 9)** usa `Resources\develop.png`, fallback `Wrench()`.

## 🔗 Plugins integrados (mismo autor)
- **[GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)** — genera automáticamente el mensaje de commit (Conventional Commits) resumiendo los archivos staged. GitHub: [zimerfeld/GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg).
- **[GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)** — gestiona Git LFS (Large File Storage): rastrear, enviar y descargar archivos binarios grandes directamente desde la interfaz de GitExtensions.

## 🔗 Relacionado
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]]
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
- [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]]
- [[📦 Dependências do ZimerfeldTree (ES)|Dependencias de ZimerfeldTree]]
- [[🔑 Fatos-Chave (ES)|Datos Clave]]
