---
tipo: ferramenta
criado: 2026-06-02
atualizado: 2026-06-05 (git-flow-next removido — plugin usa git puro)
tags: [ferramenta, dependencias, instalacao, zimerfeldtree, gitextensions, git, gitflow]
---

# 🧩 Dependências do ZimerfeldTree

> [!abstract] Resumo
> Lista completa de programas e plugins necessários para rodar todas as funcionalidades do plugin **ZimerfeldTree**. Inclui URL de download e procedimento de instalação para cada item.

---

## 1. Git for Windows

| Campo    | Valor |
|----------|-------|
| Papel    | Executa **todos** os comandos git usados pelo plugin (branch, checkout, pull, push, commit, tag, describe, flow…) |
| Obrigatório | ✅ Sim — sem git o plugin não funciona |
| Download | https://git-scm.com/download/win |

### Instalação
1. Baixar o instalador `.exe` (64-bit) e executar.
2. Na tela **"Adjusting your PATH environment"** selecionar **"Git from command line and also from 3rd-party software"** (permite que o GitExtensions chame `git` sem caminho completo).
3. Demais opções: padrão.
4. Verificar: `git --version` no terminal.

---

## 2. GitExtensions

| Campo    | Valor |
|----------|-------|
| Papel    | Aplicação **host** que carrega o plugin via MEF; fornece os diálogos nativos de Commit (`StartCommitDialog`), Push (`StartPushDialog`) e Pull |
| Obrigatório | ✅ Sim — o plugin é uma DLL carregada pelo GitExtensions |
| Versão mínima | 4.x (runtime .NET 9) |
| Download | https://github.com/gitextensions/gitextensions/releases |
| Site oficial | https://gitextensions.github.io/ |

### Instalação
1. Baixar o instalador `.msi` ou `.exe` da última release 4.x.
2. Executar o instalador — ele verifica e instala o **.NET 9 Desktop Runtime** automaticamente se não estiver presente.
3. Após instalar, o executável fica em `C:\Program Files\GitExtensions\GitExtensions.exe`.
4. Verificar: abrir o GitExtensions e ir em **Help → About**.

> [!warning] Versão
> Versões 3.x usam .NET Framework 4.8 e são incompatíveis com o plugin (compilado para `net9.0-windows`).

---

## 3. Plugin ZimerfeldTree

| Campo    | Valor |
|----------|-------|
| Papel    | O plugin em si — DLL carregada pelo GitExtensions na pasta `Plugins\` |
| Obrigatório | ✅ Sim |
| Repositório | https://github.com/zimerfeld/ZimerfeldTree |
| DLL de destino | `C:\Program Files\GitExtensions\Plugins\GitExtensions.Plugins.ZimerfeldTree.dll` |

### Instalação (opção 1 — script automático como Admin)
```powershell
cd C:\GitExtensions\ZimerfeldTree\tools
.\install.ps1
```

### Instalação (opção 2 — manual)
1. Copiar `GitExtensions.Plugins.ZimerfeldTree.dll` para:
   ```
   C:\Program Files\GitExtensions\Plugins\
   ```
2. Reiniciar o GitExtensions.
3. Verificar: menu **Plugins → ZimerfeldTree**.

### Build a partir do fonte
```powershell
# Requer .NET SDK 9 e NuGet CLI (ver itens 5 e 6 abaixo)
# Executar como Administrador para deploy automático
pwsh C:\GitExtensions\ZimerfeldTree\build.ps1
```

---

## 4. .NET SDK 9 *(apenas para build/desenvolvimento)*

| Campo    | Valor |
|----------|-------|
| Papel    | Compilar o projeto `GitExtensions.ZimerfeldTree.csproj` (`net9.0-windows`) |
| Obrigatório | ⚠️ Condicional — apenas para compilar o fonte |
| Download | https://dotnet.microsoft.com/download/dotnet/9.0 |

### Instalação
1. Baixar o instalador do **.NET 9 SDK** (não confundir com Runtime).
2. Executar o instalador.
3. Verificar: `dotnet --version` (deve retornar `9.x.x`).

---

## 5. NuGet CLI *(apenas para build/desenvolvimento)*

| Campo    | Valor |
|----------|-------|
| Papel    | Gerar o pacote `.nupkg` (usado por `build.ps1`) |
| Obrigatório | ⚠️ Condicional — apenas para gerar o pacote NuGet |
| Download | https://www.nuget.org/downloads |

### Instalação
1. Baixar `nuget.exe` (última versão estável).
2. Colocar em uma pasta do `PATH` (ex.: `C:\Program Files\NuGet\`).
3. Verificar: `nuget` no terminal.

---

## Resumo rápido

> [!info] GitFlow sem dependência externa
> Todos os comandos GitFlow (Start, Publish, Track, Update, Finish) usam **git puro**. O binário `git-flow` não precisa estar instalado.

| # | Programa / Plugin     | Obrigatório para uso | Para GitFlow | Para build |
|---|-----------------------|:--------------------:|:------------:|:----------:|
| 1 | Git for Windows       | ✅                   | ✅           | ✅         |
| 2 | GitExtensions 4.x     | ✅                   | ✅           | ✅         |
| 3 | Plugin ZimerfeldTree  | ✅                   | ✅           | —          |
| 4 | .NET SDK 9            | —                    | —            | ✅         |
| 5 | NuGet CLI             | —                    | —            | ✅         |

---

## 🔗 Relacionado
- [[GitExtensions.ZimerfeldTree]]
- [[Interface GitFlow — botões e fluxos]]
- [[Plugin MEF para GitExtensions]]
