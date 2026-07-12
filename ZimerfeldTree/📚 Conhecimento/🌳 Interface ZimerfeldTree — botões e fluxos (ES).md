---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-06-28 (un push atrasado ahora ofrece Bajar-con-rebase-y-luego-enviar vía `DoPullRebaseThenPush`/`PullRebase`, en lugar de solo bloquear) | 2026-06-16 (iconos Pull/Push en los botones y en el menú; fetch de la branch actual al abrir; menú Bajar/Enviar actuando sobre la branch clicada; aviso que bloquea el push atrasado; cabecera con la branch en checkout en el menú)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, zimerfeldtree]
fonte: src\GitExtensions.ZimerfeldTree\BranchHierarchyForm.cs
---

# Interfaz ZimerfeldTree — botones y flujos

> 🇧🇷 Lee esta página en portugués → [[🌳 Interface ZimerfeldTree — botões e fluxos]]
> 🇺🇸 Read this page in English → [[🌳 Interface ZimerfeldTree — botões e fluxos (EN)]]

> [!abstract] Resumen
> Ventana **no modal** (`BranchHierarchyForm`) que muestra LOCAL / REMOTES / TAGS en árbol jerárquico y permanece abierta junto a GitExtensions. Este documento describe **cada control** y **el paso a paso exacto** de cada acción. Para la ventana `git flow` ver [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]. Proyecto: [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].

![[ScreenshotBranchHierarchy.png]]

## 🚪 Cómo se abre la ventana
- El menú **Plugins → ZimerfeldTree** llama a `ZimerfeldTreePlugin.Execute`.
- El formulario es **singleton** por sesión de GitExtensions: si ya existe, solo actualiza el working dir y lo trae al frente; si no, crea uno nuevo.
- `Execute` devuelve `false` → GitExtensions **no** actualiza su propia UI (la ventana gestiona su propio estado).
- El plugin se suscribe a los eventos del host (`Register`): `PostBrowseInitialize` → cambio de repositorio; `PostCheckoutBranch` / `PostCheckoutRevision` → reconstruye el árbol; `PostCommit` → reconstruye + enfoca; `PostRepositoryChanged` → `OnExternalChange`. Así el árbol se mantiene sincronizado automáticamente.
- `OnExternalChange` llama a `NotifyExternalRepoChanged()`: refresca ante cambios externos **genuinos**, pero **ignora el eco** de nuestro propio `NotifyRepoChanged` (flag `_suppressEcho`) — evita un refresh redundante / parpadeo del overlay.
- El **overlay solo aparece en la primera visualización** (`VisibleChanged` guarda `_initialLoadDone`): reactivar la ventana después de cerrar GitFlow/Restore **no** dispara el overlay (el árbol ya está actualizado).
- **Verificación del remoto tras la apertura** (`Shown` → `RefreshRemoteStatusAsync`): la carga inicial es **segura sin conexión** y muestra el ahead/behind del último fetch; después de que la ventana aparece, un `git fetch` del upstream de la branch actual se ejecuta **fuera del hilo de UI** (`FetchCurrentBranchUpstream` → `git fetch <remote> <branch>`), recalcula el tracking y corrige los botones Pull/Push y la etiqueta `Branch:`. Best-effort: un fallo de red o la ausencia de upstream se ignora.

## 🧭 Diseño (de arriba a abajo)
1. **Panel superior** — etiqueta "Working Directory:", combo de repositorios (`_cboRepo`), etiqueta "Branch: \<actual\>".
2. **Panel de filtro** — cuadro "Filtrar branches..." (`_txtFilter`) + botón **↺** (`_btnRefresh`).
3. **Panel de aviso** (oculto por defecto) — aviso de GitFlow + botón **Organizar como GitFlow / Restaurar jerarquía real** (`_btnGitFlow`).
4. **Panel de botones GitFlow** — **Pull**, **Push**, **Commit**, **GitFlow**, **Restore** (solo aparecen si hay una branch actual).
5. **Árbol** (`TreeView`) — 3 secciones fijas: **LOCAL (n)**, **REMOTES (n)**, **TAGS (n)**.
6. **Panel inferior** — botón **Cerrar** (centrado).
7. **Barra de estado** — `Local: n | Remoto: n | Tags: n`.
8. **Overlay de carga** — flota sobre todo durante la carga.

## 🔄 Carga del árbol (`RefreshTreeAsync`)
Disparado por: la **primera** visualización de la ventana (`VisibleChanged`, solo en la carga inicial), el botón ↺, el menú "Actualizar", un cambio de repo, los eventos de checkout/commit/repo-changed del host (con el propio eco suprimido), y tras cada mutación local.
Pasos (con % en el overlay):
1. `10%` branches locales — `git branch --format=%(refname:short)`
2. `30%` branches remotas — `git branch -r --format=%(refname:short)`
3. `50%` tags — `git tag --sort=-version:refname`
4. `65%` jerarquía local — `BuildParentMap` (1× `git log --all --format="%H %P"` + BFS)
5. `80%` jerarquía remota — `BuildRemoteParentMap`
6. `92%` sincronización — `git for-each-ref ...%(upstream:track)` → rellena ahead/behind
7. `96%` cambios pendientes — `git status --porcelain`, leído **en segundo plano** (fuera del hilo de UI); el valor se **reutiliza** en `UpdateCommitActionTexts(pending)` para el contador `Commit (n)`, sin un segundo `git status` en el hilo de UI
8. `100%` Completado.
- Las llamadas concurrentes se **coalescen** (`_isRefreshing`); un refresh en curso se **cancela** antes de iniciar otro.
- Los errores se convierten en `MessageBox`. Cancelar restaura la UI sin tocar el árbol existente.
- La branch actual aparece en **negrita + color de resalte**, con indicadores de tracking: `(↓M↑N)` (↓ atrás / ↑ adelante) solo cuando hay divergencia.

## 🌲 Estructura del árbol
- LOCAL y REMOTES combinan **dos ejes**: anidamiento vertical por **ascendencia real de commits** (`parentMap`) + agrupación horizontal por **`/` en el nombre** (p. ej., `feature/teste` → carpeta `feature` → hoja `teste`).
- REMOTES se subdivide por remoto (`origin`, ...).
- El estado de expansión se **persiste por repositorio** en `%APPDATA%\GitExtensions\ZimerfeldTree.treestate.json` (guardado con un debounce de 500 ms; restaurado al reabrir). Durante el filtrado, todo se expande.

## ⚙️ Modo GitFlow forzado (auto-organización)
- `GetGitFlowViolations()` comprueba las reglas esperadas: `master/main` raíz; `develop` hijo de master; `feature/*` hijo de develop (lo mismo para cada remoto).
- Si hay violaciones **y el usuario aún no ha elegido manualmente**, el árbol **se auto-organiza** en el diseño GitFlow (`BuildGitFlowParentMap` / `BuildGitFlowRemoteParentMap`: master=raíz, develop→master, feature/release→develop, hotfix→master).
- El panel de aviso muestra el número de violaciones o "mostrando organización GitFlow".

---

## 🖱️ Botones y acciones — paso a paso

### Combo de repositorios (`_cboRepo`)
1. `SelectedIndexChanged`: si cambió → define `WorkingDir`, **reactiva la auto-organización** de GitFlow, y `RefreshTree()`.
- La lista proviene de `%APPDATA%\GitExtensions\GitExtensions\GitExtensions.settings` (el historial de repositorios) + el working dir actual.

### Cuadro de filtro (`_txtFilter`)
1. `TextChanged` → `ApplyFilter`: reconstruye las 3 secciones filtrando por **subcadena** (sin distinguir mayúsculas/minúsculas) en el nombre completo; expande todo mientras haya un filtro.

### Botón ↺ (`_btnRefresh`)
1. Llama a `RefreshTree()` → recarga todo con el overlay (ver arriba).

### Botón "Organizar como GitFlow" / "Restaurar jerarquía real" (`_btnGitFlow`)
1. Marca `_gitFlowUserToggled = true` (una elección manual desactiva la auto-organización).
2. Invierte `_gitFlowForced`.
3. `RefreshTree()` → el árbol se reconstruye en el diseño elegido.

### Botón Pull (`_btnPull`) → `DoPull` — actúa sobre el **HEAD**
1. Deshabilita el botón.
2. En segundo plano: `git pull --tags`.
3. En la UI: rehabilita el botón, `RefreshTree()`, `NotifyRepoChanged()` (avisa a GitExtensions y devuelve el foco a la ventana).
4. Si falla y hay un mensaje → `MessageBox` "Pull falló".
- El botón muestra un **icono de flecha hacia abajo** (`ctx-pull.png`, azul) **antes del texto**, sustituyendo al antiguo carácter `↓`. Etiqueta `Bajar (M)` cuando la branch actual está M commits por detrás.
- La etiqueta superior `Branch: <nombre>` recibe el sufijo `↓M` cuando hay commits por bajar (`UpdateBranchLabel`).

### Botón Push (`_btnPush`) → `DoPush` → `PushCurrent` — actúa sobre el **HEAD**
1. **Guardia de divergencia** (`EnsureNotBehindBeforePush`): si la branch actual está **por detrás** (`behind > 0`), muestra el aviso "Su branch está N commit(s) por detrás del remoto — es necesario integrar primero. ¿Bajar (rebase) y luego enviar?" — **Sí** ejecuta `DoPullRebaseThenPush` (el servicio `PullRebase` → `git pull --rebase <remoto> <branch>` en segundo plano; éxito → `PushCurrent`; fallo/conflicto → `RefreshTree` + error `pullRebaseFailedTitle`, se omite el push), **No** cancela. El método siempre devuelve `false` cuando está por detrás: el push, si lo hay, lo dispara la continuación del rebase, no el llamador.
2. **Preferente:** abre el **diálogo nativo de Push de GitExtensions in-process** (`StartPushDialog`, `pushOnShow: true` — dispara el push automáticamente al abrir).
   - Al cerrar: `RefreshTree()` + `NotifyRepoChanged()` — **siempre**, sin importar el valor de retorno (`pushCompleted` no es fiable con `pushOnShow`).
3. **Fallback** (sin `_openPushDialog`): lanza `GitExtensions.exe push` como un proceso nuevo (fire-and-forget — sin refresh posible). Error al iniciar → `MessageBox`.
- El botón muestra un **icono de flecha hacia arriba** (`ctx-push.png`, verde) **antes del texto**, sustituyendo al antiguo carácter `↑`. Etiqueta `Enviar (N)` cuando la branch actual está N commits por delante del remoto.

### Botón Commit (`_btnCommitDedicated`) → `DoCommit`
1. **Preferente:** abre la **ventana de commit nativa de GitExtensions in-process** (`_openCommitDialog` → `IGitUICommands.StartCommitDialog`). Esto mantiene visibles los plugins de Commit Template (p. ej., "Zimerfeld: Auto-resumen").
   - Retorno `true` (hubo commit) → `RefreshTree()` + `NotifyRepoChanged()`.
   - Retorno `false` (se cerró sin hacer commit) → nada.
   - Retorno `null` (no disponible) → cae en el fallback.
2. **Fallback:** `OpenCommitWindow()` lanza un **proceso nuevo** `GitExtensions.exe commit` (los plugins no se cargan en este modo). Error → `MessageBox`.
- La etiqueta muestra `Commit (n)` con el número de cambios pendientes (`git status --porcelain`) vía `UpdateCommitActionTexts`. El recuento se recalcula: en la construcción, **después de `LoadRepositories`** (ya con el repo seleccionado), al abrir el menú contextual, y en cada refresh — en este último **reutilizando** el valor leído en segundo plano por `RefreshTreeAsync` (sin un `git status` extra en el hilo de UI).
- **Contador en vivo (`FileSystemWatcher`):** un watcher sobre `_svc.WorkingDir` (`IncludeSubdirectories = true`) actualiza `Commit (n)` **silenciosamente** conforme se crean/editan/eliminan archivos — sin reconstruir el árbol ni mostrar overlay. Flujo: `EnsureWorkingDirWatcher()` (llamado tras `ApplyRepoData`, no-op si el repo no cambió) → `RestartWorkingDirWatcher()` crea el watcher → los eventos (en un hilo del pool) pasan por `OnWorkingDirChanged`, que **ignora `.git`** vía `IsUnderGitDir` (`.gitignore`/`.gitattributes` sí pasan) y hace `BeginInvoke` al hilo de UI → `RestartCommitCountDebounce()` (un debounce de 600 ms agrupa la ráfaga de un mismo guardado) → `SilentRefreshCommitCountAsync()` ejecuta solo `GetPendingChangesCount()` en segundo plano y llama a `UpdateCommitActionTexts(pending)`. Ignorar `.git` evita el **eco** del propio `git status` (que reescribe la caché de stat del índice). `OnWorkingDirWatcherError` recrea el watcher ante un desbordamiento de buffer. Limpieza: `StopWorkingDirWatcher()` + `Dispose` del temporizador en `FormClosed`.

### Botón GitFlow (`_btnGitFlowDedicated`) → `DoGitFlow`

![[ScreenshotGitFlow.png]]

1. Crea `GitFlowForm` (modal) y la posiciona **lado a lado** con ZimerfeldTree, ambas centradas (si la pantalla lo permite; si no, se centra sobre la ventana).
2. Se suscribe a `RepoMutated`: en cada mutación dentro de GitFlow, programa revelar la branch afectada y llama a `RefreshTree()` **detrás del modal** (sin robar el foco).
3. `ShowDialog` (bloquea).
4. Al cerrar: recentra la ZimerfeldTree; **solo** llama a `RefreshTree()` si hubo un **release finish** (para enfocar la nueva tag) — en caso contrario **no** refresca, ya que `RepoMutated` ya actualizó en vivo. **No** llama a `NotifyRepoChanged()` (no trae al frente el GitExtensions minimizado).
- Mismo flujo que el elemento de menú "GitFlow…". Detalles de la ventana en [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]].

### Botón Restore (`_btnRestore`) → `DoRestore`

> Renombrado de **Voltar Versão** (`_btnVoltar`) a **Restore** (`_btnRestore`). Los campos e imágenes de cada una de las 10 pestañas están en [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]].

1. Crea `RestoreForm` (modal) y la posiciona **lado a lado** con BranchHierarchy, ambas centradas — el mismo posicionamiento que la ventana GitFlow.
2. Se suscribe a `RepoMutated`: tras cada operación exitosa, llama a `RefreshTree()` **detrás del modal** (sin robar el foco).
3. `ShowDialog` (bloquea).
4. Al cerrar: **no** refresca (el `RepoMutated` ya actualizó en vivo) y **no** llama a `NotifyRepoChanged()`. `RestoreForm` guarda los campos vía `FormClosing → SaveSettings`.

Tres operaciones disponibles en la ventana Restore:
- **Restaurar Archivo** — `git checkout <hash> -- "<archivo>"`: recupera un archivo específico del estado de un commit y lo pone en staging
- **Cherry-Pick** — `git cherry-pick <hash>` o el rango `<antiguo>..<reciente>`: aplica uno o más commits sobre la branch actual
- **Reset Branch** — `git checkout <branch>` (si no es la actual) + `git reset --mixed|--soft|--hard <hash>` + regreso a la branch original

Los valores de los campos se persisten en `%APPDATA%\GitExtensions\ZimerfeldRestore.settings.json`. Ver [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz Restore — botones y flujos]].

### Botón Excluir (`_btnExcluir`) → `DoDelete`
1. Texto dinámico vía `UpdateDeleteButtonText()`: `Eliminar` (0 marcados) → `Eliminar (N)`. Actualizado en `AfterCheck` y en cada reconstrucción.
2. `DoDelete`: los objetivos = los checkboxes marcados si los hay (`CheckedBranchNodes()`); si no, el nodo seleccionado.
   - 2+ → `DoDeleteMultiple` (una única confirmación listando los elementos, eliminación por lotes, forzar en una local no fusionada).
   - 1 → el flujo individual. 0 → nada.
3. **Protección:** main/master/develop se eliminan de los objetivos si el `Modo Developer` está apagado (`IsProtectedBranch`); si no queda nada, muestra el aviso "Branch protegida".

**Flujo de eliminación por lotes (paso a paso):**

1. Elementos marcados — el botón muestra `Eliminar (8)`:

![[ScreenshotBeforeDelete.png]]

2. Una única confirmación listando los elementos, con la opción **¿Eliminar también en remoto?**:

![[ScreenshotConfirmDelete.png]]

3. Overlay de progreso durante la eliminación (lista de pasos + botón **Abortar Operación**):

![[ScreenshotDuringDelete.png]]

4. Árbol reconstruido ya sin los elementos y con los contadores actualizados:

![[ScreenshotAfterDelete.png]]

### Checkbox "Modo Developer" (`_chkDeveloperMode`)
1. Junto a `Show Debug` en el pie. Estado persistido en `ZimerfeldTree.uisettings.json` (`developerMode`) vía `SaveUiSettings()`, cargado por `LoadDeveloperMode()`.
2. `Tree_BeforeCheck` bloquea **marcar** (no desmarcar) main/master/develop cuando está apagado.
3. Al **apagarlo**, `UncheckProtectedBranches()` desmarca las protegidas que estaban marcadas.

### Botón Cerrar (`_btnClose`)
1. `Close()`. Al cerrar, el estado de expansión del árbol se guarda en disco (`FormClosed → SaveTreeState`).

### Botón Cancelar (overlay, `_btnCancelRefresh`)
1. Se deshabilita, cambia a "Cancelando…" y cancela el `CancellationTokenSource` del refresh en curso.

---

## 🌳 Interacciones en el árbol
- **Doble clic** en una hoja de branch → `DoCheckout`.
- **Enter** con una branch seleccionada → `DoCheckout`.
- **Clic derecho** → selecciona el nodo bajo el cursor y abre el menú contextual.
- **Checkbox** en cada branch/tag (hoja) para selección múltiple. Las secciones/carpetas tienen el checkbox **oculto** (`ApplyCheckBoxVisibility` vía `TVM_SETITEM`, tras `EndUpdate`) y **bloqueado** (`Tree_BeforeCheck`). `_tree.CheckBoxes = true`.
- **Persistencia de expandir/colapsar:** `AfterExpand`/`AfterCollapse` escriben la ruta estable (`GetNodeStablePath`) en `_treeStateByRepo` → un debounce de 500 ms + `FormClosed` → `treestate.json`. Se restaura en `RestoreTreeState`, se reaplica en el **`Shown`** (el handle nativo ya existe; en el constructor, sin handle, no se aplica bien).

## 📋 Menú contextual (la visibilidad depende del tipo de nodo)
Definido en `CtxMenu_Opening`: `branch` = local|remote; específicos de `local`/`remote`/`tag`. Los separadores huérfanos se ocultan.

- **Cabecera estilo overlay** (`_miHeader`, un `ToolStripLabel` en negrita + separador `_miHeaderSep` en la parte superior): muestra la branch en checkout (`Branch: <nombre>`) — el `ContextMenuStrip` ya es una ventana flotante sin bordes; la cabecera queda arriba y los comandos debajo. Visible tanto en la selección simple como en la múltiple. (Elección deliberada en lugar de un `Form` sin bordes independiente, que sería frágil — ver la memoria "Pragmatic over literal".)
- **Bajar/Enviar actúan sobre la branch clicada** (no sobre el HEAD): la branch se pone en checkout primero y los contadores reflejan el atrás/adelante **de esa branch**. Los botones de la barra siguen actuando sobre el HEAD.

| Elemento | Visible para | Acción (paso a paso) |
|------|--------------|----------------------|
| **Bajar (N)** | branch local | `DoPullForSelected`: hace **checkout de la branch clicada** (`EnsureCurrentBranch`) y luego `DoPull`. Icono `ctx-pull.png`. `N` = commits por detrás de **esa** branch. |
| **Enviar (N)** | branch local | `DoPushForSelected`: checkout de la branch clicada + guardia de divergencia (`EnsureNotBehindBeforePush`: si está por detrás, ofrece Bajar-con-rebase-y-luego-enviar vía `DoPullRebaseThenPush`) + `PushCurrent`. Icono `ctx-push.png`. `N` = commits por delante de **esa** branch. |
| **Commit (n)** | siempre | Igual que el botón Commit → `DoCommit`. Muestra el número de pendientes. |
| **Checkout** | branch (local/remote) | `DoCheckout`: local → `git checkout "<nombre>"`; remote → `CheckoutRemoteAsLocal` = `git checkout -b "<local>" --track "<origin/...>"`. Éxito → `RefreshTree` + `NotifyRepoChanged`; error → `MessageBox`. |
| **Nueva branch desde aquí…** | local o tag | `DoNewBranch`: pide un nombre (`InputDialog`) → `git checkout -b "<nueva>" "<ref>"`. Éxito → refresh + notify. |
| **Fusionar en la branch actual** | local | `DoMerge`: confirma → `git merge "<nombre>"`. Éxito → refresh; error → `MessageBox`. |
| **Rebase sobre la branch actual** | local | `DoRebase`: confirma → `git rebase "<nombre>"`. Éxito → refresh; error → `MessageBox`. |
| **Renombrar…** | local | `DoRename`: pide un nuevo nombre → `git branch -m "<antiguo>" "<nuevo>"`. |
| **Eliminar…** | local/remote/tag | `DoDelete`: confirma; tag → `git tag -d` **+** `git push <remote> --delete <tag>` (elimina local **y** remota; "remote ref does not exist" se trata como éxito); remote → `git push <remote> --delete <branch>`; local → `git branch -d`. Si "not fully merged" → ofrece **forzar** (`git branch -D`). |
| **GitFlow…** | branch | Igual que el botón GitFlow → `DoGitFlow`. |
| **Restore…** | branch actual ≠ `develop` | Igual que el botón Restore → `DoRestore` (abre la ventana Restore). No depende del nodo clicado — siempre actúa sobre la branch en checkout. |
| **Expandir todo** | siempre | `node.ExpandAll()`. |
| **Colapsar todo** | siempre | `CollapseRecursive(node)`. |
| **Actualizar** | siempre | `RefreshTree()`. |

## 🎨 Iconos por tipo de nodo (`NodeIcons`)
- LOCAL (monitor) · REMOTES (nube) · TAGS (cinta) · carpeta de ruta (ámbar).
- `master`/`main` escudo dorado · `develop` (PNG incrustado) · `feature/*` carpeta=rama, hoja=hoja verde · `bugfix/*` mariquita · `release/*` paquete · `hotfix/*` aviso · `support/*` engranaje.
- Varios usan un **PNG incrustado** en `Resources\` con fallback GDI+.

## 🔗 Relacionado
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
- [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]]
