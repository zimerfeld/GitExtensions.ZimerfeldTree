---
tipo: meta
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
tags: [meta, protocolo]
---

# 🧭 Cómo usar esta bóveda (protocolo de Claude)

> 🇧🇷 Lee esta página en portugués → [[🧭 Como usar este cofre (PT)|🧭 Como usar este cofre]]
> 🇺🇸 Read this page in English → [[🧭 Como usar este cofre (EN)]]

> [!important] Protocolo de memoria
> Al **inicio** de cada sesión, leer: [[🏠 Home (ES)|Home]], [[📌 Backlog (ES)|Backlog]], [[🔑 Fatos-Chave (ES)|Datos Clave]] y la nota madre [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].
> Al **final** de cada sesión, actualizar el [[📌 Backlog (ES)|Backlog]] y las notas afectadas.

## ✍️ Cuándo registrar memoria
| Situación | Dónde registrar |
|----------|-------------|
| Descubrí estructura/comportamiento del proyecto | `🧩 Sistemas/` (o la nota madre en `💼 Negócio/`) |
| Aprendí un concepto o patrón reutilizable | `📚 Conhecimento/` |
| Tomamos una decisión de arquitectura | `⚖️ Decisões/` |
| Preferencia o contexto de Renato | `🧭 Meta/👤 Renato.md` |
| Terminé una etapa de trabajo | actualizar `📌 Backlog.md` (raíz) |
| Configuración de herramienta | `🧭 Meta/` |

## 🔗 Reglas de escritura
1. **Usa siempre frontmatter** (`tipo`, `criado`, `atualizado`, `tags`).
2. **Interconecta** con `[[wikilinks]]` — el valor de la bóveda está en las conexiones.
3. **Atomicidad**: una idea por nota cuando sea posible.
4. **Fechas en ISO** `AAAA-MM-DD`.
5. Usa **callouts** (`> [!note]`, `> [!warning]`) para destacar.
6. Nada de secretos/contraseñas en la bóveda.

## 🧩 Plugins recomendados (opcionales)
- **Dataview** — opcional; la [[🏠 Home (ES)|Home]] actual no depende de él.
- **Templater** — plantillas avanzadas.
Sin ellos, la bóveda funciona con normalidad; solo los bloques `dataview` se convierten en texto plano.
