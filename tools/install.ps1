#Requires -Version 5.1
<#
.SYNOPSIS
    Instala o plugin ZimerfeldTree no GitExtensions.

.DESCRIPTION
    Pode ser executado de duas formas:

      Opção A — standalone (Execute diretamente):
          cd C:\NUGET\ZimerfeldTree\tools
          .\install.ps1

      Opção B — via NuGet Package Manager Console (Visual Studio):
          Install-Package GitExtensions.ZimerfeldTree -Source C:\NUGET
          (O NuGet invoca este script automaticamente passando $installPath, $toolsPath, etc.)

.NOTES
    Requer permissão de Administrador para copiar para
    C:\Program Files\GitExtensions\Plugins\.
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

# ── Locate the plugin DLL ─────────────────────────────────────────────────────

# When run by NuGet PMC, $toolsPath is set to the package's tools\ folder.
# When run standalone, resolve relative to this script's location.
if ($toolsPath) {
    $dllDir = Join-Path $toolsPath "net9.0-windows"
} else {
    $dllDir = Join-Path $PSScriptRoot "net9.0-windows"
}

$dllName = "GitExtensions.Plugins.ZimerfeldTree.dll"
$dll     = Join-Path $dllDir $dllName

if (-not (Test-Path $dll)) {
    Write-Error @"
DLL não encontrada: $dll

Execute build.ps1 primeiro para compilar o plugin:
  pwsh C:\NUGET\ZimerfeldTree\build.ps1
"@
    exit 1
}

# ── Locate GitExtensions Plugins folder ───────────────────────────────────────

$candidates = @(
    "C:\Program Files\GitExtensions\Plugins",
    "C:\Program Files (x86)\GitExtensions\Plugins"
)

$pluginsDir = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $pluginsDir) {
    Write-Warning @"
Pasta de plugins do GitExtensions não encontrada nos caminhos padrão:
  $($candidates -join "`n  ")

Copie manualmente o arquivo:
  $dll
para a pasta Plugins\ da sua instalação do GitExtensions.
"@
    exit 0
}

# ── Check administrator rights ────────────────────────────────────────────────

$isAdmin = ([Security.Principal.WindowsPrincipal]
            [Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Warning @"
Este script precisa de permissão de Administrador para instalar em:
  $pluginsDir

Re-execute o PowerShell como Administrador e repita:
  cd $PSScriptRoot
  .\install.ps1
"@
    exit 1
}

# ── Copy DLL ──────────────────────────────────────────────────────────────────

$dest = Join-Path $pluginsDir $dllName

try {
    Copy-Item -Path $dll -Destination $dest -Force
    Write-Host ""
    Write-Host "✔  Plugin instalado com sucesso em:" -ForegroundColor Green
    Write-Host "   $dest" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Reinicie o GitExtensions e acesse Plugins → ZimerfeldTree."
}
catch {
    Write-Error "Falha ao copiar DLL: $_"
    exit 1
}
