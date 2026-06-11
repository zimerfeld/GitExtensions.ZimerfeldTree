# Copia o DLL mais recente da pasta de build para os plugins do GitExtensions (requer Admin)
# Uso rapido durante desenvolvimento, sem incrementar versao ou fazer pack.

param([string]$Config = "Release")

$dll = "$PSScriptRoot\..\src\GitExtensions.ZimerfeldTree\bin\$Config\net9.0-windows\GitExtensions.Plugins.ZimerfeldTree.dll"

if (-not (Test-Path $dll)) {
    Write-Warning "DLL nao encontrada: $dll"
    Write-Host "Execute primeiro: dotnet build -c $Config"
    exit 1
}

$dest = "C:\Program Files\GitExtensions\Plugins"
if (-not (Test-Path $dest)) { $dest = "C:\Program Files (x86)\GitExtensions\Plugins" }

if (-not (Test-Path $dest)) {
    Write-Warning "Pasta de plugins nao encontrada."
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

Copy-Item $dll $dest -Force
Write-Host ""
Write-Host "DLL atualizada com sucesso em:" -ForegroundColor Green
Write-Host "  $dest" -ForegroundColor Cyan
Write-Host ""
Write-Host "Reinicie o GitExtensions para aplicar."
