#Requires -Version 5.1
<#
.SYNOPSIS
    Compila o plugin, incrementa a versao (major.minor.build) e gera o .nupkg.
    Executar como Administrador para tambem fazer o deploy em GitExtensions.
.PARAMETER Force
    Ignora a deteccao de mudancas e sempre incrementa a versao, recompila e empacota.
#>
[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$nuspec  = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree\GitExtensions.ZimerfeldTree.nuspec"
$csproj  = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree\GitExtensions.ZimerfeldTree.csproj"
$outDir  = $PSScriptRoot

# -- 1. Ler versao atual do nuspec ---------------------------------------------
[xml]$spec = Get-Content $nuspec -Encoding UTF8
$current   = $spec.package.metadata.version
$parts     = $current -split '\.'
if ($parts.Count -ne 3) {
    Write-Error "Versao '$current' nao esta no formato major.minor.build"
    exit 1
}
$major      = [int]$parts[0]
$minor      = [int]$parts[1]
$build      = [int]$parts[2] + 1
$newVersion = "$major.$minor.$build"

# -- 1b. Detectar mudancas -----------------------------------------------------
# So' incrementa a versao (e recompila/empacota) se houver fonte OU texto/documento
# empacotado mais novo que o ultimo .nupkg gerado. Sem mudancas => mantem a versao
# atual e encerra. Use -Force para sempre empacotar.
#
# Incluimos TODOS os .md do repositorio e demais textos (LICENSE, scripts de tools\)
# porque a documentacao tambem deve gerar uma nova versao no nuget -- editar qualquer
# arquivo .md (em qualquer pasta) dispara o incremento de versao.
#
# A comparacao e' feita contra o .nupkg (e NAO contra a DLL) de proposito: quando so'
# textos mudam, o build incremental do dotnet pode nao regravar a DLL, o que faria a
# deteccao disparar em loop a cada execucao. O pack, por outro lado, sempre regenera o
# .nupkg, entao ele e' a ancora confiavel do "ultimo empacotamento".
$srcRoot  = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree"
$srcFiles = Get-ChildItem $srcRoot -Recurse -File -Include *.cs,*.csproj,*.nuspec,*.resx,*.png |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
# Todos os .md do repositorio (qualquer pasta) -- editar qualquer documentacao gera
# nova versao. Mais os textos/scripts fixos que tambem entram no pacote (.nuspec <files>).
$mdFiles  = Get-ChildItem $PSScriptRoot -Recurse -File -Filter *.md |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
$otherDocs = @(
    "$PSScriptRoot\LICENSE.txt",
    "$PSScriptRoot\tools\install.ps1",
    "$PSScriptRoot\tools\uninstall.ps1"
) | Where-Object { Test-Path $_ } | ForEach-Object { Get-Item $_ }
$docFiles = @($mdFiles) + @($otherDocs)
$inputFiles = @($srcFiles) + @($docFiles)
$newestSrc  = ($inputFiles | Measure-Object -Property LastWriteTimeUtc -Maximum).Maximum

$lastPkg = Get-ChildItem "$outDir\GitExtensions.ZimerfeldTree.*.nupkg" -ErrorAction SilentlyContinue |
           Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1

if (-not $Force -and $lastPkg -and $newestSrc -le $lastPkg.LastWriteTimeUtc) {
    Write-Host ""
    Write-Host "Nenhuma mudanca detectada em fontes ou textos -- versao mantida em $current (build/pack ignorados)." -ForegroundColor Cyan
    Write-Host "Use -Force para empacotar mesmo assim." -ForegroundColor DarkGray
    exit 0
}

Write-Host "Versao: $current  ->  " -NoNewline
Write-Host $newVersion -ForegroundColor Green

# -- 1c. Fechar GitExtensions e plugins antes de compilar ----------------------
# Feito so' quando ha' mudancas, para nao encerrar o GitExtensions num run sem efeito.
$geProcs = Get-Process -Name GitExtensions -ErrorAction SilentlyContinue
if ($geProcs) {
    Write-Host "Fechando GitExtensions e plugins..."
    $geProcs | Stop-Process -Force
    Start-Sleep -Milliseconds 800
    Write-Host "GitExtensions encerrado."
} else {
    Write-Host "GitExtensions nao esta em execucao."
}

# -- 2. Atualizar nuspec -------------------------------------------------------
$spec.package.metadata.version = $newVersion
$spec.Save($nuspec)

# -- 3. Atualizar csproj -------------------------------------------------------
$csprojContent = Get-Content $csproj -Raw -Encoding UTF8
$csprojContent = $csprojContent -replace '<Version>[^<]+</Version>', "<Version>$newVersion</Version>"
[System.IO.File]::WriteAllText($csproj, $csprojContent, [System.Text.Encoding]::UTF8)

# -- 4. Atualizar link do nuget no README.md -----------------------------------
# Atualiza apenas o conteudo interno (URL do pacote / "Versao atual"); o cabecalho
# de versao+data e' tratado de forma unificada na secao 4b (que tambem emite a
# mensagem "README.md atualizado para ..."), evitando log duplicado.
$readmeDoc = "$PSScriptRoot\README.md"
if (Test-Path $readmeDoc) {
    $content = Get-Content $readmeDoc -Raw -Encoding UTF8
    $content = $content -replace '\*\*Versão atual: [^\*]+\*\*', "**Versão atual: $newVersion**"
    $content = $content -replace 'https://www\.nuget\.org/packages/GitExtensions\.ZimerfeldTree/[\d\.]+', "https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/$newVersion"
    [System.IO.File]::WriteAllText($readmeDoc, $content, [System.Text.Encoding]::UTF8)
}

# -- 4b. Atualizar cabecalho (Versao/Atualizado) no topo dos READMEs -----------
# README.md (cabecalhos EN + PT), README.pt-BR.md e README.en-US.md tem nas primeiras
# linhas o cabecalho de versao e data. Mantemos todos sincronizados com a versao
# empacotada e a data do build. A alternancia cobre os rotulos PT e EN, incluindo o
# "Updated" (sem "on") usado no README.md bilingue.
# Editar qualquer README*.md ja' dispara o incremento de versao (ver secao 1b, que
# inclui todos os .md na deteccao de mudancas), entao aqui apenas carimbamos a nova
# versao/data e registramos uma linha por arquivo no formato:
#   "<arquivo> atualizado para <versao> (<data>)".
$today = (Get-Date).ToString('yyyy-MM-dd')
foreach ($doc in @("$PSScriptRoot\README.md", "$PSScriptRoot\README.pt-BR.md", "$PSScriptRoot\README.en-US.md")) {
    if (Test-Path $doc) {
        $c = Get-Content $doc -Raw -Encoding UTF8
        # Nao ancorado em ^: no README.md raiz o cabecalho e' inline apos "> ![EN](...)"
        # (dois no mesmo arquivo, EN + PT); nos README.en-US/pt-BR fica no inicio da linha.
        # Sem a ancora, um unico replace global cobre os tres formatos. Os rotulos so'
        # aparecem nos cabecalhos (verificado), entao nao ha risco de recarimbar o corpo.
        $c = $c -replace '\*\*(Versão|Version):\*\*\s+[\d\.]+',                       "**`$1:** $newVersion"
        $c = $c -replace '\*\*(Atualizado em|Updated on|Updated):\*\*\s+\d{4}-\d{2}-\d{2}', "**`$1:** $today"
        [System.IO.File]::WriteAllText($doc, $c, [System.Text.Encoding]::UTF8)
        Write-Host "$([System.IO.Path]::GetFileName($doc)) atualizado para $newVersion ($today)"
    }
}

# -- 4c. Carimbar a versao nas notas do cofre Obsidian (o vault espelha o README) ----
# O bump tambem deve refletir no cofre, para o vault nao ficar defasado em relacao ao
# README -- mesma versao em README/csproj/nuspec/vault, sem sync manual. Atualiza o
# 'versao:' do frontmatter e a linha 'Versao atual: **X**' (Versionamento).
#
# Lista somente as notas que carimbam a versao ATUAL do projeto (espelham o README).
# Notas de sessao/historico e notas com versionamento proprio (ex.: "Interface...",
# "Dependencias...") NAO entram aqui de proposito -- o 'atualizado:' delas e' um
# changelog escrito a mao. O laco deixa trivial somar novas notas no futuro.
#
# Roda ANTES do pack (secao 7), entao o .nupkg permanece o arquivo mais novo e a
# deteccao de mudancas (secao 1b) nao dispara em loop. Cada nota atualizada registra
# uma linha no formato: "Obsidian: <arquivo> atualizado para <versao> (<data>)".
$obsidianDocs = @(
    "$PSScriptRoot\OBSIDIAN\01 - Projetos\GitExtensions.ZimerfeldTree.md",
    "$PSScriptRoot\OBSIDIAN\02 - Conhecimento\README — Instalação, Uso e Build.md",
    "$PSScriptRoot\OBSIDIAN\Sistema\Versionamento.md",
    "$PSScriptRoot\OBSIDIAN\Sistema\Visão Geral.md"
)
foreach ($obsDoc in $obsidianDocs) {
    if (Test-Path $obsDoc) {
        $v = Get-Content $obsDoc -Raw -Encoding UTF8
        # Frontmatter -- versao
        $v = $v -replace '(?m)^versao:\s+[\d\.]+',          "versao: $newVersion"
        # Carimba so' a DATA inicial da linha 'atualizado:', preservando o texto descritivo
        # do changelog (ex.: '(1.0.x: ...)') que e' escrito a mao.
        $v = $v -replace '(?m)^atualizado:\s+\d{4}-\d{2}-\d{2}', "atualizado: $today"
        # Corpo -- "Versao atual: **X**" (texto corrido) e "| Versao atual | **X** |" (tabela)
        $v = $v -replace 'Versão atual: \*\*[\d\.]+\*\*',                   "Versão atual: **$newVersion**"
        $v = $v -replace '(\|\s*Versão atual\s*\|\s*)\*\*[\d\.]+\*\*',      ('${1}' + "**$newVersion**")
        [System.IO.File]::WriteAllText($obsDoc, $v, [System.Text.Encoding]::UTF8)
        Write-Host "Obsidian: $([System.IO.Path]::GetFileName($obsDoc)) atualizado para $newVersion ($today)"
    }
}

# -- 5. Build ------------------------------------------------------------------
# Garante que o SDK esteja disponivel antes de tentar compilar, com erro claro.
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet.exe nao encontrado no PATH. Instale o .NET 9 SDK e tente novamente."
    exit 1
}
Write-Host "Compilando..."
$buildOutput = & dotnet build $csproj -c Release --nologo -v minimal 2>&1
$buildExit   = $LASTEXITCODE
$buildOutput | ForEach-Object {
    $line = "$_"
    # Oculta as mensagens dos eventos de prebuild do GitExtensions.Extensibility
    if ($line -match 'GitExtensions\.Extensibility') { return }
    # Colore o resumo do MSBuild: sucesso em verde, avisos em amarelo, erros em vermelho
    if     ($line -match '^\s*Build succeeded\.')  { Write-Host $line -ForegroundColor Green }
    elseif ($line -match '^\s*\d+\s+Warning\(s\)') { Write-Host $line -ForegroundColor Yellow }
    elseif ($line -match '^\s*\d+\s+Error\(s\)')   { Write-Host $line -ForegroundColor Red }
    else { Write-Host $line }
}

# Analisa o resultado do build a partir dos diagnosticos emitidos (formato MSBuild:
# "arquivo(linha,col): error CSxxxx" / "... : warning CSxxxx").
$buildText    = $buildOutput | Out-String
$errorCount   = ([regex]::Matches($buildText, '(?im):\s*error\s')).Count
$warningCount = ([regex]::Matches($buildText, '(?im):\s*warning\s')).Count

if ($buildExit -ne 0 -or $errorCount -gt 0) {
    Write-Host "Build falhou: $errorCount erro(s)." -ForegroundColor Red
    exit 1
}
elseif ($warningCount -gt 0) {
    Write-Host "Build concluido com $warningCount aviso(s)." -ForegroundColor Yellow
}
else {
    Write-Host "Build concluido com sucesso (nenhum erro ou aviso)." -ForegroundColor Green
}

# -- 6. Deploy (requer Admin) --------------------------------------------------
$pluginsDir = "C:\Program Files\GitExtensions\Plugins"
if (-not (Test-Path $pluginsDir)) {
    $pluginsDir = "C:\Program Files (x86)\GitExtensions\Plugins"
}
$dll = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree\bin\Release\net9.0-windows\GitExtensions.Plugins.ZimerfeldTree.dll"

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin -and (Test-Path $pluginsDir)) {
    Copy-Item $dll $pluginsDir -Force
    Write-Host "Plugin instalado em: $pluginsDir"
} else {
    Write-Warning "Sem permissao de Admin ou pasta nao encontrada -- deploy pulado."
    Write-Host "  Copie manualmente: $dll"
    Write-Host "  Para: $pluginsDir"
}

# Atualiza copia na pasta tools (usada pelo nupkg)
$toolsTarget = "$PSScriptRoot\tools\net9.0-windows"
if (-not (Test-Path $toolsTarget)) { New-Item -ItemType Directory $toolsTarget | Out-Null }
Copy-Item $dll $toolsTarget -Force

# -- 7. Pack -------------------------------------------------------------------
Write-Host "Gerando pacote $newVersion..."

# Resolve nuget.exe: PATH -> tools\ local -> download automatico
$nugetCmd = Get-Command nuget -ErrorAction SilentlyContinue
$nugetExe = if ($nugetCmd) { $nugetCmd.Source } else { $null }
if (-not $nugetExe) {
    $nugetExe = Join-Path $PSScriptRoot "tools\nuget.exe"
    if (-not (Test-Path $nugetExe)) {
        Write-Host "nuget.exe nao encontrado - baixando para tools\nuget.exe..."
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" `
                          -OutFile $nugetExe -UseBasicParsing
        Write-Host "Download concluido."
    }
}

# NU5101 (DLL diretamente em lib\) e' INTENCIONAL: o GitExtensions Plugin Manager so'
# extrai o grupo lib cujo framework esta na sua lista de monikers { net5.0..net10.0, any,
# netstandard2.0 }. lib\ raiz = grupo "any" (extraido); uma subpasta net9.0-windows NAO
# esta na lista e quebraria a instalacao. Por isso filtramos esse aviso especifico.
& $nugetExe pack $nuspec -OutputDirectory $outDir 2>&1 |
    Where-Object { $_ -notmatch 'NU5101' } |
    ForEach-Object { Write-Host $_ }
if ($LASTEXITCODE -ne 0) { Write-Error "nuget pack falhou."; exit 1 }

# Remove pacotes de versoes anteriores
Get-ChildItem "$outDir\GitExtensions.ZimerfeldTree.*.nupkg" |
    Where-Object { $_.Name -notlike "*$newVersion*" } |
    Remove-Item -Force

Write-Host ""
Write-Host "Concluido: GitExtensions.ZimerfeldTree.$newVersion.nupkg"
