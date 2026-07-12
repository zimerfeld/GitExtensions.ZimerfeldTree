---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, git]
status: aceita
criado: 2026-06-05
---

# ADR — GitFlow ejecutado en git puro (sin el binario git-flow)

> 🇧🇷 Lee esta página en portugués → [[🔀 GitFlow em git puro]]
> 🇺🇸 Read this page in English → [[🔀 GitFlow em git puro (EN)]]

## Contexto
Había dos formas de implementar los comandos de GitFlow: (a) depender del binario externo **`git-flow`** instalado en la máquina, o (b) reproducir cada operación con **git nativo**. El binario externo añade una dependencia de instalación, tiene variaciones de puerto (`git-flow-avh` frente al clásico) y claves de configuración propias que GitExtensions no siempre escribe en el formato esperado.

## Decisión
Implementar **todos** los comandos de GitFlow (start / publish / track / update / finish, para feature/bugfix/release/hotfix/support) como **secuencias de git nativo** dentro de [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]. El binario `git-flow` **no** necesita estar instalado.

## Consecuencias
**Positivas:**
- Cero dependencias externas más allá del propio Git for Windows.
- Control total sobre cada paso → posibilita el **GitFlow flexible** (feature hija de feature) y el *finish release* automático (merge en main + tag + merge en develop + push de todo).
- Registro visible de cada comando git ejecutado.

**Trade-offs:**
- El plugin reimplementa la semántica de git-flow y debe mantenerla correcta.
- Las claves `gitflow.*` de configuración existen solo por compatibilidad visual/CLI — ver [[⚙️ git flow - chaves de config (CLI) (ES)|git flow - claves de configuración (CLI)]].

## 🔗 Relacionado
- [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de feature]]
- [[🔀 GitFlowForm (ES)|GitFlowForm]]
- [[👁️ Visão Geral (ES)|Visión general]]
