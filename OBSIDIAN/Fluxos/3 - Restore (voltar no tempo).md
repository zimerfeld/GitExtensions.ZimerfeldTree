---
tipo: fluxo
tags: [fluxo, restore, recuperacao, desfazer, etapa3]
atualizado: 2026-07-04
---

# Fluxo: 3 — Restore (voltar no tempo)

Janela `RestoreForm`, aberta pelo botão **Restore**. 10 abas ordenadas da mais segura à mais destrutiva.

![[Anexos/ScreenShots/ScreenshotRestore.png]]

## Passos

```
[Restore]  →  escolher a aba conforme o objetivo (segura → destrutiva):

  1. Plano de Emergência   branch ← tag (rede de segurança)
  2. Restaurar Arquivo     git restore (Procurar… restrito à raiz do repo)
  3. Restaurar Árvore      restaura a árvore num commit
  4. Restaurar Tag         checkout/restore a partir de tag
  5. Cherry-Pick           git cherry-pick
  6. Reverter              git revert (commit / merge -m 1)
  7. Reset Branch          git reset
  8. Nova Branch/Tag       cria ref (+ Inspecionar detached)
  9. Recuperar (Reflog)    git reflog → recuperar refs perdidas
 10. Descartar Locais      checkout / reset --hard HEAD / clean
     Rebase                remove commit
```

## Detalhes

- Cada categoria traz **explicação embutida** + orientações de trabalho em equipe; aba **"Sobre o Restore"** rolável.
- O **Reflog** é a rede de segurança mesmo após operações destrutivas.
- Ao fechar, a janela principal retoma o foco e atualiza a árvore.

## Relacionado

- [[1 - Abrir e navegar a árvore]]
- [[2 - GitFlow (Start a Finish)]]
- [[../Arquivos-Chave/RestoreForm]]
- [[../Decisoes/Restore — central de voltar no tempo]]
- [[Interface Restore — botões e fluxos]]
