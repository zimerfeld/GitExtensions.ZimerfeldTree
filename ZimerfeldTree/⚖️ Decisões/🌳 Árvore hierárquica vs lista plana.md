---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [decisao, adr, ui, arvore, hierarquia]
status: aceita
criado: 2026-06-01
---

# ADR — Árvore hierárquica de branches em vez de lista plana

## Contexto
O GitExtensions (e a maioria dos clientes git) exibe as branches numa **lista plana**. Em repositórios que adotam GitFlow, o nome carrega hierarquia implícita (`feature/login`, `release/1.2`, `feature/login-oauth` derivando de `feature/login`) que a lista plana esconde, dificultando enxergar parentesco e organização.

## Decisão
Exibir as branches numa **árvore hierárquica**, em 3 seções fixas (**LOCAL / REMOTES / TAGS**), combinando **duas dimensões**:
1. **Ancestralidade real** — parentesco por commits / relações GitFlow (via grafo de `git log --all` + BFS).
2. **Agrupamento por caminho** — o separador `/` do nome vira pasta na árvore.

## Consequências
**Positivas:**
- Contexto visual imediato de quem deriva de quem.
- Contadores por seção `(N)` e status bar `Local: N | Remoto: N | Tags: N`.
- Base para o GitFlow flexível (feature filha de feature).

**Trade-offs:**
- O agrupamento por pasta é **por nome (`/`)**, não por parentesco de commit — `master` e `develop` aparecem como irmãos.
- **Branch real não pode ser nó-pai** de outra (o ref seria arquivo **e** diretório). Ver [[🌿 Hierarquia de branches — branches no mesmo commit]].

## 🔗 Relacionado
- [[⚙️ BranchHierarchyService]]
- [[🌿 GitFlow flexível — feature filha de feature]]
- [[👁️ Visão Geral]]
