---
tipo: procedimento
projeto: GitExtensions.ZimerfeldTree
lang: pt-BR
atualizado: 2026-07-04
tags: [operacao, prod, release, nupkg, nuget, github]
---

# 🚀 Deploy em Produção (Prod)

> [!abstract] 🎯 Objetivo
> Publicar uma **release** do plugin: gerar o `.nupkg` versionado e disponibilizá-lo aos usuários (feed do **nuget.org**, de onde o Plugin Manager do GitExtensions instala, + release no GitHub). Conteúdo derivado de [[🔧 build.ps1]], [[🏷️ Versionamento]] e [[📘 README — Instalação, Uso e Build]].

## ⚡ TL;DR — o comando único

```powershell
# na raiz do repo, como Administrador
.\build.ps1
```

O `build.ps1` é o gerador do artefato de produção: incrementa a versão, compila em Release, carimba os docs e produz `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` na raiz do repo (removendo os `.nupkg` antigos). Em seguida, publicar esse `.nupkg` (ver abaixo).

## ⚙️ O que o script faz (em ordem)

1. Lê a versão atual do `.nuspec` e detecta mudanças (fontes + `*.md`) vs. o último `.nupkg` — sem mudanças, encerra (use `-Force` para forçar).
2. Calcula a nova versão (`major.minor.BUILD` — só o `BUILD` incrementa; major/minor são manuais).
3. Carimba versão + data **primeiro nos docs**: READMEs (`md` / `pt-BR` / `en-US`) e notas do cofre Obsidian.
4. Faz o bump no `.nuspec` (`<version>`) e no `.csproj` (`<Version>`).
5. `dotnet build -c Release`.
6. Copia a DLL para `C:\Program Files\GitExtensions\Plugins\` (requer Admin) e para `tools\net9.0-windows\`.
7. `nuget pack` → `.nupkg` na raiz; remove `.nupkg` de versões anteriores.

## 📦 Publicação da release

1. **nuget.org** — publicar o `GitExtensions.ZimerfeldTree.X.Y.Z.nupkg` no feed do nuget.org (pacote `GitExtensions.ZimerfeldTree`). É desse feed que o **Plugin Manager** do GitExtensions instala o plugin (opção A recomendada de instalação do README).
2. **GitHub release** — publicar a release correspondente no repositório do owner `zimerfeld`, anexando o `.nupkg` gerado.
3. Conferir no README se o link do NuGet e a "Versão atual" foram carimbados pelo `build.ps1` (passo automático 4/4b).

> [!warning] ⚠️ Requisitos do nupkg (não alterar)
> - A DLL fica **direto em `lib\`** (grupo "any") — o Plugin Manager só extrai grupos `lib` com moniker na lista dele; subpasta `net9.0-windows` quebraria a instalação (por isso o aviso NU5101 é filtrado, intencional).
> - `<dependency id="GitExtensions.Extensibility" version="[0.4.0, 0.5.0)">` — o range precisa **conter** a versão que o Plugin Manager anuncia (v3.x → 0.4.0).

## 📐 Regras que respeita

- **GitFlow** (regra global): finalizar a release atualizando `develop` **e** `main`, criar a **tag** e só então publicar — nunca publicar direto de uma release branch.
- **Versão**: fonte da verdade é `.nuspec` / `.csproj`; docs (READMEs + cofre) sempre em sincronia via `build.ps1`. Ver [[🏷️ Versionamento]].
- **Adoção**: manter atualizada a contagem de clones/downloads do repo `zimerfeld/GitExtensions.ZimerfeldTree` (regra global do portfólio).

## 🩹 Troubleshooting

- **Build encerra "sem mudanças"** → `.\build.ps1 -Force`.
- **Passo de deploy pulado** → rodar como Administrador (sem Admin o passo 6 é pulado com aviso; o `.nupkg` ainda é gerado).
- **Plugin Manager não encontra/instala o pacote** → verificar os dois requisitos do nupkg no callout acima (DLL em `lib\` raiz + range da dependency `GitExtensions.Extensibility`).
- **Usuário em GitExtensions 3.x** → incompatível; o plugin requer GitExtensions 4.x (.NET 9).

## 🔗 Ligações

- [[💻 Ambiente Local (Dev)]] — build e instalação local
- [[🔧 build.ps1]] — a nota do arquivo-chave
- [[🏷️ Versionamento]] — esquema de versão, NU5101, arquivos versionados
- [[📘 README — Instalação, Uso e Build]] — opções de instalação (Plugin Manager / script / manual)
- [[🌳 GitExtensions.ZimerfeldTree]] — nota-mãe do projeto
