---
tipo: meta
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
criado: 2026-06-01
tags: [meta, protocolo]
---

# 🧭 Como usar este cofre (protocolo do Claude)

> [!important] Protocolo de memória
> No **início** de cada sessão, leia: [[🏠 Home]], [[📌 Backlog]], [[🔑 Fatos-Chave]] e a nota-mãe [[🌳 GitExtensions.ZimerfeldTree]].
> No **fim** de cada sessão, atualize o [[📌 Backlog]] e as notas afetadas.

## ✍️ Quando gravar memória
| Situação | Onde gravar |
|----------|-------------|
| Descobri estrutura/comportamento do projeto | `🧩 Sistemas/` (ou a nota-mãe em `💼 Negócio/`) |
| Aprendi um conceito ou padrão reutilizável | `📚 Conhecimento/` |
| Tomamos uma decisão de arquitetura | `⚖️ Decisões/` |
| Preferência ou contexto do Renato | `🧭 Meta/👤 Renato.md` |
| Terminei uma etapa de trabalho | atualizar `📌 Backlog.md` (raiz) |
| Configuração de ferramenta | `🧭 Meta/` |

## 🔗 Regras de escrita
1. **Sempre use frontmatter** (`tipo`, `criado`, `atualizado`, `tags`).
2. **Interligue** com `[[wikilinks]]` — o valor do cofre está nas conexões.
3. **Atomicidade**: uma ideia por nota quando possível.
4. **Datas em ISO** `AAAA-MM-DD`.
5. Use **callouts** (`> [!note]`, `> [!warning]`) para destaques.
6. Nada de segredos/senhas no cofre.

## 🧩 Plugins recomendados (opcionais)
- **Dataview** — opcional; a [[🏠 Home]] atual não depende dele.
- **Templater** — templates avançados.
Sem eles, o cofre funciona normalmente; só os blocos `dataview` viram texto.
