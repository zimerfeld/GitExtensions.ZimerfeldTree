---
tipo: decisao
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [decisao, adr, gitflow, git]
status: aceita
criado: 2026-06-05
---

# ADR — GitFlow executado em git puro (sem o binário git-flow)

## Contexto
Havia duas formas de implementar os comandos GitFlow: (a) depender do binário externo **`git-flow`** instalado na máquina, ou (b) reproduzir cada operação com **git nativo**. O binário externo adiciona uma dependência de instalação, tem variações de porta (`git-flow-avh` vs. clássico) e chaves de config próprias que o GitExtensions nem sempre grava no formato esperado.

## Decisão
Implementar **todos** os comandos GitFlow (start / publish / track / update / finish, para feature/bugfix/release/hotfix/support) como **sequências de git nativo** dentro do [[⚙️ BranchHierarchyService (PT)|BranchHierarchyService]]. O binário `git-flow` **não** precisa estar instalado.

## Consequências
**Positivas:**
- Zero dependência externa além do próprio Git for Windows.
- Controle total sobre cada passo → possibilita o **GitFlow flexível** (feature filha de feature) e o *finish release* automático (merge main + tag + merge develop + push de tudo).
- Log visível de cada comando git executado.

**Trade-offs:**
- O plugin reimplementa a semântica do git-flow e precisa mantê-la correta.
- Chaves `gitflow.*` de config existem só para compatibilidade visual/CLI — ver [[⚙️ git flow - chaves de config (CLI) (PT)|⚙️ git flow - chaves de config (CLI)]].

## 🔗 Relacionado
- [[🌿 GitFlow flexível — feature filha de feature (PT)|🌿 GitFlow flexível — feature filha de feature]]
- [[🔀 GitFlowForm (PT)|🔀 GitFlowForm]]
- [[👁️ Visão Geral (PT)|👁️ Visão Geral]]
