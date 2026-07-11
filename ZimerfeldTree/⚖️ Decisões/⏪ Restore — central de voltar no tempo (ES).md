---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, restore, recuperacao, ux]
status: aceita
criado: 2026-06-07
---

# ADR — Restore como centro único para "volver en el tiempo"

> 🇧🇷 Lee esta página en portugués → [[⏪ Restore — central de voltar no tempo]]
> 🇺🇸 Read this page in English → [[⏪ Restore — central de voltar no tempo (EN)]]

## Contexto
Las operaciones de deshacer/recuperar de git (restore, revert, reset, cherry-pick, reflog, rebase, discard) son poderosas pero **intimidantes y dispersas**, con riesgos muy distintos entre sí — desde algo trivial (restaurar un archivo) hasta algo destructivo (reset --hard, rebase). El usuario rara vez recuerda el comando correcto ni su grado de peligrosidad.

## Decisión
Concentrar todo en una única ventana **`RestoreForm`** con **10 pestañas ordenadas de la más segura a la más destructiva**, cada categoría con una **explicación integrada** y orientaciones de trabajo en equipo, además de una pestaña **"Acerca de Restore"** desplazable. Orden: Plan de Emergencia → Restaurar Archivo/Árbol/Tag → Cherry-Pick → Revertir → Reset → Nueva Branch/Tag → Reflog → Descartar Locales → Rebase.

## Consecuencias
**Positivas:** un único lugar para recuperarse de errores; el orden por riesgo educa y reduce accidentes; el **Reflog** ofrece una red de seguridad incluso después de operaciones destructivas.

**Trade-offs:** ventana grande (~980 px, 10 pestañas) — mitigado por la ordenación por riesgo y por las explicaciones de cada categoría.

## 🔗 Relacionado
- [[⏪ RestoreForm (ES)|RestoreForm]]
- [[🛡️ Modo Developer protege main-develop (ES)|Modo Developer protege main/develop]]
- [[⏪ Interface Restore — botões e fluxos (ES)|Interfaz de Restore — botones y flujos]]
- [[⏪ Restore (voltar no tempo) (ES)|Restore (volver en el tiempo)]]
