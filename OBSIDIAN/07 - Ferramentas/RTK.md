---
tipo: ferramenta
criado: 2026-06-01
atualizado: 2026-06-01
tags: [ferramenta, cli, rtk]
---

# 🦀 RTK — Rust Token Killer

## Resumo
Proxy CLI que economiza **60–90% de tokens** em operações de desenvolvimento. Reescreve comandos automaticamente via hook do Claude Code (ex.: `git status` → `rtk git status`, transparente, 0 tokens de overhead).

## Meta-comandos (usar rtk direto)
```bash
rtk gain              # Mostra analytics de economia de tokens
rtk gain --history    # Histórico de uso de comandos com economia
rtk discover          # Analisa histórico do Claude Code por oportunidades perdidas
rtk proxy <cmd>       # Executa comando cru sem filtro (debug)
```

## Verificação de instalação
```bash
rtk --version         # rtk X.Y.Z
rtk gain              # Deve funcionar (não "command not found")
which rtk             # Verificar binário correto
```

> [!warning] Colisão de nome
> Se `rtk gain` falhar, talvez exista o reachingforthejack/rtk (Rust Type Kit) instalado em vez do correto.

## 🔗 Relacionado
- [[🔑 Fatos-Chave]]
- [[Renato]]
