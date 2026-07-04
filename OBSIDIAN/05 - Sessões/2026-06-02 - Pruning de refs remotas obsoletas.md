---
tipo: sessao
data: 2026-06-02
hora: 00:00
tags: [sessao, git, gitflow, gitextensions]
resumo: Remoção de referências remotas obsoletas e configuração de auto-prune
projetos: [GitExtensions.ZimerfeldTree]
---

# Sessão 2026-06-02 — Pruning de refs remotas obsoletas

## 🎯 Pedido do Renato
- Entender por que branches já deletados do remote ainda apareciam na árvore do Git Extensions sob `origin/feature/`
- Criar regra permanente para que nunca mais apareçam

## ✅ O que foi feito

### 1. Remoção manual das refs obsoletas
```powershell
git remote prune origin
```
Foram removidas 3 referências stale:
- `origin/feature/alinha`
- `origin/feature/basedon`
- `origin/feature/code_B1`

### 2. Configuração global de auto-prune
```powershell
git config --global fetch.prune true
```
A partir de agora, todo `git fetch` (inclusive o F5 do Git Extensions) remove automaticamente refs remotas que não existem mais no servidor.

## 🧠 Aprendizados / decisões
- Refs remotas obsoletas ocorrem porque o Git **não remove automaticamente** os ponteiros locais de rastreamento quando o branch é deletado no servidor.
- `fetch.prune = true` é a solução definitiva — sem necessidade de rodar `git remote prune` manualmente.
- A config foi aplicada **globalmente** (`--global`) para valer em todos os repositórios.

## 📝 Arquivos tocados
- `~/.gitconfig` — adicionada chave `fetch.prune = true`

## ⏭️ Próximos passos
- [x] Atualizar F5 no Git Extensions para confirmar que a árvore está limpa

## 🔗 Notas relacionadas
- [[git flow - chaves de config (CLI)]]
- [[GitExtensions.ZimerfeldTree]]
