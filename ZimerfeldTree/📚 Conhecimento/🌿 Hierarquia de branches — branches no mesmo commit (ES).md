---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-06
tags: [conhecimento, git, hierarquia, branch, bfs, ancestralidade]
---

# Jerarquía de branches — branches en el mismo commit

> 🇧🇷 Lee esta página en portugués → [[🌿 Hierarquia de branches — branches no mesmo commit (PT)|🌿 Hierarquia de branches — branches no mesmo commit]]
> 🇺🇸 Read this page in English → [[🌿 Hierarquia de branches — branches no mesmo commit (EN)]]

## Resumen

Cuando dos branches apuntan al mismo commit, el algoritmo BFS de ZimerfeldTree no puede establecer una relación padre-hijo entre ellas — porque git no registra de dónde se creó una branch, solo a qué commit apunta.

## Detalles

El algoritmo `FindParentInGraph` (en `BranchHierarchyService.cs`) funciona por BFS:

1. Toma el SHA del tip de la branch analizada
2. Encola los **padres** de ese commit (no el commit en sí)
3. Sube por el grafo; la primera branch encontrada = parent

Cuando `branch A` y `branch B` comparten el mismo SHA:
- El BFS de A parte de los padres del SHA compartido
- B está **en el mismo nivel** de partida — nunca se encuentra como ancestro
- Resultado: no se establece ninguna relación, ambas aparecen como raíces

## Ejemplo

```
# Situación problemática — ambas en el mismo commit
git log --oneline feature/gridsolo feature/mododebug
* c19d7dc  ← HEAD de AMBAS las branches

# Situación correcta — gridsolo un commit por delante
* cea86c1  ← feature/gridsolo   (aparece como hija de mododebug en el árbol)
* c19d7dc  ← feature/mododebug
```

## Cuándo ocurre

- Branch creada (`git checkout -b nova-branch base`) sin ningún commit posterior
- Branch cuyo trabajo fue "absorbido" de vuelta (un fast-forward hace que la base alcance a la hija)

## Solución en ZimerfeldTree

Al crear una branch con la casilla **based on:** marcada en la ventana GitFlow → Start, el plugin ejecuta automáticamente:

```
git commit --allow-empty -m "chore: start <nombre-de-branch>"
```

Esto garantiza que la nueva branch diverja de inmediato de su base, haciendo visible la jerarquía sin necesitar un commit de contenido.

## 🔗 Relacionado

- [[🔀 Interface GitFlow — botões e fluxos (ES)|Interfaz GitFlow — botones y flujos]]
