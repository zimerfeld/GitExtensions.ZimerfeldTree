---
tipo: sessao
data: 2026-06-06
hora: 00:00
tags: [sessao, git, hierarquia, gitflow, branch, cascata]
resumo: Diagnóstico de por que duas branches no mesmo commit não formam hierarquia pai-filho; solução com commit automático no DoStart quando "based on" está marcado
projetos: [GitExtensions.ZimerfeldTree]
---

# Sessão 2026-06-06 — Hierarquia branches mesmo commit, commit automático no Start

## 🎯 Pedidos do Renato

1. Explicar por que `feature/gridsolo` não aparece como filha de `feature/mododebug` na árvore, dado que foi criada a partir dela
2. Ao final do Start (quando checkbox "based on" estiver marcado), fazer um commit automático na branch recém-criada

---

## ✅ O que foi feito

### 1. Diagnóstico — branches no mesmo commit não formam hierarquia

**Causa raiz:** `feature/gridsolo` e `feature/mododebug` apontavam para o **exato mesmo commit** (`c19d7dc`).

O algoritmo BFS em `BranchHierarchyService.FindParentInGraph` funciona assim:
1. Monta `tipToName`: SHA → nome da branch *(last writer wins — apenas um nome por SHA)*
2. Parte do commit-tip da branch sendo analisada
3. Sobe o grafo pelos **pais** desse commit
4. Primeira branch encontrada = parent

Quando dois branches compartilham o mesmo SHA, o BFS de `gridsolo` começa pelos **pais do commit compartilhado** — e jamais encontra `mododebug` como ancestral, porque ela está no mesmo nível de partida, não acima. Nenhuma relação pai-filho pode ser estabelecida.

**Isso não é uma limitação do plugin — é uma limitação do git:** quando uma branch é criada a partir de outra sem receber commits próprios, o git não registra nenhuma relação de parentesco; ambas são simplesmente aliases do mesmo commit.

```
ANTES (sem hierarquia):
c19d7dc ← feature/gridsolo
c19d7dc ← feature/mododebug   (mesmo commit — BFS não acha pai)

DEPOIS (hierarquia correta):
cea86c1 ← feature/gridsolo    (novo commit)
c19d7dc ← feature/mododebug   (BFS de gridsolo encontra mododebug aqui)
```

---

### 2. Solução — commit automático no DoStart com "based on"

**Onde:** `GitFlowForm.cs → DoStart()` (linha ~527)

**Lógica:** após `checkout -b` criar e fazer checkout da nova branch com sucesso, se `_chkBasedOn.Checked` for verdadeiro, executa um commit vazio:

```csharp
if (_chkBasedOn.Checked)
    RunFlow($"commit --allow-empty -m \"chore: start {fullBranch}\"");
```

**Por que `--allow-empty`:** a working tree recém-criada não tem mudanças. O flag permite criar um commit sem nenhum arquivo staged — apenas move o ponteiro da branch um commit à frente da base, tornando a hierarquia imediatamente visível na árvore.

**Por que só quando "based on" está marcado:** o caso padrão (base implícita = develop/main) não precisa do commit — `develop` e `main` já são branches com histórico bem estabelecido e a hierarquia já funciona corretamente. O commit vazio é necessário apenas quando o usuário está criando uma sub-branch explícita de outra feature.

---

## 📁 Arquivos Modificados

| Arquivo | Mudança |
|---|---|
| `GitFlowForm.cs` | `DoStart()`: `RunFlow("commit --allow-empty -m ...")` após checkout bem-sucedido quando `_chkBasedOn.Checked` |

---

## 🧠 Aprendizados / Decisões Técnicas

- **Git não tem memória de origem de branch.** Criada a partir de onde for, sem commits próprios, uma branch é indistinguível da sua base para qualquer ferramenta que usa o grafo de commits.
- **`--allow-empty` é a ferramenta certa aqui.** Não força o usuário a ter mudanças staged, não polui o histórico com arquivos desnecessários, e é convencional para commits de "início de trabalho".
- O commit vazio só ocorre quando a relação pai-filho é explícita (checkbox marcado). Nos outros casos, o comportamento original é preservado.

## 🔗 Notas relacionadas

- [[Hierarquia de branches — branches no mesmo commit]]
- [[Interface GitFlow — botões e fluxos]]
