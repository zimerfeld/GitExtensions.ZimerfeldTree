---
tipo: conhecimento
criado: 2026-06-06
tags: [conhecimento, git, hierarquia, branch, bfs, ancestralidade]
---

# Hierarquia de branches — branches no mesmo commit

## Resumo

Quando duas branches apontam para o mesmo commit, o algoritmo BFS do ZimerfeldTree não consegue estabelecer relação pai-filho entre elas — porque git não registra de onde uma branch foi criada, apenas para qual commit ela aponta.

## Detalhes

O algoritmo `FindParentInGraph` (em `BranchHierarchyService.cs`) funciona por BFS:

1. Pega o SHA do tip da branch sendo analisada
2. Enfileira os **pais** desse commit (não o commit em si)
3. Sobe o grafo; a primeira branch encontrada = parent

Quando `branch A` e `branch B` compartilham o mesmo SHA:
- O BFS de A parte dos pais do SHA compartilhado
- B está **no mesmo nível** de partida — nunca é encontrada como ancestral
- Resultado: nenhuma relação é estabelecida, ambas aparecem como raízes

## Exemplo

```
# Situação problemática — ambas no mesmo commit
git log --oneline feature/gridsolo feature/mododebug
* c19d7dc  ← HEAD de AMBAS as branches

# Situação correta — gridsolo um commit à frente
* cea86c1  ← feature/gridsolo   (aparece como filha de mododebug na árvore)
* c19d7dc  ← feature/mododebug
```

## Quando acontece

- Branch criada (`git checkout -b nova-branch base`) sem nenhum commit em seguida
- Branch cujo trabalho foi "absorvido" de volta (fast-forward faz base alcançar a filha)

## Solução no ZimerfeldTree

Ao criar uma branch com o checkbox **based on:** marcado na janela GitFlow → Start, o plugin executa automaticamente:

```
git commit --allow-empty -m "chore: start <nome-da-branch>"
```

Isso garante que a nova branch imediatamente diverge de sua base, tornando a hierarquia visível sem precisar de um commit de conteúdo.

## 🔗 Relacionado

- [[Interface GitFlow — botões e fluxos]]
- [[2026-06-06 - Hierarquia branches mesmo commit, commit automático no Start]]
