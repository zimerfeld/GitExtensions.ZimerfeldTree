---
tipo: meta
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
tags: [ferramenta, cli, rtk]
---

# 🦀 RTK — Rust Token Killer

> 🇧🇷 Lee esta página en portugués → [[🧰 RTK]]
> 🇺🇸 Read this page in English → [[🧰 RTK (EN)]]

## Resumen
Proxy CLI que ahorra **60–90% de tokens** en operaciones de desarrollo. Reescribe comandos automáticamente vía un hook de Claude Code (p. ej. `git status` → `rtk git status`, transparente, 0 tokens de sobrecarga).

## Meta-comandos (usar rtk directo)
```bash
rtk gain              # Muestra analíticas de ahorro de tokens
rtk gain --history    # Historial de uso de comandos con ahorro
rtk discover          # Analiza el historial de Claude Code en busca de oportunidades perdidas
rtk proxy <cmd>       # Ejecuta comando en crudo sin filtro (debug)
```

## Verificación de instalación
```bash
rtk --version         # rtk X.Y.Z
rtk gain              # Debe funcionar (no "command not found")
which rtk             # Verificar el binario correcto
```

> [!warning] Colisión de nombre
> Si `rtk gain` falla, tal vez exista el reachingforthejack/rtk (Rust Type Kit) instalado en lugar del correcto.

## 🔗 Relacionado
- [[🔑 Fatos-Chave (ES)|Datos Clave]]
- [[👤 Renato (ES)|Renato]]
