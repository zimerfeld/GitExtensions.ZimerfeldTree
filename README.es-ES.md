# GitExtensions.ZimerfeldTree

![Icono](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/icon-128.png)

[![NuGet version](https://img.shields.io/nuget/v/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=NuGet)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/) &nbsp; [![NuGet downloads](https://img.shields.io/nuget/dt/GitExtensions.ZimerfeldTree?style=for-the-badge&logo=nuget&label=Downloads)](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/)

Este plugin se desarrolla y mantiene en mi tiempo libre. Si te ahorra tiempo gestionando branches, un patrocinio ayuda a mantenerlo actualizado para las nuevas versiones de GitExtensions. 💜

[![GitHub Sponsor](https://img.shields.io/badge/Sponsor-zimerfeld-EA4AAA?style=for-the-badge&logo=githubsponsors&logoColor=white)](https://github.com/sponsors/zimerfeld) &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; [![Ko-fi](https://img.shields.io/badge/Ko--fi-Buy%20me%20a%20coffee-FF5E2B?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/C0D621FCGD)

**Versión:** 1.0.361  
**Actualizado:** 2026-07-04

Un plugin de [GitExtensions](https://gitextensions.github.io/) que muestra las branches de forma **jerárquica** en una vista de árbol, incluyendo las branches hijas.

[English](README.en-US.md) | [Português](README.pt-BR.md) | [Español](README.es-ES.md)

[...More information](https://www.nuget.org/packages/GitExtensions.ZimerfeldTree "More information about GitExtensions.ZimerfeldTree package")

---

## Funcionalidades

### Vista jerárquica de branches

![ZimerfeldTree - BranchHierarchy](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBranchHierarchy.png)

- Ventana no modal que permanece abierta junto a GitExtensions. La barra de título es **`ZimerfeldTree - BranchHierarchy`**; las ventanas auxiliares son `ZimerfeldTree - GitFlow` y `ZimerfeldTree - Restore`.
- Árbol dividido en tres secciones fijas: **LOCAL**, **REMOTES** y **TAGS**.
- **LOCAL** y **REMOTES** combinan la ascendencia real de commits / organización GitFlow con la agrupación por ruta mediante `/`. Por ejemplo, `feature/test` aparece como `feature` -> `test`; cuando `feature/*` es hija de `develop`, el árbol muestra `develop` -> `feature` -> `test`.
- **TAGS** se agrupa por `/` sin cálculo de ascendencia.
- Las secciones vacías muestran `(no se encontraron branches locales)`.
- La ventana se abre centrada en la pantalla, tiene un tamaño fijo y expone los botones estándar de Windows para minimizar y cerrar. El botón de maximizar está deshabilitado.
- La ventana es independiente de GitExtensions: minimizar GitExtensions no afecta a BranchHierarchy.
- Carga asíncrona: la ventana aparece de inmediato y luego muestra un panel de progreso centrado mientras los datos del repositorio se leen en segundo plano.
- Construcción optimizada de la jerarquía: la detección de padres usa un único `git log --all` para construir el grafo de commits en memoria y resuelve los padres mediante BFS, evitando el antiguo cuello de botella O(N^2 x subproceso).
- Overlay de recarga explícita: el panel de progreso aparece en la primera apertura y durante recargas o mutaciones explícitas, como actualizar, checkout, nueva branch, merge, renombrar, eliminar, GitFlow, Restore, Pull/Push/Commit, cambios genuinos del repositorio en GitExtensions y cambios de repositorio.
- Lista de pasos de solo lectura: el overlay acumula los pasos en ejecución y mantiene visible el mensaje final «Completado.» durante un segundo.
- Botón Cancelar en el overlay: cancela la carga entre los pasos de git conservando los datos anteriores del árbol.
- El formulario se bloquea durante la carga y se vuelve a habilitar cuando la carga finaliza o se cancela.
- En la parte inferior de la ventana hay un botón **Cerrar** centrado; atajo: **Esc**.

### Selector de Working Directory y Branch

- La fila superior **Working Directory:** contiene una etiqueta fija, un ComboBox de solo selección poblado a partir del historial del dashboard de GitExtensions y una etiqueta `Branch: <nombre>` con la branch en checkout.
- Seleccionar otro repositorio recarga el árbol para ese working directory.
- La lista de repositorios se actualiza automáticamente cada vez que GitExtensions cambia de repositorio.
- La branch actual se resalta con texto en negrita y el color de selección del sistema.
- Las secciones del árbol muestran contadores: `LOCAL (N)`, `REMOTES (N)`, `TAGS (N)`.
- La barra de estado inferior muestra `Local: N | Remote: N | Tags: N`.

### Filtro en tiempo real

- El campo de búsqueda filtra branches en todas las secciones a la vez.
- Los nodos padre se conservan cuando contienen hijos que coinciden con la búsqueda.

### Botones Pull / Push / Commit / GitFlow / Restore

Se muestran encima del árbol cuando hay una branch en checkout:

- **Pull** / **Pull ↓N** ejecuta `git pull --tags`, trayendo los commits de la branch rastreada y todos los tags remotos. El botón muestra un **icono de flecha hacia abajo** en azul (sustituye al antiguo carácter `↓`); `↓N` es el número de commits remotos aún no descargados.
- **Push** / **Push ↑N** abre el diálogo nativo de Push de GitExtensions. El botón muestra un **icono de flecha hacia arriba** en verde (sustituye al antiguo carácter `↑`); `↑N` es el número de commits locales aún no enviados.
  - Cuando la branch en checkout está **por detrás** del remoto (`↓N > 0`), un aviso («tu branch está N commit(s) por detrás — hay que integrar primero») ofrece **hacer pull con rebase y luego push automáticamente** — `git pull --rebase` reaplica tus commits locales encima de los remotos (sin commit de merge), dejando la branch en fast-forward para que el push se acepte. Un rebase fallido (conflictos) se notifica y el push se omite; una branch ya actualizada se envía directamente.
- **Commit** / **Commit (N)** abre la ventana nativa de Commit de GitExtensions. `(N)` solo se muestra cuando hay cambios pendientes.
- **Contador de Commit en vivo** — la ventana **vigila la carpeta del working directory** (`FileSystemWatcher`, incluidas las subcarpetas) y actualiza el `(N)` del botón Commit **de forma silenciosa** a medida que creas, editas o eliminas archivos — sin reconstruir el árbol ni mostrar el overlay «Loading…». La ráfaga de eventos de un mismo guardado se agrupa (debounce de 600 ms) y después se ejecuta un único `git status` en segundo plano. Los cambios dentro de `.git` se ignoran (son irrelevantes para el contador y una fuente de ecos); `.gitignore`/`.gitattributes` sí cuentan con normalidad. El watcher se reapunta automáticamente al cambiar de repositorio.
- Al abrir la ventana, un `git fetch` en segundo plano del upstream de la branch actual actualiza los contadores fuera del hilo de la interfaz (la ventana se mantiene ágil y a salvo sin conexión), y la etiqueta `Branch: <nombre>` gana un sufijo `↓N` cuando hay commits por descargar.
- Tras un Push, un Pull o un Commit, el árbol se actualiza automáticamente y los contadores de los botones se recalculan.
- **GitFlow** abre la ventana de operaciones de GitFlow.
- **Restore** abre la ventana Restore con tres operaciones de recuperación de historial.
- **Eliminar** / **Eliminar (N)** elimina las branches o tags seleccionadas.
- Los iconos de los botones reutilizan los mismos iconos incrustados que usa el menú contextual (Pull/Push usan los nuevos iconos de flecha abajo/arriba).

### Selección múltiple y eliminación de branches

- Cada branch local, branch remota y tag tiene una casilla de verificación a la izquierda de su nombre. Los nodos de sección y las carpetas de ruta no son seleccionables.
- Marca uno o más elementos para eliminarlos en lote.
- El botón **Eliminar** cambia dinámicamente: `Eliminar` cuando no hay nada marcado, `Eliminar (N)` cuando hay elementos marcados.
- Con dos o más elementos marcados, la eliminación se ejecuta en lote con un único diálogo de confirmación.
- Con un elemento marcado, se elimina ese elemento.
- Sin elementos marcados, se elimina el nodo del árbol seleccionado.
- Para una branch local que no está completamente fusionada, se ofrece la eliminación forzada.
- Los tags se eliminan localmente con `git tag -d` y en el remoto con `git push <remote> --delete <tag>`.
- El menú contextual sigue las mismas reglas de selección.
- Tras la eliminación, el árbol se reconstruye y las casillas se limpian.

Flujo completo de eliminación en lote:

**1. Antes - elementos marcados** (el botón muestra `Eliminar (8)`):

![Before deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBeforeDelete.png)

**2. Confirmación única** que enumera cada elemento, con la opción **¿Eliminar también en remoto?**:

![Confirm deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotConfirmDelete.png)

**3. Durante la eliminación** - overlay de progreso con la lista de pasos y el botón **Abortar operación**:

![During deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotDuringDelete.png)

**4. Después** - el árbol se reconstruye sin los elementos eliminados y con los contadores actualizados:

![After deletion](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotAfterDelete.png)

#### Protección de branches principales y Modo Developer

- Las branches **main**, **master** y **develop**, locales o remotas, están protegidas por defecto y no se pueden seleccionar ni eliminar.
- La casilla **Modo Developer**, en la parte inferior de la ventana, desbloquea la selección y la eliminación de esas branches.
- Al desactivar el Modo Developer se desmarca automáticamente cualquier branch protegida.
- El Modo Developer se guarda en `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json` junto con **Show Debug**.

### Foco automático tras el Commit

- Tras cerrar la ventana de Commit de GitExtensions, BranchHierarchy recupera el foco automáticamente.
- El árbol se actualiza y la ventana pasa a primer plano.

### Casilla "Show Debug"

La casilla **Show Debug** activa tooltips de identificación para los controles del plugin:

- Línea 1 del tooltip: `TYPE: <tipo de control>`.
- Línea 2 del tooltip: `ID: <nombre interno>`.
- El tooltip de la propia ventana muestra `TYPE: BranchHierarchyForm` y `Handle: 0x<HWND>`.
- GitFlowForm también muestra su tipo y su handle cuando Show Debug está activado.
- Funciona en las ventanas BranchHierarchy, GitFlow y Restore.
- **Cada ventana persiste y recarga su propio estado de Show Debug de forma individual** — BranchHierarchy en `%APPDATA%\GitExtensions\ZimerfeldTree.uisettings.json`, GitFlow en `ZimerfeldTree.gitflowsettings.json` y Restore en `ZimerfeldRestore.settings.json`. La primera vez que se abre una ventana auxiliar (sin valor guardado), hereda el estado de BranchHierarchy.

### Selector de idioma

Un desplegable **Idioma** en la parte inferior de cada ventana cambia el texto de la interfaz en vivo entre **Automático** (sigue la configuración regional del SO), **Inglés**, **Portugués** y **Español**:

- **Cada ventana persiste y recarga su propio idioma de forma individual**, de modo que BranchHierarchy, GitFlow y Restore pueden mostrarse cada una en un idioma distinto al mismo tiempo. BranchHierarchy se guarda en `%APPDATA%\GitExtensions\ZimerfeldTree.language.json`, GitFlow en `ZimerfeldTree.gitflowsettings.json` y Restore en `ZimerfeldRestore.settings.json`.
- Una ventana se reabre en el último idioma establecido. La primera vez que se abre una ventana auxiliar (sin valor guardado), hereda el idioma de BranchHierarchy como valor por defecto.

### Casilla Modo Developer

La casilla **Modo Developer** controla la protección de las branches principales:

- **Desactivado por defecto:** **main**, **master** y **develop** están protegidas.
- **Activado:** esas branches específicas se pueden seleccionar y eliminar.
- Al desactivarlo se desmarca cualquier branch protegida que estuviera seleccionada.
- El ajuste se guarda junto con **Show Debug**.

### Persistencia del estado del árbol

- El estado de expansión y colapso se guarda por working directory, incluyendo los nodos raíz, las branches y las carpetas de ruta.
- Los nodos se identifican mediante una ruta estable, como `LOCAL|master|develop|feature`.
- El estado se guarda al expandir/colapsar nodos, con un debounce de 500 ms, y al cerrar la ventana.
- La primera apertura restaura el estado anterior del árbol una vez cargados los datos del repositorio.
- Los repositorios nuevos empiezan con LOCAL totalmente expandido y REMOTES/TAGS colapsados salvo sus raíces.
- Mientras se filtra, todos los nodos se expanden automáticamente para mostrar las coincidencias.

### Organización automática como GitFlow

- El plugin comprueba si la ascendencia real cumple las reglas de GitFlow: `master`/`main` en la raíz, `develop` bajo `master`, y `feature/*`, `release/*` y `hotfix/*` bajo los padres esperados.
- Cuando la jerarquía no coincide con GitFlow, el plugin aplica automáticamente la organización GitFlow en el árbol y muestra un aviso.
- El botón del aviso puede volver a mostrar la ascendencia real de git.
- La elección manual se respeta hasta que el repositorio cambia o se reabre la ventana.

### Actualización automática

- El árbol se recarga automáticamente cuando cambia la branch en checkout, cuando GitExtensions cambia de repositorio o cuando se inicializa/reabre un repositorio.
- El botón **Actualizar** dispara una recarga manual.

### Menú contextual

Cada elemento tiene un icono incrustado de 16 x 16 generado a partir de `Resources/ctx-*.png`:

| Icono                                                                                                                                                           | Elemento                    | Disponible para                                                                                                          |
| ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-pull.png" width="16" height="16">       | Pull (N)                    | Branch local - `N` = commits por detrás; primero hace checkout de la branch pulsada y después el pull                    |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-push.png" width="16" height="16">       | Push (N)                    | Branch local - `N` = commits por delante; primero hace checkout de la branch pulsada y después el push (pull-rebase y luego push cuando va por detrás) |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-commit.png" width="16" height="16">     | Commit (N)                  | Siempre - abre la ventana de Commit de GitExtensions; `N` es el número de cambios pendientes                             |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-checkout.png" width="16" height="16">   | Checkout                    | Local, remota, tag                                                                                                       |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-new-branch.png" width="16" height="16"> | Nueva branch desde aquí...  | Local, tag                                                                                                               |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-merge.png" width="16" height="16">      | Merge en la branch actual   | Local                                                                                                                    |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rebase.png" width="16" height="16">     | Rebase sobre la branch actual | Local                                                                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-rename.png" width="16" height="16">     | Renombrar...                | Local                                                                                                                    |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-delete.png" width="16" height="16">     | Eliminar...                 | Local, remota, tag                                                                                                       |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-gitflow.png" width="16" height="16">    | GitFlow...                  | Branch (local/remota/tag)                                                                                                |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-restore.png" width="16" height="16">    | Restore...                  | Cuando la branch actual no es `develop`                                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-expand.png" width="16" height="16">     | Expandir todo               | Siempre                                                                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-collapse.png" width="16" height="16">   | Colapsar todo                | Siempre                                                                                                                  |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/ctx-refresh.png" width="16" height="16">    | Actualizar                  | Siempre                                                                                                                  |

Los elementos **Pull/Push del menú contextual actúan sobre la branch en la que has hecho clic derecho** (no sobre HEAD): primero se hace checkout de la branch pulsada y después se ejecuta el pull/push, y sus contadores `(N)` muestran el propio estado de atraso/adelanto de esa branch. Un Push sobre una branch atrasada dispara el mismo aviso de pull-rebase-y-luego-push que el botón. Solo aparecen para branches locales y se sitúan justo antes de **Commit**. El menú emergente también muestra, en la parte superior, una **cabecera con la branch actualmente en checkout** (`Branch: <nombre>`).

El elemento **Commit** recalcula el número de cambios pendientes en el working tree cada vez que se abre el menú. Abre la ventana nativa de Commit en el proceso de GitExtensions ya en ejecución cuando es posible, de modo que los plugins de Commit Template ya estén cargados. Si el repositorio mostrado en BranchHierarchy difiere del repositorio activo en GitExtensions, se abre mediante un nuevo proceso como alternativa.

### Botón GitFlow Initialize

El botón **GitFlow Initialize** aplica las claves de configuración por defecto de GitFlow al repositorio actual:

| Clave                        | Valor por defecto |
| ---------------------------- | ------------------ |
| `gitflow.branch.main`        | `main`              |
| `gitflow.branch.develop`     | `develop`           |
| `gitflow.prefix.feature`     | `feature/`          |
| `gitflow.prefix.bugfix`      | `bugfix/`           |
| `gitflow.prefix.release`     | `release/`          |
| `gitflow.prefix.hotfix`      | `hotfix/`           |
| `gitflow.prefix.support`     | `support/`          |
| `gitflow.prefix.versiontag`  | _(vacío)_           |

Esto equivale a ejecutar `git config <key> <value>` para cada fila.

## Estructura del proyecto

```text
ZimerfeldTree/
|-- src/
|   `-- GitExtensions.ZimerfeldTree/
|       |-- ZimerfeldTreePlugin.cs             # MEF entry point (IGitPlugin)
|       |-- BranchHierarchyForm.cs             # Main branch hierarchy window
|       |-- GitFlowForm.cs                     # GitFlow operation window
|       |-- RestoreForm.cs                     # Restore window
|       |-- BranchHierarchyService.cs          # Git logic
|       |-- BranchNode.cs                      # Models
|       |-- NodeIcons.cs                       # Tree icons
|       |-- PluginIcon.cs                      # Plugin/window icon
|       |-- Resources/                         # Embedded PNGs
|       |-- GitExtensions.ZimerfeldTree.csproj
|       `-- GitExtensions.ZimerfeldTree.nuspec # NuGet package metadata
|-- build.ps1                                  # Build, versioning, and deploy script
|-- README.md                                  # Language selector
|-- README.pt-BR.md                            # Portuguese documentation
|-- README.es-ES.md                            # Spanish documentation
`-- README.en-US.md                            # English documentation
```

### Ventana GitFlow

![GitFlow window](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotGitFlow.png)

- Cerrar la ventana GitFlow vuelve a centrar BranchHierarchy y no dispara una actualización innecesaria, salvo tras finalizar una release, cuando el árbol se recarga una vez para enfocar el nuevo tag.
- Tras un **Start** con éxito, el panel "Manage existing branches" queda preseleccionado con la nueva branch.
- Tras cualquier acción de GitFlow que se complete con éxito, el árbol de BranchHierarchy se actualiza de inmediato mientras el foco permanece en GitFlow.
- La branch afectada queda en checkout y se muestra en la sección LOCAL del árbol.

### Reglas de Start y Finish por tipo

El diagrama resume, para cada tipo de branch, la **base usada en el Start**, la **branch creada** y el **destino del merge en el Finish**:

![Start and Finish rules per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotStartFinish.png)

- **feature** — parte de `develop` (u otra `feature/*`, opcional); finaliza en `develop` o en el padre based-on
- **bugfix** — parte de una `release/*` (**obligatorio** — un bugfix solo puede existir vinculado a una release, así que el Start se bloquea cuando no se elige ninguna o no existe ninguna) y queda **anidado bajo esa release** en el árbol; finaliza en esa **release (su padre)** — o en `develop` si la release ya no existe
- **release** — parte de `develop` (base fija); finaliza en `main` (`merge --no-ff` + tag) y en `develop`, haciendo push de main/develop/tag
- **hotfix** — parte de `main` (base fija); finaliza en `main` (`merge --no-ff` + tag) y en `develop`
- **support** — parte de un **tag** de producción (selección obligatoria); finaliza solo en `main`, sin tag y sin tocar `develop`
- Común a todo Finish: fetch opcional, eliminación de la branch local y remota (salvo **Keep**) y reconexión de los hijos en el árbol

El flujo completo de comandos `git` de cada tipo, desde el Start hasta el Finish:

![Full Start to Finish flow per type](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotFlowPerType.png)

### Jerarquía: cómo se posiciona el nodo en el árbol

Git solo guarda el commit-tip de cada branch, no su origen. Para anidar la nueva branch bajo su base, el Start usa uno de estos mecanismos:

![Hierarchy: empty commit and based-on override](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenShotHierarchyBasedOn.png)

- **commit vacío** (base = develop/main, based-on marcado) — `git commit --allow-empty` hace divergir el tip; la ascendencia real anida el nodo
- **based-on override** (base = `feature/*` personalizada, based-on marcado) — escribe `.git/zimerfeld-basedon.json` (enlace puramente visual, historial limpio)
- **regla GitFlow / de ruta** (sin based-on) — un simple `checkout -b`; el nodo se sitúa en el tip de la base y se agrupa por la regla de GitFlow + prefijo
- En el Finish, `RebaseBasedOnOnFinish` elimina el enlace de la branch finalizada y reapunta sus hijos hacia el destino del merge, manteniendo el árbol conectado

### Ventana GitFlow - branch base en el Start

El panel **Start branch** incluye una opción **based on:**:

- Cuando está deshabilitada, el plugin usa la branch base estándar para el tipo GitFlow seleccionado.
- Cuando está habilitada, la nueva branch parte de la branch seleccionada.
- Es útil para branches que deben quedar anidadas visualmente bajo otra branch.

### Ventana GitFlow - "Manage existing branches"

El plugin ejecuta comandos nativos de git directamente y **no** requiere el binario `git-flow`.

| Acción   | Comportamiento                                                            |
| -------- | --------------------------------------------------------------------------- |
| Publish  | Envía (push) la branch local seleccionada y establece el tracking upstream  |
| Track    | Crea una branch local de tracking a partir de la branch remota seleccionada |
| Update   | Trae (pull) las actualizaciones de la branch seleccionada                   |
| Finish   | Fusiona (merge) la branch seleccionada en el destino configurado y la elimina |

Comportamiento operativo:

- La ventana GitFlow mantiene el foco después de cada comando.
- BranchHierarchy se actualiza en segundo plano tras las operaciones exitosas.
- La branch afectada, o la branch resultante tras el finish, queda seleccionada en el árbol.
- Al abrir la ventana, si la branch en checkout coincide con un tipo de GitFlow, los desplegables de tipo y branch quedan preseleccionados.

#### Gestión de errores

Cuando un comando de git falla, la salida se muestra en la ventana y aparece un aviso. Si faltan branches de destino, el mensaje orienta al usuario hacia las branches existentes y la configuración `gitflow.branch.*`. Los conflictos de merge dejan el repositorio en estado de fusión y deben resolverse manualmente con `git merge --abort` o resolviendo los conflictos y haciendo commit.

### Ventana Restore

Se abre desde **Restore** — una ventana modal situada junto a BranchHierarchy. Es el centro de "volver atrás en el tiempo" para tu código: reúne **todas** las formas de recuperar, deshacer o descartar un estado del repositorio, cada una en su propia **pestaña**. Las pestañas están ordenadas **de la más segura a la más destructiva**, y los botones en **rojo** son irreversibles y piden confirmación.

> 💡 El razonamiento completo — incluyendo la categorización por seguridad y las recomendaciones de **trabajo en equipo** — está en el enlace **About Restore** (esquina superior derecha), que abre una ventana desplazable con la explicación completa.

Las **10 pestañas** están ordenadas **de la más segura a la más destructiva** — a continuación, la imagen y los **campos de cada pestaña**. Los botones en **rojo** son irreversibles y piden confirmación.

> Común a todas las pestañas: en la parte superior se muestra el **HEAD** actual; los campos **Commit hash** / **Entry** son desplegables que listan los commits/movimientos recientes como `(AAAA-MM-DD HH:mm:ss) [branch] hash  →  mensaje` (también puedes escribir un hash manualmente); el panel **Result** (fondo beige) muestra la salida de `git` en vivo.

#### 🟢 Seguras (no reescriben el historial)

##### Emergency Plan — lleva una branch al estado de un tag (release)

![Emergency Plan](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreEmergencyPlan.png)

- **Branch:** desplegable de la branch a restaurar/resetear — **por defecto la branch en checkout** (con reserva `develop` → `main` → `master`)
- **Tag:** desplegable del tag de referencia
- **Restore to Tag** → `git checkout <tag> -- .` — trae el contenido del tag como cambios _staged_; el historial queda intacto
- **Reset to Tag** (rojo) → `git reset --hard <tag>` — mueve el puntero de la branch y **descarta** todo lo posterior

##### Restore File — recupera UN archivo de un commit antiguo

![Restore File](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreFile.png)

- **Commit hash:** desplegable del commit de origen
- **File (relative path):** ruta del archivo dentro del repositorio; el botón **Browse…** abre el selector de archivos de Windows **ya situado en la carpeta del proyecto** y **rechaza** archivos fuera de la raíz del repositorio (convertidos a una ruta relativa con `/`)
- **Restore File** → `git checkout <hash> -- "<file>"` — el archivo recuperado queda _staged_

##### Restore Tree — recupera TODO el contenido rastreado de un commit

![Restore Tree](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreTree.png)

- **Commit hash:** desplegable del commit de origen
- **Restore Tree** → `git checkout <hash> -- .` — trae todo el árbol rastreado como cambios _staged_; el historial queda intacto

##### Cherry-Pick — aplica uno o más commits sobre la branch actual

![Cherry-Pick](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreCherry-Pick.png)

- **Commit hash:** desplegable; acepta un hash único **o un rango** `<antiguo>..<reciente>`
- **Apply Cherry-Pick** → `git cherry-pick <hash>`

##### Revert — deshace un commit creando un nuevo commit

![Revert](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreRevert.png)

- **Commit hash:** desplegable del commit a deshacer
- **Revert Commit** → `git revert <hash>` — crea un **nuevo commit** que deshace el elegido **sin reescribir el historial** (la forma correcta de deshacer algo **ya enviado al remoto**)
- **Revert Merge (-m 1)** → `git revert -m 1 <hash>` — deshace un **merge** completo

##### New Branch/Tag — crea una branch/tag (o inspecciona) a partir de un commit antiguo

![New Branch/Tag](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreNewBranchTag.png)

- **Commit hash:** desplegable del commit destino
- **Name:** nombre de la nueva branch/tag
- **Inspect** → `git checkout <hash>` — abre el código en ese commit en **HEAD desacoplado** (solo lectura; para volver, haz checkout de una branch)
- **Create Tag** → `git tag <name> <hash>` · **Create Branch** → `git branch <name> <hash>` — "bifurca" el pasado sin tocar ninguna branch existente

#### 🟡 Recuperación

##### Recover (Reflog) — lista todos los movimientos de HEAD

![Recover (Reflog)](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreRecoverReflog.png)

- **Entry:** desplegable de las entradas `HEAD@{n}` (commit, reset, rebase, checkout, merge)
- **Name:** nombre de la branch a crear
- **Create Branch Here** → `git branch <name> <entry>` — recupera una branch eliminada / un commit "perdido"
- **Reset Current Here** (rojo) → `git reset --hard <entry>` — la red de seguridad para un `reset --hard` que salió mal

#### 🟠 Descartar cambios locales (working tree)

##### Discard Local — descarta cambios sin commitear (historial intacto)

![Discard Local](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreDiscarLocal.png)

- **Discard unstaged changes (tracked)** → `git checkout -- .`
- **Reset --hard HEAD (discards staged + unstaged)** (rojo) → `git reset --hard HEAD`
- **Remove untracked files (clean -fd)** (rojo) → `git clean -fd`

#### 🔴 Reescribir historial (avanzado)

##### Reset Branch — mueve el puntero de una branch a un commit anterior

![Reset Branch](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreResetBranch.png)

- **Branch:** desplegable de la branch a resetear (por defecto la branch en checkout). Si no es la actual, el plugin hace `checkout`, aplica el reset y **vuelve automáticamente a la branch original**
- **Commit hash:** desplegable del commit destino
- **Mode** (opción): `--mixed` deshace commits y mantiene los cambios como **unstaged** (por defecto) · `--soft` los mantiene como **staged** · `--hard` **DESCARTA TODO** (irreversible, pide confirmación)
- **Reset Branch** (rojo) → `git reset --<mode> <hash>`

##### Rebase — elimina un commit específico del historial

![Rebase](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotRestoreRebase.png)

- **Commit hash:** desplegable del commit a eliminar
- **Remove Commit from History** (rojo) → `git rebase --onto <hash>^ <hash>` — reaplica los commits posteriores sobre el padre; en caso de conflicto, resuélvelo y ejecuta `git rebase --continue`
- **Abort Rebase** → `git rebase --abort` — vuelve al estado anterior

> ⚠️ **Nunca** reescribas (Reset --hard, Rebase) ni descartes el historial de una branch que ya tienen otras personas — rompe el repositorio de tus compañeros.

#### 👥 Trabajo en equipo (en About Restore)

- **Varios desarrolladores en la misma branch (p. ej. `main`):** para deshacer algo **ya enviado**, usa **Revert** (no Reset --hard); ejecuta `git pull` (preferiblemente `--rebase`) **antes** de hacer push para evitar el rechazo por _non-fast-forward_; Reset --hard/Rebase/Discard solo son seguros en trabajo **local** que nadie más tiene.
- **Varias branches que fusionar en `develop`:** usa **Cherry-Pick** para traer commits concretos; **Revert Merge (-m 1)** para deshacer un merge problemático conservando el resto; resuelve los conflictos con calma (**Abort Rebase** / `git merge --abort` vuelve al estado anterior); crea una **New Branch/Tag** a partir de un commit para aislar o retomar trabajo.

#### Comportamiento de la ventana

- Ventana **modal** situada junto a BranchHierarchy, ambas centradas en la pantalla (mismo comportamiento que la ventana GitFlow). Se ha **ensanchado** (980 px) y usa pestañas en **varias filas** para que **todas** sean visibles a la vez.
- Los desplegables de commit hash (en cada pestaña que lo necesita) listan los commits recientes como `(YYYY-MM-dd HH:mm:ss) [branch] hash  →  mensaje`, del más nuevo al más antiguo; cada lista se ajusta al ancho de su campo, dentro del margen derecho. El desplegable **Reflog** lista las entradas `HEAD@{n}` de la misma forma.
- Cada desplegable se abre con el aviso **Select...** / **Selecione...** y **no** se persiste, de modo que nunca se reutiliza silenciosamente un hash obsoleto.
- Al abrir, los dos desplegables de **branch** (Emergency Plan y Reset Branch) muestran por defecto la **branch actualmente en checkout** (con reserva: develop → main → master).
- El resultado de cada comando `git` aparece en vivo en el panel **Result** (fuente monoespaciada, fondo beige `#EFEBD8` como la consola nativa de GitExtensions, con desplazamiento automático hasta el final).
- Tras las operaciones exitosas, BranchHierarchy se actualiza en segundo plano sin robarle el foco a Restore.
- **Ningún desplegable se persiste** — cada combo se reabre en su valor por defecto cada vez. Solo se recuerdan entre aperturas los campos que no son combo (la ruta del archivo y el modo de reset), en `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json` (junto con el idioma y el Show Debug de esta ventana).
- El enlace **About Restore** abre una ventana **desplazable** con la explicación completa de cada pestaña, la categorización por seguridad y las recomendaciones de trabajo en equipo.
- Cerrar la ventana (botón **Close** o **Esc**) guarda los valores automáticamente; cerrar **no** dispara una actualización adicional (el árbol ya se actualizó en vivo) ni trae GitExtensions a primer plano.

### Iconos

El icono del plugin se usa en:

- El menú **Plugins** de GitExtensions.
- La barra de título de la ventana del plugin y la barra de tareas de Windows.

El icono del Árbol de la Vida es el PNG incrustado de 16 x 16 [`Resources/ico.png`](src/GitExtensions.ZimerfeldTree/Resources/ico.png), cargado una vez por [`PluginIcon.cs`](src/GitExtensions.ZimerfeldTree/PluginIcon.cs) y almacenado en caché durante la vida del proceso.

### Iconos por tipo de branch

Cada nodo del árbol recibe un icono de 16 x 16. Los tipos de GitFlow tienen iconos dedicados:

| Icono                                                                                                                                                    | Tipo de nodo          | Descripción                |
| ----------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- | --------------------------- |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/master.png" width="16" height="16">  | `master` / `main`      | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/develop.png" width="16" height="16"> | `develop`               | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/feature.png" width="16" height="16"> | carpeta `feature`      | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/folha.png" width="16" height="16">   | hijos de `feature/*`   | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/bugfix.png" width="16" height="16">  | `bugfix/*`              | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/release.png" width="16" height="16"> | `release/*`             | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/hotfix.png" width="16" height="16">  | `hotfix/*`              | Imagen personalizada incrustada |
| <img src="https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/src/GitExtensions.ZimerfeldTree/Resources/support.png" width="16" height="16"> | `support/*`             | Imagen personalizada incrustada |

#### Iconos personalizados

| Tipo                        | Recurso                        | Aspecto visual                 |
| ---------------------------- | ------------------------------- | -------------------------------- |
| sección **LOCAL**            | `Resources/local.png`           | monitor azul acero               |
| sección **REMOTES**          | `Resources/remotes.png`         | nube azul oscuro                 |
| sección **TAGS**             | `Resources/tags.png`            | etiqueta/cinta morada            |
| grupo remoto (`origin`)      | `Resources/origin.png`          | nube azul                        |
| branch remota                | `Resources/remote-branch.png`   | tenedor verde                    |
| tag                          | `Resources/tag.png`             | etiqueta verde azulado           |
| `master` / `main`            | `Resources/master.png`          | escudo dorado                    |
| `develop`                    | `Resources/develop.png`         | llave y martillo cruzados        |
| carpeta `feature`            | `Resources/feature.png`         | brote de branch                  |
| `feature/*`                  | `Resources/folha.png`           | hoja verde                       |
| `release/*`                  | `Resources/release.png`         | caja de paquete                  |
| `bugfix/*`                   | `Resources/bugfix.png`          | bicho rojo                       |
| `hotfix/*`                   | `Resources/hotfix.png`          | extintor rojo                    |
| `support/*`                  | `Resources/support.png`         | botiquín de primeros auxilios    |

El plugin sigue siendo autocontenido: las imágenes están incrustadas en la DLL y no dependen de archivos externos en la máquina del usuario.

### Atajos de teclado y ratón

- **Doble clic** en una branch o tag para hacer checkout.
- **Enter** hace checkout del nodo seleccionado.
- **Esc** cierra la ventana BranchHierarchy.
- El clic derecho abre el menú contextual.

### Ventana no modal persistente

- La ventana puede permanecer abierta durante el trabajo habitual con GitExtensions.
- Las acciones devuelven el foco a BranchHierarchy al completarse.
- GitFlow es la principal excepción, ya que abre su propia ventana modal y conserva el foco mientras está abierta.

## Dependencias

### Necesarias para el uso

| Dependencia               | Versión / ruta                                     | Propósito                                 |
| -------------------------- | ---------------------------------------------------- | ------------------------------------------- |
| **Windows**                 | Escritorio de Windows                                 | Host del plugin WinForms                    |
| **Git**                      | Disponible en el PATH o en la configuración de GitExtensions | Operaciones del repositorio           |
| **GitExtensions**            | 6.x (build .NET 9) — compilado y probado con la 6.0.5 | Aplicación host que carga el plugin        |
| **Plugin ZimerfeldTree**     | DLL instalada                                          | El propio plugin                            |

> **Compatibilidad:** el plugin tiene como destino `net9.0-windows` y se compila contra la API de extensibilidad de GitExtensions 6.0.5 (`GitExtensions.Extensibility`). Carga en la línea 6.x de `.NET 9`. GitExtensions 3.x (`.NET Framework 4.8`) es incompatible. Cuando se publique una nueva versión de GitExtensions, recompila contra ella y vuelve a ejecutar la prueba de humo (cargar la DLL, abrir la ventana de ZimerfeldTree, hacer un checkout/actualizar) antes de publicar.

### Condicionales - solo build/desarrollo

| Dependencia     | Propósito                          |
| ---------------- | ------------------------------------ |
| **.NET SDK**      | Compilar el proyecto                 |
| **NuGet CLI**     | Generar los paquetes `.nupkg`        |
| **PowerShell**    | Ejecutar los scripts de build e instalación |

## Instalación

### Opción A - Gestor de Plugins de GitExtensions (recomendado)

Dentro de GitExtensions, ve a **Plugins → Plugin Manager**, busca
**GitExtensions.ZimerfeldTree** en el feed de nuget.org y haz clic en instalar.
Reinicia GitExtensions y abre **Plugins → ZimerfeldTree**. No requiere PowerShell
ni permisos de Administrador.

### Opción B - PowerShell

Desde la raíz del repositorio:

```powershell
cd tools
.\install.ps1
```

![Installation via install.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotInstall.png)

Reinicia GitExtensions tras la instalación.

### Opción C - Manual

Compila o extrae:

```text
tools\net9.0-windows\GitExtensions.Plugins.ZimerfeldTree.dll
```

Copia `GitExtensions.Plugins.ZimerfeldTree.dll` en:

```text
C:\Program Files\GitExtensions\Plugins\
```

Reinicia GitExtensions. El plugin debería aparecer en **Plugins** y en **Settings -> Plugins -> ZimerfeldTree**.

## Desinstalación

```powershell
cd tools
.\uninstall.ps1
```

![Uninstall via uninstall.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUninstall.png)

Esto elimina la DLL del plugin de la carpeta Plugins de GitExtensions. El propio GitExtensions no se ve afectado.

## Actualización de la DLL

Usa el script de actualización para reemplazar solo la DLL instalada:

```powershell
cd tools
.\update-dll.ps1
```

![Update via update-dll.ps1](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotUpdate.png)

## Build

```powershell
# Incrementa la versión, compila y crea el .nupkg
# Ejecútalo como Administrador para copiar también la DLL en Plugins\
pwsh C:\NUGET\ZimerfeldTree\build.ps1
```

El script:

1. Lee e incrementa la versión en el `.nuspec`.
2. Sincroniza la versión del `.csproj`.
3. Compila en modo Release.
4. Copia la DLL en `C:\Program Files\GitExtensions\Plugins\` cuando se ejecuta como Administrador.
5. Empaqueta el `.nupkg` en la raíz del repositorio.

Build con éxito:

![Successful build](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotBuild.png)

Build sin cambios:

![Build with no changes](https://raw.githubusercontent.com/zimerfeld/ZimerfeldTree/main/ScreenShots/ScreenshotNoBuild.png)

## Jerarquía de branches - limitaciones

### La jerarquía se basa en la ascendencia de branches y la agrupación por ruta

El plugin combina la ascendencia de commits de git con la agrupación por ruta. El propio git no guarda de dónde se creó una branch; solo guarda a qué commit apunta cada una. Cuando la ascendencia es ambigua, la agrupación por ruta y las reglas de GitFlow ayudan a que el árbol sea legible.

### Una branch real no puede ser solo un nodo carpeta

Si el nombre de una branch es también el prefijo de otras branches, el árbol debe representar ambos conceptos: la branch real y la ruta de carpeta. Esto evita ocultar una ref real detrás de un nodo de agrupación puramente visual.

El propio git se niega a crear una branch anidada bajo una ref de branch existente — por ejemplo, crear `feature/login/oauth` cuando ya existe `feature/login` falla con `cannot lock ref … exists`, porque la misma ruta sería a la vez un archivo (la branch) y un directorio. Cuando esto ocurre a través del botón/menú **New branch**, ZimerfeldTree lo detecta y, en lugar del error crudo de git, muestra una explicación que sugiere un nombre hermano (`feature/login-oauth`) o una estructura de carpeta de agrupación (`feature/login/base` + `feature/login/oauth`).

### Jerarquía flexible de GitFlow — feature bajo feature

El GitFlow clásico no contempla una branch feature como hija de otra feature. GitFlow define una jerarquía fija en la que todas las branches `feature/*` derivan de `develop` y son hermanas entre sí. Las sub-features suelen gestionarse con commits separados en la misma branch o con branches hermanas que comparten un prefijo común.

**ZimerfeldTree GitFlow**, sin embargo, permite una jerarquía flexible en la que las branches `feature/*` pueden derivar de `develop` o de otra `feature/*` situada por encima de ellas (usa **based on:** en GitFlow → Start). En ese caso, finalizar una feature debe necesariamente **encadenar** todos sus cambios hacia arriba, hasta el nodo `feature/*` padre, reaplicando sucesivamente _finish feature_ hasta llegar a `develop`.

### Dos branches en el mismo commit no son padre e hija

Ejemplo:

```text
# No hierarchy - both point to commit c19d7dc
master
develop

# Hierarchy - gridsolo is one commit ahead
develop
`-- feature/gridsolo
```

Esto es una limitación de git, no del plugin.

Solución automática: al usar **based on:** en GitFlow -> Start, el plugin puede crear un commit vacío en la nueva branch:

```powershell
git commit --allow-empty -m "Start <branch>"
```

Eso le da a la nueva branch un commit-tip diferenciado, haciendo posible la detección de la relación padre-hija.

## Uso

1. Abre GitExtensions.
2. Ve a **Plugins -> ZimerfeldTree**.
3. Usa el árbol para inspeccionar, filtrar, hacer checkout, crear, hacer merge, rebase, renombrar, eliminar o ejecutar operaciones de GitFlow/Restore.

## Plugins integrados

### [GitExtensions.ZimerfeldCommitMsg](https://www.nuget.org/packages/GitExtensions.ZimerfeldCommitMsg)

Plugin de GitExtensions que genera automáticamente un mensaje de commit resumiendo los cambios staged en una frase al estilo Conventional Commits (`feat` / `fix` / `docs` / `test` / `chore`).

- **GitHub:** [zimerfeld/GitExtensions.ZimerfeldCommitMsg](https://github.com/zimerfeld/GitExtensions.ZimerfeldCommitMsg)

### [GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)

Plugin de GitExtensions para gestionar Git LFS (Large File Storage) — rastrear, enviar y descargar archivos binarios grandes directamente desde la interfaz de GitExtensions.

- **GitHub:** [zimerfeld/GitExtensions.ZimerfeldLFS](https://github.com/zimerfeld/GitExtensions.ZimerfeldLFS)

## Licencia

Copyright © 2026 Renato Zimerfeld — **CC BY-NC-ND 4.0** (ver [`LICENSE.txt`](LICENSE.txt)).
