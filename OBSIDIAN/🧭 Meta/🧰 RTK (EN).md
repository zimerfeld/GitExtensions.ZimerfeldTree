---
tipo: meta
projeto: GitExtensions.ZimerfeldTree
lang: en-US
atualizado: 2026-07-04
criado: 2026-06-01
tags: [ferramenta, cli, rtk]
---

# 🦀 RTK — Rust Token Killer

## Summary
CLI proxy that saves **60–90% of tokens** in development operations. It automatically rewrites commands via a Claude Code hook (e.g. `git status` → `rtk git status`, transparent, 0 overhead tokens).

## Meta-commands (use rtk directly)
```bash
rtk gain              # Shows token-savings analytics
rtk gain --history    # Command usage history with savings
rtk discover          # Analyzes Claude Code history for missed opportunities
rtk proxy <cmd>       # Runs a raw command with no filter (debug)
```

## Installation check
```bash
rtk --version         # rtk X.Y.Z
rtk gain              # Should work (not "command not found")
which rtk             # Verify the correct binary
```

> [!warning] Name collision
> If `rtk gain` fails, you may have reachingforthejack/rtk (Rust Type Kit) installed instead of the correct one.

## 🔗 Related
- [[🔑 Fatos-Chave (EN)|Key Facts]]
- [[👤 Renato (EN)|Renato]]
