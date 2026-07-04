---
tipo: decisao
tags: [decisao, adr, restore, recuperacao, ux]
status: aceita
criado: 2026-06-07
atualizado: 2026-07-04
---

# ADR — Restore como central única de "voltar no tempo"

## Contexto
As operações de desfazer/recuperar do git (restore, revert, reset, cherry-pick, reflog, rebase, discard) são poderosas mas **assustadoras e dispersas**, com riscos muito diferentes entre si — de trivial (restaurar um arquivo) a destrutivo (reset --hard, rebase). O usuário raramente lembra o comando certo e o grau de perigo.

## Decisão
Concentrar tudo numa janela **`RestoreForm`** com **10 abas ordenadas da mais segura à mais destrutiva**, cada categoria com **explicação embutida** e orientações de trabalho em equipe, além de uma aba **"Sobre o Restore"** rolável. Ordem: Plano de Emergência → Restaurar Arquivo/Árvore/Tag → Cherry-Pick → Reverter → Reset → Nova Branch/Tag → Reflog → Descartar Locais → Rebase.

## Consequências
**Positivas:** um único lugar para recuperar-se de erros; a ordem por risco educa e reduz acidentes; o **Reflog** dá rede de segurança mesmo após operações destrutivas.

**Trade-offs:** janela grande (~980 px, 10 abas) — mitigado pela ordenação por risco e pelas explicações por categoria.

## 🔗 Relacionado
- [[../Arquivos-Chave/RestoreForm]]
- [[Modo Developer protege main-develop]]
- [[Interface Restore — botões e fluxos]]
- [[../Fluxos/3 - Restore (voltar no tempo)]]
