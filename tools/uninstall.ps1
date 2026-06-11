#Requires -Version 5.1
<#
.SYNOPSIS
    Remove o plugin ZimerfeldTree do GitExtensions sem causar danos ao programa.

.DESCRIPTION
    Pode ser executado de duas formas:

      Opção A — standalone:
          cd C:\NUGET\ZimerfeldTree\tools
          .\uninstall.ps1

      Opção B — via NuGet Package Manager Console (Visual Studio):
          Uninstall-Package GitExtensions.ZimerfeldTree
          (O NuGet invoca este script automaticamente.)

.NOTES
    Apenas o DLL do plugin é removido.
    O GitExtensions e seus dados não são afetados.
    Requer permissão de Administrador.
#>

param(
    # Provided automatically by NuGet PMC; ignored when run standalone
    $installPath,
    $toolsPath,
    $package,
    $project
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$dllName = "GitExtensions.Plugins.ZimerfeldTree.dll"

# ── Locate GitExtensions Plugins folder ───────────────────────────────────────

$candidates = @(
    "C:\Program Files\GitExtensions\Plugins",
    "C:\Program Files (x86)\GitExtensions\Plugins"
)

$pluginsDir = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $pluginsDir) {
    Write-Warning "Pasta de plugins do GitExtensions não encontrada. Nada a remover."
    exit 0
}

$target = Join-Path $pluginsDir $dllName

if (-not (Test-Path $target)) {
    Write-Host "Plugin não está instalado em '$pluginsDir'. Nada a remover." -ForegroundColor Yellow
    exit 0
}

# ── Check administrator rights ────────────────────────────────────────────────

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Warning @"
Este script precisa de permissão de Administrador para remover:
  $target

Re-execute o PowerShell como Administrador e repita:
  cd $PSScriptRoot
  .\uninstall.ps1
"@
    exit 1
}

# ── Close GitExtensions if running ────────────────────────────────────────────

$geProcesses = Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue

if ($geProcesses) {
    Write-Host "GitExtensions está aberto. Fechando antes de remover o plugin..." -ForegroundColor Yellow

    # Tenta fechar normalmente (graceful) primeiro
    foreach ($p in $geProcesses) {
        $p.CloseMainWindow() | Out-Null
    }

    # Aguarda até 10s pelo encerramento normal
    try {
        $geProcesses | Wait-Process -Timeout 10 -ErrorAction Stop
    }
    catch {
        Write-Warning "GitExtensions não fechou em 10s. Encerrando à força..."
        Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue |
            Stop-Process -Force -ErrorAction SilentlyContinue
    }

    # Confirma que todos os processos foram encerrados
    Start-Sleep -Milliseconds 500
    if (Get-Process -Name "GitExtensions" -ErrorAction SilentlyContinue) {
        Write-Error "Não foi possível encerrar o GitExtensions. Feche-o manualmente e repita."
        exit 1
    }

    Write-Host "GitExtensions encerrado." -ForegroundColor Green
}

# ── Remove DLL ────────────────────────────────────────────────────────────────

try {
    Remove-Item -Path $target -Force
    Write-Host ""
    Write-Host "✔  Plugin removido com sucesso:" -ForegroundColor Green
    Write-Host "   $target" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "O GitExtensions não foi afetado. Reinicie-o se ainda estiver aberto."
}
catch {
    Write-Error "Falha ao remover o arquivo: $_"
    exit 1
}
