---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, ui, seguranca, branches]
status: aceita
criado: 2026-06-16
---

# ADR — "Modo Developer" para proteger main / master / develop

> 🇧🇷 Lee esta página en portugués → [[🛡️ Modo Developer protege main-develop]]
> 🇺🇸 Read this page in English → [[🛡️ Modo Developer protege main-develop (EN)]]

## Contexto
La ventana permite selección múltiple mediante checkbox y **eliminación en lote** de branches. Las branches de larga vida (`main`/`master`/`develop`) son las más peligrosas de borrar por accidente.

## Decisión
Añadir un checkbox **"Modo Developer"** (junto a "Show Debug"):
- **Desactivado (por defecto):** `main`/`master`/`develop` quedan **protegidas** — checkbox bloqueado, no se pueden marcar ni eliminar.
- **Activado:** libera la marcación/eliminación de esas branches específicas.

Desactivar el modo **desmarca automáticamente** cualquier main/master/develop que estuviera marcada. Estado persistido en `ZimerfeldTree.uisettings.json`.

## Consecuencias
**Positivas:** protege por defecto el estado que refleja producción; la liberación es explícita y consciente.

**Trade-offs:** un paso adicional para quien realmente necesita tocar esas branches (aceptable — es precisamente la intención).

## 🔗 Relacionado
- [[🪟 BranchHierarchyForm (ES)|BranchHierarchyForm]]
- [[⏪ Restore — central de voltar no tempo (ES)|Restore — centro para volver en el tiempo]]
- [[🌳 Interface ZimerfeldTree — botões e fluxos (ES)|Interfaz de ZimerfeldTree — botones y flujos]]
