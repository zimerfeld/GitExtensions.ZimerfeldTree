---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-06
historico: 2026-06-17 (ventana ampliada a 980 px con 10 pestañas cubriendo TODAS las formas de viajar en el tiempo: + Restaurar Árbol, Revertir, Nueva Branch/Tag (+Inspeccionar), Recuperar (Reflog), Descartar Locales y Rebase; botón Examinar restringido a la raíz del repo; About pasó a ser una ventana desplazable con explicación por categoría + trabajo en equipo)
tags: [conhecimento, gitextensions, plugin, winforms, ui, fluxos, restore]
fonte: src\GitExtensions.ZimerfeldTree\RestoreForm.cs
---

# Interfaz Restore — botones y flujos

> 🇧🇷 Lee esta página en portugués → [[⏪ Interface Restore — botões e fluxos]]
> 🇺🇸 Read this page in English → [[⏪ Interface Restore — botões e fluxos (EN)]]

> [!abstract] Resumen
> Ventana **modal** (`RestoreForm`) — el centro de "viaje en el tiempo" del código. Reúne **todas** las formas de recuperar, deshacer o descartar un estado del repositorio, cada una en su propia **pestaña**, organizadas de la más segura a la más destructiva. Accesible mediante el botón **Restore** de la [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]. Proyecto: [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].

## 🧭 Diseño
- Ventana de **980 px** de ancho, `TabControl` con **Multiline = true** (pestañas en varias líneas — todas visibles a la vez). Cabecera `HEAD: <ref>` + enlace **"Acerca de Restore"**. Caja de **Resultado** (Consolas, fondo beige `#EFEBD8`) llena el espacio debajo de las pestañas. Pie: **Cerrar** (centro, = `CancelButton`/Esc), **Show Debug** (izq.), **Idioma** (der.).
- **Diseño responsivo** (`LayoutResponsive`) — combos/campos estirados y botones realineados a la derecha en tiempo de ejecución, **margen derecho = margen izquierdo** (`SideMargin = 14`); recalculado en `Load` y en `_tabs.ClientSizeChanged`.

## 🗂️ Pestañas (orden segura → destructiva) — campos de cada pestaña

🟢 **Seguras (no reescriben el historial)**

- **Plan de Emergencia** — **Branch:** (combo, preselecciona la branch actual) + **Tag:** (combo). Botones **Restaurar a la Tag** `checkout <tag> -- .` (en staging) / **Resetear a la Tag** (rojo) `reset --hard <tag>` (confirma).
  ![[ScreenshotRestoreEmergencyPlan.png]]
- **Restaurar Archivo** — **Commit hash:** (combo) + **Archivo (ruta relativa):** (cuadro de texto) + **Examinar…** (`_btnBrowseFile`, `OpenFileDialog` en `_svc.WorkingDir`, valida que esté dentro de la raíz, guarda la ruta relativa con `/`). Botón **Restaurar Archivo** `checkout <hash> -- "<archivo>"`.
  ![[ScreenshotRestoreFile.png]]
- **Restaurar Árbol** — **Commit hash:** (`_cboTreeHash`). Botón **Restaurar Árbol** `checkout <hash> -- .` (todo el árbol rastreado, en staging).
  ![[ScreenshotRestoreTree.png]]
- **Cherry-Pick** — **Commit hash:** (combo, acepta un rango `antiguo..reciente`). Botón **Aplicar Cherry-Pick** `cherry-pick <hash>`.
  ![[ScreenshotRestoreCherry-Pick.png]]
- **Revertir** — **Commit hash:** (`_cboRevertHash`). Botones **Revertir Commit** `revert --no-edit <hash>` / **Revertir Merge (-m 1)** `revert -m 1 --no-edit <hash>` (deshacer **seguro**, nuevo commit, para una branch compartida).
  ![[ScreenshotRestoreRevert.png]]
- **Nueva Branch/Tag** — **Commit hash:** (`_cboNewRefHash`) + **Nombre:** (`_txtNewRefName`). Botones **Inspeccionar** `checkout <hash>` (🔵 detached HEAD, solo lectura, confirma) / **Crear Tag** `tag <nombre> <hash>` / **Crear Branch** `branch <nombre> <hash>`.
  ![[ScreenshotRestoreNewBranchTag.png]]

🟡 **Recuperación**
- **Recuperar (Reflog)** — **Entrada:** (`_cboReflog`, poblado por `git log -g -150`, selector `%gd`=`HEAD@{n}`, subject `%gs`) + **Nombre:** (`_txtReflogBranch`). Botones **Crear Branch Aquí** `branch <nombre> <sha>` / **Resetear Actual Aquí** (rojo) `reset --hard <sha>` (confirma).
  ![[ScreenshotRestoreRecoverReflog.png]]

🟠 **Descartar locales**
- **Descartar Locales** — botones **Descartar sin staging (tracked)** `checkout -- .` / **Reset --hard HEAD** (rojo) / **Eliminar no rastreados (clean -fd)** (rojo); todos confirman.
  ![[ScreenshotRestoreDiscarLocal.png]]

🔴 **Reescriben el historial**
- **Reset Branch** — **Branch:** (`_cboBranch`, preselecciona la actual) + **Commit hash:** (`_cboResetHash`) + **Modo** (radio `--mixed`/`--soft`/`--hard`). Botón **Resetear Branch** (rojo) `reset --<modo> <hash>`; si la branch ≠ la actual, hace `checkout <branch>` → reset → vuelve. `--hard` confirma.
  ![[ScreenshotRestoreResetBranch.png]]
- **Rebase** — **Commit hash:** (`_cboRebaseHash`). Botones **Eliminar Commit del Historial** (rojo) `rebase --onto <hash>^ <hash>` (elimina el commit, reaplica los posteriores, confirma; en caso de conflicto añade `rebaseConflictHint`) / **Abortar Rebase** `rebase --abort`.
  ![[ScreenshotRestoreRebase.png]]

> **Desplegables de commit** (`HashCombos`: Restaurar Archivo, Árbol, Cherry-Pick, Revertir, Reset, Nueva Branch/Tag, Rebase) poblados por `git log --all --source -200 ... %h␟%S␟%cd␟%s`; cada elemento `(YYYY-MM-dd HH:mm:ss) [branch] hash → mensaje`, el más nuevo primero, prompt **Seleccione...**, **no** persistidos. El combo de Reflog usa su propia fuente (`LoadReflogRefs`).

## 🚀 Al abrir (`Load` → `InitData`)
1. `HEAD:` vía `GetHeadRef()`. 2. Combos de branch (Reset + Emergencia) vía `git branch`. 3. Tags en Emergencia. 4. Todos los `HashCombos` reciben prompt + refs; el de Reflog recibe prompt + `LoadReflogRefs()`. 5. `RestoreSettings` — **ningún combo** se restaura; solo `restoreFile`, `resetMode`, `showDebug`, `language`. 6. `SelectBranchDefault` preselecciona la branch en checkout en ambos combos de branch.

## 🧩 Auxiliares
- `RevealInTree(branch)` — dispara `RepoMutated` (ZimerfeldTree actualiza el árbol en segundo plano y revela la branch) y reactiva el modal; `null` solo refresca.
- `HashOf(combo)` — devuelve `CommitRef.Hash` del elemento seleccionado o el texto escrito (ignora el prompt).
- `RunGit(args, append)` — ejecuta vía `_svc.RunGitFlow`, escribe en Resultado y actualiza `HEAD:`.

## ⚙️ Comportamiento de la ventana
- Posicionada **lado a lado** con BranchHierarchy (el fallback de `DoRestore` gestiona las pantallas más pequeñas que `main + 980 + gap`).
- Después de cada operación exitosa, el árbol se actualiza **en segundo plano** sin robar el foco a Restore.
- **Al cerrar**: el owner **no** dispara un refresh adicional ni trae GitExtensions al frente; `FormClosing` solo persiste los campos que no son combos (`restoreFile`, `resetMode`, `showDebug`, `language`) en `ZimerfeldRestore.settings.json`.
- **Show Debug** e **Idioma** son **por ventana** (mismo archivo de settings), independientes de los demás.
- **Acerca de Restore** (`ShowAbout`) — ahora abre una **ventana propia desplazable** (un `TextBox` de solo lectura, redimensionable) con la explicación completa: cada pestaña por **categoría de seguridad** (🟢🔵🟡🟠🔴) + una sección **👥 Trabajo en equipo** (varios devs en la misma `main` → usar Revertir, `pull --rebase` antes de hacer push; varias branches en `develop` → Cherry-Pick, Revertir Merge -m 1, abortar rebase/merge, crear branch a partir de un commit). El texto viene de la clave `aboutBody` (en/pt).

## 🔗 Relacionado
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz ZimerfeldTree — botones y flujos]]
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
