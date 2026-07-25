---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
tags: [decisao, adr, ui, arvore, hierarquia]
status: aceita
criado: 2026-06-01
---

# ADR — Árbol jerárquico de branches en lugar de una lista plana

> 🇧🇷 Lee esta página en portugués → [[🌳 Árvore hierárquica vs lista plana (PT)|🌳 Árvore hierárquica vs lista plana]]
> 🇺🇸 Read this page in English → [[🌳 Árvore hierárquica vs lista plana (EN)]]

## Contexto
GitExtensions (y la mayoría de los clientes git) muestra las branches en una **lista plana**. En repositorios que adoptan GitFlow, el nombre lleva una jerarquía implícita (`feature/login`, `release/1.2`, `feature/login-oauth` derivando de `feature/login`) que la lista plana oculta, dificultando ver el parentesco y la organización.

## Decisión
Mostrar las branches en un **árbol jerárquico**, en 3 secciones fijas (**LOCAL / REMOTES / TAGS**), combinando **dos dimensiones**:
1. **Ascendencia real** — parentesco por commits / relaciones de GitFlow (vía el grafo de `git log --all` + BFS).
2. **Agrupación por ruta** — el separador `/` del nombre se convierte en carpeta en el árbol.

## Consecuencias
**Positivas:**
- Contexto visual inmediato de quién deriva de quién.
- Contadores por sección `(N)` y barra de estado `Local: N | Remoto: N | Tags: N`.
- Base para el GitFlow flexible (feature hija de feature).

**Trade-offs:**
- La agrupación por carpeta es **por nombre (`/`)**, no por parentesco de commit — `master` y `develop` aparecen como hermanas.
- Una **branch real no puede ser nodo padre** de otra (la ref sería archivo **y** directorio a la vez). Ver [[🌿 Hierarquia de branches — branches no mesmo commit (ES)|Jerarquía de branches — branches en el mismo commit]].

## 🔗 Relacionado
- [[⚙️ BranchHierarchyService (ES)|BranchHierarchyService]]
- [[🌿 GitFlow flexível — feature filha de feature (ES)|GitFlow flexible — feature hija de feature]]
- [[👁️ Visão Geral (ES)|Visión general]]
