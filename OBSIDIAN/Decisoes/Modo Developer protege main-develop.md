---
tipo: decisao
tags: [decisao, adr, ui, seguranca, branches]
status: aceita
criado: 2026-06-16
atualizado: 2026-07-04
---

# ADR — "Modo Developer" para proteger main / master / develop

## Contexto
A janela permite seleção múltipla por checkbox e **exclusão em lote** de branches. As branches de longa vida (`main`/`master`/`develop`) são as mais perigosas de apagar por acidente.

## Decisão
Adicionar um checkbox **"Modo Developer"** (ao lado de "Show Debug"):
- **Desligado (padrão):** `main`/`master`/`develop` ficam **protegidas** — checkbox bloqueado, não podem ser marcadas nem excluídas.
- **Ligado:** libera a marcação/exclusão dessas branches específicas.

Desativar o modo **desmarca automaticamente** qualquer main/master/develop que estivesse marcada. Estado persistido em `ZimerfeldTree.uisettings.json`.

## Consequências
**Positivas:** protege por padrão o estado que reflete produção; a liberação é explícita e consciente.

**Trade-offs:** um passo a mais para quem realmente precisa mexer nessas branches (aceitável — é justamente a intenção).

## 🔗 Relacionado
- [[../Arquivos-Chave/BranchHierarchyForm]]
- [[Restore — central de voltar no tempo]]
- [[Interface ZimerfeldTree — botões e fluxos]]
