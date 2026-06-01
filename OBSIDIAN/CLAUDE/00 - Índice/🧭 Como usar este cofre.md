---
tipo: guia
criado: 2026-06-01
tags: [meta, protocolo]
---

# 🧭 Como usar este cofre (protocolo do Claude)

> [!important] Protocolo de memória
> No **início** de cada sessão, leia: [[🔑 Fatos-Chave]], a nota do projeto relevante em `01 - Projetos`, e as últimas notas de `05 - Sessões`.
> No **fim** de cada sessão, crie uma nota em `05 - Sessões` e atualize as notas afetadas.

## ✍️ Quando gravar memória
| Situação | Onde gravar |
|----------|-------------|
| Descobri estrutura/comportamento de um projeto | `01 - Projetos/<projeto>.md` |
| Aprendi um conceito ou padrão reutilizável | `02 - Conhecimento/` |
| Tomamos uma decisão de arquitetura | `03 - Decisões/ADR-XXXX.md` |
| Preferência ou contexto do Renato | `04 - Pessoas e Contexto/Renato.md` |
| Terminei uma sessão de trabalho | `05 - Sessões/AAAA-MM-DD-tema.md` |
| Detalhe de tabela/query DB2 | `06 - Bancos de Dados/` |
| Configuração de ferramenta | `07 - Ferramentas/` |

## 🔗 Regras de escrita
1. **Sempre use frontmatter** (`tipo`, `criado`, `atualizado`, `tags`).
2. **Interligue** com `[[wikilinks]]` — o valor do cofre está nas conexões.
3. **Atomicidade**: uma ideia por nota quando possível.
4. **Datas em ISO** `AAAA-MM-DD`.
5. Use **callouts** (`> [!note]`, `> [!warning]`) para destaques.
6. Nada de segredos/senhas — DB2 usa 2FA, senha pedida a cada vez.

## 🧩 Plugins recomendados (opcionais)
- **Dataview** — ativa as listas dinâmicas da [[🧠 HOME - Cofre de Neurônios|HOME]].
- **Templater** — templates avançados.
Sem eles, o cofre funciona normalmente; só os blocos `dataview` viram texto.
