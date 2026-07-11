---
tipo: conhecimento
projeto: GitExtensions.ZimerfeldTree
lang: es-ES
atualizado: 2026-07-04
criado: 2026-06-01
tags: [conhecimento, git, gitflow]
---

# git flow — claves de config (CLI)

> 🇧🇷 Lee esta página en portugués → [[⚙️ git flow - chaves de config (CLI)]]
> 🇺🇸 Read this page in English → [[⚙️ git flow - chaves de config (CLI) (EN)]]

## Problema
GitExtensions escribe la configuración de gitflow en **su propio formato interno** (`gitflow.branch.develop.type=base`, etc.), pero el plugin **Plugins → Gitflow** usa el **git flow CLI**, que espera claves distintas. Como las claves esperadas no existen, el plugin cree que gitflow nunca se inicializó y sigue mostrando **"Init Gitflow"**.

## Solución — añadir las claves en el formato estándar
```bash
git config gitflow.branch.master main
git config gitflow.branch.develop develop
git config gitflow.prefix.feature feature/
git config gitflow.prefix.bugfix bugfix/
git config gitflow.prefix.release release/
git config gitflow.prefix.hotfix hotfix/
git config gitflow.prefix.support support/
git config gitflow.prefix.versiontag ""
```

> Fuente: `GitFlowFix.txt` en la raíz del repositorio [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]].

---

## Mantenimiento — refs remotas obsoletas

### Problema
Las branches eliminadas en el servidor siguen apareciendo en el árbol de Git Extensions bajo `origin/feature/` porque Git **no elimina automáticamente** los punteros locales de seguimiento.

### Limpieza manual
```bash
git remote prune origin
```

### Regla permanente (configurada el 2026-06-02)
```bash
git config --global fetch.prune true
```
Con esta configuración, cada `git fetch` (incluido F5 en Git Extensions) elimina automáticamente las refs obsoletas.

## 🔗 Relacionado
- [[🌳 GitExtensions.ZimerfeldTree (ES)|GitExtensions.ZimerfeldTree]]
