---
tipo: indice
criado: 2026-06-01
atualizado: 2026-06-01
tags: [moc, home, indice]
---

# 🧠 Cofre de Neurônios — Memória do Claude

> [!abstract] O que é isto
> Este é o **cérebro persistente** do Claude. Tudo que eu (Claude) aprender, decidir, configurar ou descobrir sobre os projetos do Renato é gravado aqui, em notas interligadas. Da próxima vez que trabalharmos juntos, eu leio este cofre primeiro e recupero o contexto.

## 🗺️ Mapa de Conteúdo (MOC)

### 📌 Navegação rápida
- [[📥 Inbox]] — captura rápida, ainda não organizado
- [[🔑 Fatos-Chave]] — verdades sempre úteis (paths, nomes, convenções)
- [[🧭 Como usar este cofre]] — protocolo de leitura/escrita do Claude

### 🗂️ Áreas
| Pasta | Conteúdo |
|-------|----------|
| `01 - Projetos` | Um arquivo por projeto/repositório |
| `02 - Conhecimento` | Conceitos técnicos, padrões, snippets reutilizáveis |
| `03 - Decisões` | ADRs — registros de decisões de arquitetura |
| `04 - Pessoas e Contexto` | Preferências do Renato, equipe, contexto de negócio |
| `05 - Sessões` | Log cronológico de cada sessão de trabalho |
| `06 - Bancos de Dados` | DB2 (HSGUCA, HSGUWEB, DB2CA, DBWEB), tabelas, queries |
| `07 - Ferramentas` | RTK, Obsidian, CLIs, MCP servers |
| `99 - Templates` | Modelos de notas |

## 🔥 Atalhos de projetos
```dataview
LIST
FROM "01 - Projetos"
SORT file.mtime DESC
```
> _Se o plugin Dataview não estiver instalado, este bloco aparece como texto. Veja [[🧭 Como usar este cofre]]._

## 🕒 Sessões recentes
```dataview
TABLE data as "Data", resumo as "Resumo"
FROM "05 - Sessões"
SORT file.name DESC
LIMIT 10
```

## 🏷️ Tags principais
#projeto #decisao #conhecimento #sessao #db2 #ferramenta #preferencia

---
_Cofre criado em 2026-06-01 por Claude. Mantido automaticamente a cada sessão._
