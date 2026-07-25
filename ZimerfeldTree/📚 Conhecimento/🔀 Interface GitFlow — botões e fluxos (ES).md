---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
historico: 2026-06-11 (regla "based on" dirigida por el Type en Start — combo filtrado + checkbox habilitado/deshabilitado según el tipo)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, gitflow]
fonte: src\GitExtensions.ZimerfeldTree\GitFlowForm.cs
---

# Interfaz GitFlow — botones y flujos

> 🇧🇷 Lee esta página en portugués → [[🔀 Interface GitFlow — botões e fluxos (PT)|🔀 Interface GitFlow — botões e fluxos]]
> 🇺🇸 Read this page in English → [[🔀 Interface GitFlow — botões e fluxos (EN)]]

> [!abstract] Resumen
> Ventana **modal** (`GitFlowForm`) que dirige las operaciones de git flow usando **git puro** (sin depender del binario `git-flow`): iniciar feature/release/hotfix, publicar, rastrear, actualizar y finalizar. La salida de los comandos aparece en una caja de texto con desplazamiento automático. Se abre desde el botón/menú **GitFlow** de la [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]. Proyecto: [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].

![[ScreenshotGitFlow.png]]

## 🌳 Reglas de Start y Finish por tipo

Diagrama resumen: para cada tipo, la **base del Start**, la **branch creada** y el **destino del merge en el Finish**.

![[ScreenShotStartFinish.png]]

| Tipo | Start — base | Finish — destino |
| --- | --- | --- |
| **feature** | `develop` o `feature/*` (opcional) | `develop` o el padre based-on (`merge --no-ff`) |
| **bugfix** | `release/*` (obligatorio) | la propia **release (padre)** — o `develop` si la release no existe (`merge --no-ff`) |
| **release** | `develop` (fija) | `main` (`merge --no-ff` + tag) + `develop`; push de main/develop/tag |
| **hotfix** | `main` (fija) | `main` (`merge --no-ff` + tag) + `develop` |
| **support** | tag de producción (obligatoria) | solo `main` (`merge --no-ff`, sin tag, sin develop) |

> Común a todo Finish: fetch opcional · elimina la branch local y remota (excepto **Keep**) · reconecta los hijos en el árbol. Detalles completos en [[#Botón Finish (`_btnFinish`) → `DoFinish` ⚠️ flujo compuesto]].

### Flujo completo de comandos por tipo

Secuencia de comandos `git` de cada tipo, del Start al Finish (con remoto, sin No fetch):

![[ScreenShotFlowPerType.png]]

### Posicionamiento del nodo en el árbol (based-on)

Git guarda solo el commit tip de cada branch, no su origen. El Start anida el nuevo nodo mediante uno de estos mecanismos:

![[ScreenShotHierarchyBasedOn.png]]

- **commit vacío** (base develop/main + based-on): `git commit --allow-empty` → el tip diverge → ascendencia real
- **based-on override** (base `feature/*` personalizada + based-on): escribe `.git/zimerfeld-basedon.json` (enlace visual, historial limpio)
- **sin based-on**: `checkout -b` simple, se anida por la regla de GitFlow + prefijo
- Finish → `RebaseBasedOnOnFinish` elimina el enlace y reapunta los hijos al destino. Ver el código en `BranchHierarchyService.cs` (`SaveBasedOnOverride`, `ApplyBasedOnOverrides`, `BreakCycles`).

## 🧭 Diseño
- **Cabecera** — `HEAD: <ref simbólica>` + enlace **"About GitFlow"**.
- **Start branch** (grupo) — `Type` (combo), `Expected name` (etiqueta de prefijo + cuadro de texto) + botón **Start**, checkbox **based on:** + combo de base. El contenido del combo y el estado del checkbox están **dirigidos por el Type** (ver [[#Regla "based on" por tipo (Start)]]).
- **Manage existing branches** (grupo) — `Type` (combo), `Branch` (combo de branches locales con el prefijo), botones **Publish / Track / Update / Finish**, checkboxes **Keep branch after finish** (`-k`, marcado por defecto) y **No fetch (--no-fetch)**.
- **Resultado de los comandos git** — caja multilínea de solo lectura (fuente Consolas); se limpia al iniciar cada acción y hace scroll automático hasta el final conforme se ejecutan los subcomandos.
- **Cerrar**.

Tipos soportados (`GitFlowTypes`): `feature`, `bugfix`, `release`, `hotfix`, `support`. El prefijo de cada tipo viene de `git config gitflow.prefix.<tipo>` (fallback `tipo/`).

## 🚀 Al abrir (`Load` → `InitData` + `ApplySettings`)
1. Rellena `HEAD:` (`git rev-parse --symbolic-full-name HEAD`).
2. Combo "based on": rellenado por `ApplyStartTypeRule()` según el `Type` inicial (`feature` → `develop` + `feature/*`). Ver [[#Regla "based on" por tipo (Start)]].
3. **Detecta el tipo git-flow de la branch actual** y abre el panel Manage ya apuntando a ella.
4. `Type` (Start) comienza en `feature`; `Type` (Manage) en la branch detectada.
5. Carga los checkboxes guardados desde `%APPDATA%\GitExtensions\ZimerfeldTree.gitflowsettings.json`.

## 🔁 `RunFlow(args)` — el ejecutor común
Toda acción pasa por aquí:
1. Cursor de espera; ejecuta `git <args>` (`RunGitFlow` → stdout+stderr combinados + código de salida).
2. Añade `command - git <args>` + la salida a la caja de Resultado vía `AppendText` (scroll automático hasta el final). Cada botón llama a `_txtResult.Clear()` antes del primer `RunFlow`, limpiando el resultado anterior.
3. Actualiza la etiqueta `HEAD:` y **recarga el combo de branches** de Manage (una branch eliminada por finish desaparece de aquí).
4. Si el código de salida ≠ 0 y no es `suppressError` → un `MessageBox` de error (`ShowFlowError`).
5. Devuelve `true` si el código de salida es 0.

`RevealInTree(branch, checkout)`: opcionalmente hace `git checkout "<branch>"`, dispara `RepoMutated` (la ZimerfeldTree de detrás se actualiza y revela/selecciona la branch) y reactiva el modal.

---

## 🖱️ Botones y acciones — paso a paso

### Combo Type — Start
1. `SelectedIndexChanged`: actualiza la etiqueta de prefijo (`git config gitflow.prefix.<tipo>`).
2. Si el tipo es **release** o **hotfix** y el nombre está vacío → lo rellena automáticamente con la convención **`yyyyMMddHHmm`** (p. ej., `202606011230`). No sobrescribe una entrada manual.
3. Llama a `ApplyStartTypeRule()` — ver a continuación.

### Regla "based on" por tipo (Start)
`ApplyStartTypeRule()` (disparado por el `SelectedIndexChanged` de `cboStartType` y reaplicado tras un Start exitoso) repuebla `cboBasedOn` y define el estado de `chkBasedOn` según el tipo:

| `cboStartType` | `cboBasedOn` | `chkBasedOn` |
| --- | --- | --- |
| **hotfix**  | `main` (base fija)                                | **deshabilitado** |
| **release** | `develop` (base fija)                             | **deshabilitado** |
| **feature** | `develop` (1er elemento) + branches `feature/*` locales | **habilitado** |
| **bugfix**  | solo branches `release/*` locales                | **marcado + habilitado (obligatorio)** |
| *otros (support)* | `develop` + todas las branches locales       | **habilitado** |

- El combo solo queda **utilizable** cuando el checkbox está **habilitado Y marcado** (`_cboBasedOn.Enabled = _chkBasedOn.Enabled && _chkBasedOn.Checked`).
- hotfix/release: el checkbox se desmarca y se deshabilita → base fija; el combo solo muestra `main`/`develop`. El fallback de `DoStart` (sin "based on") ya resuelve al mismo `main`/`develop`, manteniendo la coherencia.
- **bugfix (regla del proyecto)**: un bugfix **solo puede existir vinculado a una release**. El checkbox ya viene **marcado** y el combo lista solo las `release/*`; `DoStart` **bloquea** el Start si no hay ninguna release o si la base elegida no es una `release/*`. La base release (no raíz) hace que `DoStart` escriba un **based-on override** → el bugfix queda **anidado bajo la release** en el árbol.
- feature: marque el checkbox para elegir la base en el combo filtrado (feature hija de feature).
- Los nombres de branch en el combo son **completos** (p. ej., `feature/x`, `release/2026`).

### Botón Start (`_btnStart`) → `DoStart`
1. Lee el tipo y el nombre; si el nombre está vacío → `MessageBox` y aborta.
1b. **bugfix**: si no hay ninguna `release/*` → `MessageBox` (`bugfixNeedsRelease`) y aborta; si el checkbox no está marcado o la base no es una `release/*` existente → `MessageBox` (`bugfixSelectRelease`) y aborta.
2. Limpia `_txtResult`.
3. `git checkout -b <prefijo><nombre> <base>` (base por defecto: develop para feature/release; main para hotfix/support; **release para bugfix (obligatoria)**; o la branch elegida en "based on").
4. Limpia el cuadro de nombre.
5. Éxito: preselecciona la nueva branch en el panel **Manage** y la revela en ZimerfeldTree (`RevealInTree(prefijo+nombre, checkout:false)` — el checkout ya se hizo con el `-b`).
6. Fallo → reactiva el modal.

### Botón Publish (`_btnPublish`) → `DoPublish`
1. Lee tipo+nombre (aborta si está vacío); aborta si no hay remoto configurado.
2. Limpia `_txtResult`.
3. `git push --set-upstream <remote> <prefijo+nombre>`.
4. Éxito → `RevealInTree(prefijo+nombre, checkout:false)`.

### Botón Track (`_btnTrack`) → `DoTrack`
1. Lee tipo+nombre; aborta si no hay remoto.
2. Limpia `_txtResult`.
3. Si No fetch está desmarcado: `git fetch <remote>`.
4. `git checkout -b <prefijo+nombre> --track <remote>/<prefijo+nombre>`.
5. Éxito → reveal.

### Botón Update (`_btnUpdate`) → `DoUpdate`
1. Lee tipo+nombre.
2. Limpia `_txtResult`.
3. Si No fetch está desmarcado y existe un remoto: `git fetch <remote>`.
4. `git checkout <prefijo+nombre>`.
5. `git merge <remote>/<padre>` (o `<padre>` local si No fetch). Padre = develop para feature/release; main para hotfix/support; **la release (padre)** para bugfix — siempre fusionada desde el ref local de la release (las releases suelen ser locales), con fallback a develop si no se resuelve ninguna release.
6. Éxito → reveal.

### Botón Finish (`_btnFinish`) → `DoFinish` ⚠️ flujo compuesto
1. Lee tipo+nombre (aborta si está vacío).
2. Limpia `_txtResult`.
3. Si No fetch está desmarcado y existe un remoto: `git fetch <remote>`.
4. **Secuencia de merge** (git puro, sin binario git-flow):
   - feature: `checkout <develop o padre based-on>` → `merge --no-ff`.
   - bugfix: `checkout <release (padre based-on), o develop si la release no existe>` → `merge --no-ff` (el padre based-on es la release elegida en Start; solo no se usa si fue finalizada/eliminada).
   - hotfix/release: `checkout main` → `merge --no-ff` → `tag -a <nombre> -m <nombre>` → `checkout develop` → `merge --no-ff`.
   - support: `checkout main` → `merge --no-ff`.
5. Si **Keep** está desmarcado: `git branch -d <prefijo+nombre>`.
6. **Eliminación remota** (todos los tipos): `git ls-remote --heads <remote> <branch>` → si existe, `git push <remote> --delete <branch>`; si no, añade la nota "(omitido: ya no existe)".
7. Post-finish para **release** (adicional):
   a. `LastFinishedReleaseTag = nombre` (ZimerfeldTree enfoca la tag al cerrar).
   b. Sin remoto → aviso "finalizada localmente" y se detiene.
   c. `git push <remote> <main>` → `git push <remote> <develop>`.
   d. `git push <remote> refs/tags/<nombre>`.
   e. Eliminación remota de la branch release (el paso 6 ya se ejecutó antes del paso 7).
   f. `git checkout <develop>` + reveal.
8. No release (feature/bugfix/hotfix/support): `RevealInTree(branch actual, checkout:false)`.

> Los errores de merge detienen el flujo y muestran el resultado en el panel. Resolver manualmente (`git merge --abort` o commit).

### Checkboxes del Finish
- **Keep branch after finish** y **No fetch (--no-fetch)**: al cambiar, se guardan en `ZimerfeldTree.gitflowsettings.json`.
- **Show Debug** (`chkShowDebug`): persiste/recarga su propio estado **de forma individual** (clave `showDebug` en el mismo `ZimerfeldTree.gitflowsettings.json`). En la primera apertura (sin valor guardado) usa el estado heredado del owner (`showControlIds`).
- **Idioma** (`cboLanguage`): **por ventana** (campo `_lang`), persistido individualmente (clave `language` en `ZimerfeldTree.gitflowsettings.json`), aplicado en `Load` vía `ApplyLanguage()` con `I18n.Load(scope, _lang)`. **Ya no** llama al `I18n.SetLanguage` global. En `ApplySettings`, `_lang` se define **antes** de los checkboxes (cuyo `CheckedChanged` llama a `SaveSettings`) para no sobrescribir el idioma guardado. La primera apertura sin valor hereda `I18n.Current`.

### Enlace "About GitFlow"
- Abre un `MessageBox` describiendo los comandos git ejecutados por cada botón.

### Botón Cerrar (`_btnClose`)
- `Close()` (también es el `CancelButton`). No hay `FormClosing` (los checkboxes ya se guardan de forma incremental en cada `CheckedChanged`).
- En el owner, tras cerrarse el modal: recentra la ZimerfeldTree y **solo** llama a `RefreshTree()` si hubo un **release finish** (para enfocar la tag). En caso contrario no refresca (el `RepoMutated` ya actualizó en vivo) y **no** llama a `NotifyRepoChanged()`.

## ⚠️ Errores comunes (`ShowFlowError`)
Cuando la salida contiene "does not exist" / "not found" / "unknown revision" / "pathspec", el mensaje orienta a comprobar `git branch --list main master develop` y `git config gitflow.branch.*`, crear la branch faltante o usar **GitFlow Initialize**.

## 🔗 Relacionado
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[⚙️ git flow - chaves de config (CLI) (ES)|git flow — claves de config (CLI)]]
- [[🧩 Plugin MEF para GitExtensions (ES)|Plugin MEF para GitExtensions]]
