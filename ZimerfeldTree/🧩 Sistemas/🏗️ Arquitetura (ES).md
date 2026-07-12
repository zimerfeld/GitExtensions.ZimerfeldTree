---
tipo: sistema
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [arquitetura, classes, design, i18n, threading, gitextensions]
versao: 1.0.358
---

# Arquitectura

> 🇧🇷 Lee esta página en portugués → [[🏗️ Arquitetura]]
> 🇺🇸 Read this page in English → [[🏗️ Arquitetura (EN)]]

## Diagrama de clases

```
GitExtensions (host)
    │
    │  MEF (System.ComponentModel.Composition)
    ▼
ZimerfeldTreePlugin        ← [Export(IGitPlugin)] : GitPluginBase
    │  Execute()  → abre/trae al frente la ventana singleton
    │  captura _commands (Register/Unregister)
    ▼
BranchHierarchyForm (ventana principal)   ← Sizable, no modal, singleton
    │  cboRepo (directorio de trabajo independiente)
    │  RefreshTreeAsync(...) → Task.Run (overlay de progreso)
    │  botones: Pull / Push / Commit / Eliminar / GitFlow / Restore
    │        │                       │
    │        ▼                       ▼
    │  GitFlowForm (modal)      RestoreForm (modal)
    │  start/publish/track/     10 pestañas: restore/revert/
    │  update/finish            reset/reflog/rebase…
    │        │                       │
    ▼        ▼                       ▼
BranchHierarchyService     ← ejecutor de git + parser de salida + ensamblado de la jerarquía
    │  RunGit(args) → (StdOut, StdErr, ExitCode)
    ▼
git (PATH)   ·   modelos: BranchInfo / BranchType (BranchNode.cs)
                 iconos:  NodeIcons (árbol) · PluginIcon (ventana)
```

## Las clases

### `ZimerfeldTreePlugin` — punto de entrada
Hereda de `GitPluginBase`, exportado vía MEF como `IGitPlugin`. **`Execute`** abre (o trae al frente) la **ventana singleton** `BranchHierarchyForm`; **`Register`/`Unregister`** capturan/limpian `_commands` (`IGitUICommands`) usados para abrir los diálogos nativos de Commit/Push/Pull del host. Ver [[🌳 ZimerfeldTreePlugin (ES)|ZimerfeldTreePlugin]].

### `BranchHierarchyForm` — la ventana principal
WinForms `Sizable`, no modal, `CenterScreen`, singleton por sesión. Árbol en 3 secciones fijas (**LOCAL / REMOTES / TAGS**), filtro en tiempo real, botones encima del árbol, overlay de progreso en la 1ª carga, contador de Commit en vivo (FileSystemWatcher). El constructor **no** hace git — todo se lee en segundo plano disparado por el `Shown`. Ver [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]] y [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz de ZimerfeldTree — botones y flujos]].

### `GitFlowForm` — la ventana GitFlow
Modal. Dirige start/publish/track/update/finish para feature, bugfix, release, hotfix y support usando **solo git nativo** (no depende del binario `git-flow`). Permite una **jerarquía flexible** (feature hija de feature vía *based on:*). Ver [[🔀 GitFlowForm (ES)|GitFlowForm]] y [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz de GitFlow — botones y flujos]].

### `RestoreForm` — la ventana Restore
Modal, ~980 px, 10 pestañas de la más segura a la más destructiva (Plan de Emergencia → Restaurar Archivo/Árbol/Tag → Cherry-Pick → Revertir → Reset → Nueva Branch/Tag → Reflog → Descartar Locales → Rebase). Ver [[⏪ RestoreForm (ES)|RestoreForm]] y [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]].

### `BranchHierarchyService` — ejecutor de git + jerarquía
Ejecuta `git` en subprocesos (stdout/stderr redirigidos) y **parsea** la salida. Construye el grafo de commits con **un único `git log --all`** y resuelve padres por BFS → **O(commits)** en lugar de O(N²×subproceso). Contiene la lógica de los comandos GitFlow (git puro). Ver [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]].

### Modelos e iconos
- `BranchNode.cs` — `BranchInfo` (datos de la branch) + enum `BranchType` (Local/Remote/Tag). Ver [[🌿 BranchNode (ES)|BranchNode]].
- `NodeIcons.cs` — iconos 16×16 GDI+ + PNGs incrustados (ImageList del árbol). Ver [[🎨 NodeIcons (ES)|NodeIcons]].
- `PluginIcon.cs` — icono "Árbol de la Vida" del plugin/ventana (`Resources/ico.png`), cargado 1 vez y cacheado. Ver [[🖼️ PluginIcon (ES)|PluginIcon]].

## Desacoplamiento del host

> [!important] La ventana elige el repositorio mediante su propio `cboRepo`
> El directorio de trabajo viene del combo `cboRepo` (poblado desde el XML de settings de GitExtensions), independiente del repositorio activo en el host. `Register` guarda `_commands` solo para poder abrir los diálogos nativos (Commit/Push/Pull) **en el directorio de trabajo seleccionado**. Ver [[🪟 Janela não-modal singleton (ES)|Ventana no modal singleton]].

## Localización (i18n)

Inglés / Portugués, elegido **por ventana** y recordado. La ventana principal usa `I18n.SetLanguage` global (persistido en `ZimerfeldTree.language.json`); GitFlow y Restore tienen su propio selector persistido en sus archivos de settings. Traduce **controles/etiquetas**, nunca los datos.

## Threading

> La ventana **se abre instantáneamente**: el constructor no hace **ningún** trabajo de git. La 1ª carga se ejecuta detrás del evento `Shown` (`FirstLoadAsync` → `RefreshTreeAsync(showOverlay:true)`).

- **`RefreshTreeAsync`** — recolecta branches/tags/jerarquía en un `Task.Run` en un hilo de fondo y lo aplica a la UI; overlay de progreso (0→100%, 8 pasos) en la 1ª apertura y en las recargas explícitas.
- **Contador de Commit en vivo** — `FileSystemWatcher` con debounce de 600 ms → un único `git status` en segundo plano, sin reconstruir el árbol. Ignora cambios en `.git`.
- **Verificación del remoto al abrir** — un `git fetch` del upstream se ejecuta en segundo plano después del `Shown` (offline-safe), corrigiendo los contadores Pull/Push.

## Relacionado

- [[🌳 ZimerfeldTreePlugin (ES)|ZimerfeldTreePlugin]]
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]
- [[🪟 Janela não-modal singleton (ES)|Ventana no modal singleton]]
- [[🔀 GitFlow em git puro (ES)|GitFlow en git puro]]
- [[📦 Dependências (ES)|Dependencias]]
- [[👁️ Visão Geral (ES)|Visión General]]
