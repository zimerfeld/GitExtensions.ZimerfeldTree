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

Copy-Item $dll $dest -Force
Write-Host ""
Write-Host "DLL atualizada com sucesso em:" -ForegroundColor Green
Write-Host "  $dest" -ForegroundColor Cyan
Write-Host ""
Write-Host "Reinicie o GitExtensions para aplicar."
