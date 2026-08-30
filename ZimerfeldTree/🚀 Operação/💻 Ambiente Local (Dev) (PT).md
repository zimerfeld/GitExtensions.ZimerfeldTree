---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [operacao, dev, build, install, powershell]
---

# 💻 Ambiente Local (Dev)

> [!abstract] 🎯 Objetivo
> Compilar o plugin **ZimerfeldTree** e instalá-lo no GitExtensions local para desenvolver e testar. Conteúdo derivado de [[🔧 build.ps1 (PT)|🔧 build.ps1]], [[🏷️ Versionamento (PT)|🏷️ Versionamento]] e [[📘 README — Instalação, Uso e Build (PT)|📘 README — Instalação, Uso e Build]].

## ⚡ TL;DR — o comando único

```powershell
# na raiz do repo, como Administrador
.\build.ps1
```

Compila, incrementa a versão, instala a DLL no GitExtensions local e gera o `.nupkg` — tudo em um passo. Para iterar rápido sem mexer na versão:

```powershell
.\tools\update-dll.ps1      # só copia a DLL compilada (requer Admin)
```

## ⚙️ O que o script faz (em ordem)

```
build.ps1
  ├─ 1.  Lê versão atual do .nuspec
  ├─ 1b. Detecta mudanças (fontes + textos) vs. último .nupkg → sem mudanças encerra
  ├─ 1c. Fecha GitExtensions e plugins antes de compilar
  ├─ 2.  Bump no .nuspec  ← <version>
  ├─ 3.  Bump no .csproj  ← <Version>
  ├─ 4.  Atualiza link do NuGet e "Versão atual" no README.md
  ├─ 4b. Carimba cabeçalho (Versão/Atualizado) nos READMEs (md / pt-BR / en-US)
  ├─ 4c. Carimba o cofre Obsidian (notas que refletem a versão)
  ├─ 5.  dotnet build -c Release
  ├─ 6.  Copia DLL → C:\Program Files\GitExtensions\Plugins\  (requer Admin)
  │       e atualiza tools\net9.0-windows\  (para o nupkg)
  ├─ 7.  nuget pack .nuspec → .nupkg na raiz
  └─ —   Remove .nupkg de versões anteriores
```

## 🚩 Parâmetros / flags

- `-Force` — empacota mesmo sem mudanças detectadas (a detecção incremental compara as entradas do pacote contra o último `.nupkg`, não contra a DLL — de propósito, para evitar rebuild em loop).

## 🧰 Scripts auxiliares (`tools\`)

| Script | Função |
|---|---|
| `install.ps1` | Instala a DLL no GitExtensions (Admin) |
| `uninstall.ps1` | Remove a DLL (Admin) — não afeta nada mais do GitExtensions |
| `update-dll.ps1` | Deploy rápido só da DLL, sem bump de versão (Admin) |

## 📐 Regras que respeita

- **Versionamento** `major.minor.BUILD` — só o `BUILD` é incrementado automaticamente; fonte da verdade: `.nuspec` / `.csproj`. Ver [[🏷️ Versionamento (PT)|🏷️ Versionamento]].
- **Docs carimbados antes do pack** — READMEs e cofre são atualizados antes do passo 7, mantendo o `.nupkg` como artefato mais recente (detecção por timestamp correta).
- **GitFlow** — desenvolvimento em feature branch (regra global do Renato); o build não interage com git.

## 🔧 Pré-requisitos

- **.NET SDK 9** (compila `net9.0-windows`) e **NuGet CLI** (resolvido via PATH → `tools\nuget.exe` → download automático).
- **GitExtensions 4.x** instalado em `C:\Program Files\GitExtensions\`.
- PowerShell **como Administrador** para o deploy da DLL.
- Detalhes de instalação de cada dependência: [[📦 Dependências do ZimerfeldTree (PT)|📦 Dependências do ZimerfeldTree]].

## 🩹 Troubleshooting

- **Sem Admin** → o passo 6 (deploy da DLL) é pulado com aviso; rode o terminal como Administrador.
- **"Sem mudanças" e você quer empacotar assim mesmo** → use `.\build.ps1 -Force`.
- **Aviso NU5101 no pack** → intencional e filtrado: a DLL fica direto em `lib\` (grupo "any") porque o Plugin Manager do GitExtensions só extrai esse grupo; uma subpasta `net9.0-windows` quebraria a instalação.
- **GitExtensions aberto** → o próprio script fecha o GitExtensions antes de compilar (passo 1c).
- **GitExtensions 3.x** → incompatível (.NET Framework 4.8); o plugin requer `net9.0-windows`.

## 🔗 Ligações

- [[🔧 build.ps1 (PT)|🔧 build.ps1]] — a nota do arquivo-chave
- [[🏷️ Versionamento (PT)|🏷️ Versionamento]] — esquema de versão e ciclo completo
- [[📦 Dependências do ZimerfeldTree (PT)|📦 Dependências do ZimerfeldTree]] — instalação passo a passo das dependências
- [[📘 README — Instalação, Uso e Build (PT)|📘 README — Instalação, Uso e Build]] — espelho do README
- [[🚀 Deploy em Produção (Prod) (PT)|🚀 Deploy em Produção (Prod)]] — publicação da release
