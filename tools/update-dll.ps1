#Requires -Version 5.1
<#
.SYNOPSIS
    Atualiza a DLL do plugin no GitExtensions.

.DESCRIPTION
    Atalho de desenvolvimento: valida a saida em bin\<Config>, chama build.ps1
    -Force quando ela estiver ausente ou defasada, fecha o GitExtensions se
    necessario e copia a DLL final para a pasta Plugins.
#>

param(
    [string]$Config = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dllName     = "GitExtensions.Plugins.ZimerfeldTree.dll"
$repoRoot    = Resolve-Path (Join-Path $PSScriptRoot "..")
$buildScript = Join-Path $repoRoot "build.ps1"
$projectRoot = Join-Path $repoRoot "src\GitExtensions.ZimerfeldTree"
$buildDll    = Join-Path $projectRoot "bin\$Config\net9.0-windows\$dllName"
$toolsDll    = Join-Path $PSScriptRoot "net9.0-windows\$dllName"

if (-not (Test-Path $buildScript)) {
    Write-Error "build.ps1 nao encontrado: $buildScript"
    exit 1
}

$currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal   = New-Object Security.Principal.WindowsPrincipal($currentUser)
$isAdmin     = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Warning "Este script precisa de permissao de Administrador para copiar para Program Files."
    Write-Host "Re-execute o PowerShell como Administrador e rode:"
    Write-Host "  cd $repoRoot"
    Write-Host "  .\tools\update-dll.ps1"
    exit 1
}

$pluginCandidates = @(
    "C:\Program Files\GitExtensions\Plugins",
    "C:\Program Files (x86)\GitExtensions\Plugins"
)
$pluginsDir = $pluginCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $pluginsDir) {
    Write-Warning "Pasta de plugins do GitExtensions nao encontrada."
    Write-Host "Caminhos verificados:"
    $pluginCandidates | ForEach-Object { Write-Host "  $_" }
    exit 1
}

$shouldBuild = $false
$buildReason = $null

if (-not (Test-Path $buildDll)) {
    $shouldBuild = $true
    $buildReason = "DLL de build nao encontrada em $buildDll"
}
elseif ((Test-Path $toolsDll) -and ((Get-Item $buildDll).LastWriteTimeUtc -lt (Get-Item $toolsDll).LastWriteTimeUtc)) {
    $shouldBuild = $true
    $buildReason = "DLL de build mais antiga que a DLL em tools\net9.0-windows"
}
else {
    $sourceFiles = Get-ChildItem $projectRoot -Recurse -File -Include *.cs,*.csproj,*.resx,*.png |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    $newestSource = ($sourceFiles | Measure-Object -Property LastWriteTimeUtc -Maximum).Maximum

    if ($newestSource -and $newestSource -gt (Get-Item $buildDll).LastWriteTimeUtc) {
        $shouldBuild = $true
        $buildReason = "fontes ou recursos do projeto mais novos que a DLL de build"
    }
}

if ($shouldBuild) {
    Write-Host "Build necessario: $buildReason" -ForegroundColor Yellow
    Write-Host "Executando: $buildScript -Force"
    & $buildScript -Force

    if (-not $?) {
        Write-Error "build.ps1 -Force falhou."
        exit 1
    }
    if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        Write-Error "build.ps1 -Force falhou com codigo $LASTEXITCODE."
        exit $LASTEXITCODE
    }
}

if (-not (Test-Path $buildDll)) {
    Write-Error "DLL nao encontrada apos o build: $buildDll"
    exit 1
}

# -- Fecha o GitExtensions se estiver aberto (ele bloqueia o DLL do plugin) -----
$geProcesses = Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue
if ($geProcesses) {
    Write-Host "GitExtensions esta aberto. Fechando antes de continuar..." -ForegroundColor Yellow
    foreach ($p in $geProcesses) { $p.CloseMainWindow() | Out-Null }
    try {
        $geProcesses | Wait-Process -Timeout 10 -ErrorAction Stop
    }
    catch {
        Write-Warning "GitExtensions nao fechou em 10s. Encerrando a forca..."
        Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue |
            Stop-Process -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 500
    if (Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue) {
        Write-Error "Nao foi possivel encerrar o GitExtensions. Feche-o manualmente e repita."
        exit 1
    }
    Write-Host "GitExtensions encerrado." -ForegroundColor Green
}

$dest = Join-Path $pluginsDir $dllName
Copy-Item -Path $buildDll -Destination $dest -Force

$sourceHash = (Get-FileHash $buildDll -Algorithm SHA256).Hash
$destHash   = (Get-FileHash $dest -Algorithm SHA256).Hash

if ($sourceHash -ne $destHash) {
    Write-Error "A copia terminou, mas o hash da DLL instalada nao confere."
    exit 1
}

Write-Host ""
Write-Host "DLL atualizada com sucesso em:" -ForegroundColor Green
Write-Host "  $dest" -ForegroundColor Cyan
Write-Host ""
Write-Host "Origem:"
Write-Host "  $buildDll"
Write-Host ""
Write-Host "Reinicie o GitExtensions para aplicar."
