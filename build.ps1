#Requires -Version 5.1
<#
.SYNOPSIS
    Compila o plugin, incrementa a versao (major.minor.build) e gera o .nupkg.
    Executar como Administrador para tambem fazer o deploy em GitExtensions.
#>

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
# So' incrementa a versao (e recompila/empacota) se houver fonte mais novo que a
# DLL ja' compilada. Sem mudancas => mantem a versao atual e encerra.
$dll     = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree\bin\Release\net9.0-windows\GitExtensions.Plugins.ZimerfeldTree.dll"
$srcRoot = "$PSScriptRoot\src\GitExtensions.ZimerfeldTree"
$srcFiles = Get-ChildItem $srcRoot -Recurse -File -Include *.cs,*.csproj,*.nuspec,*.resx,*.png |
            Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
$newestSrc = ($srcFiles | Measure-Object -Property LastWriteTimeUtc -Maximum).Maximum

if ((Test-Path $dll) -and $newestSrc -le (Get-Item $dll).LastWriteTimeUtc) {
    Write-Host ""
    Write-Host "Nenhuma mudanca detectada nos fontes -- versao mantida em $current (build/pack ignorados)." -ForegroundColor Cyan
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

# -- 4. Atualizar FUNCIONALIDADES.md -------------------------------------------
$funcDoc = "$PSScriptRoot\FUNCIONALIDADES.md"
if (Test-Path $funcDoc) {
    $today   = (Get-Date).ToString("yyyy-MM-dd")
    $content = Get-Content $funcDoc -Raw -Encoding UTF8
    $content = $content -replace '\*\*Versão:\*\* [^\r\n]+', "**Versão:** $newVersion"
    $content = $content -replace '\*\*Atualizado em:\*\* [^\r\n]+', "**Atualizado em:** $today"
    [System.IO.File]::WriteAllText($funcDoc, $content, [System.Text.Encoding]::UTF8)
    Write-Host "FUNCIONALIDADES.md atualizado para $newVersion ($today)"
}

# -- 4b. Atualizar README.md --------------------------------------------------
$readmeDoc = "$PSScriptRoot\README.md"
if (Test-Path $readmeDoc) {
    $content = Get-Content $readmeDoc -Raw -Encoding UTF8
    $content = $content -replace '\*\*Versão atual: [^\*]+\*\*', "**Versão atual: $newVersion**"
    $content = $content -replace 'https://www\.nuget\.org/packages/GitExtensions\.ZimerfeldTree/[\d\.]+', "https://www.nuget.org/packages/GitExtensions.ZimerfeldTree/$newVersion"
    [System.IO.File]::WriteAllText($readmeDoc, $content, [System.Text.Encoding]::UTF8)
    Write-Host "README.md atualizado para $newVersion"
}

# -- 5. Build ------------------------------------------------------------------
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
