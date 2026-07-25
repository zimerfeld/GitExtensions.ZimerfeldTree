---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, ui, janela, nao-modal, singleton]
status: aceita
criado: 2026-06-01
---

# ADR — Ventana principal no modal y singleton

> 🇧🇷 Lee esta página en portugués → [[🪟 Janela não-modal singleton (PT)|🪟 Janela não-modal singleton]]
> 🇺🇸 Read this page in English → [[🪟 Janela não-modal singleton (EN)]]

## Contexto
El árbol de branches se consulta y manipula **continuamente** durante el trabajo (checkout, crear branch, GitFlow, restore). Un diálogo modal bloquearía GitExtensions y obligaría a abrir/cerrar en cada operación.

## Decisión
Exponer la ventana principal (`BranchHierarchyForm`) como un **`Form` no modal, singleton por sesión**, `Sizable` y `CenterScreen`, abierta desde el menú Plugins → ZimerfeldTree. Queda disponible junto al host mientras se trabaja; reabrirla trae la instancia existente al frente. El **working directory** proviene del propio `cboRepo` (leído del XML de settings de GitExtensions), **independiente** del repositorio activo en el host.

## Consecuencias
**Positivas:**
- Flujo fluido — la ventana permanece abierta durante el trabajo.
- Desacoplamiento del host → robusta entre versiones de GitExtensions.
- El `cboRepo` independiente permite operar sobre un repositorio distinto al abierto en el host.

**Trade-offs:**
- La ventana **sondea el estado por sí sola** (`RefreshTreeAsync`, `FileSystemWatcher`, `git fetch` al abrir) en lugar de reaccionar a eventos del host.
- Commit/Push reaprovechan los diálogos nativos vía `IGitUICommands.WithWorkingDirectory(dir)`.

## 🔗 Relacionado
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[🌳 ZimerfeldTreePlugin (ES)|ZimerfeldTreePlugin]]
- [[🏗️ Arquitetura (ES)|Arquitectura]]
