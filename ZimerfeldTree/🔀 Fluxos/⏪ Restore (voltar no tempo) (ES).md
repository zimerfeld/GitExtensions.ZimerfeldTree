---
tipo: fluxo
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [fluxo, restore, recuperacao, desfazer, etapa3]
---

# Flujo: 3 — Restore (volver en el tiempo)

> 🇧🇷 Lee esta página en portugués → [[⏪ Restore (voltar no tempo) (PT)|⏪ Restore (voltar no tempo)]]
> 🇺🇸 Read this page in English → [[⏪ Restore (voltar no tempo) (EN)]]

Ventana `RestoreForm`, abierta con el botón **Restore**. 10 pestañas ordenadas de la más segura a la más destructiva.

![[ScreenshotRestore.png]]

## Pasos

```
[Restore]  →  elegir la pestaña según el objetivo (segura → destructiva):

  1. Plan de Emergencia    branch ← tag (red de seguridad)
  2. Restaurar Archivo     git restore (Examinar… restringido a la raíz del repo)
  3. Restaurar Árbol       restaura el árbol en un commit
  4. Restaurar Tag         checkout/restore a partir de un tag
  5. Cherry-Pick           git cherry-pick
  6. Revertir              git revert (commit / merge -m 1)
  7. Reset Branch          git reset
  8. Nueva Branch/Tag      crea una ref (+ Inspeccionar detached)
  9. Recuperar (Reflog)    git reflog → recuperar refs perdidas
 10. Descartar Locales     checkout / reset --hard HEAD / clean
     Rebase                elimina un commit
```

## Detalles

- Cada categoría trae una **explicación incrustada** + orientaciones de trabajo en equipo; pestaña **"Acerca de Restore"** desplazable.
- El **Reflog** es la red de seguridad incluso después de operaciones destructivas.
- Al cerrar, la ventana principal recupera el foco y actualiza el árbol.

## Relacionado

- [[🌲 Abrir e navegar a árvore (ES)|Abrir y navegar el árbol]]
- [[🔀 GitFlow (Start a Finish) (ES)|GitFlow (Start a Finish)]]
- [[⏪ RestoreForm (ES)|RestoreForm]]
- [[⏪ Restore — central de voltar no tempo (ES)|Restore — centro para volver en el tiempo]]
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]]
