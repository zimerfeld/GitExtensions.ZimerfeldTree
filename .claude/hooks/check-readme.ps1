#!/usr/bin/env pwsh
# Hook PostToolUse: lembra Claude de atualizar README.md quando .cs do projeto for editado
$raw = [Console]::In.ReadToEnd()
try { $j = $raw | ConvertFrom-Json } catch { exit 0 }

$fp = $j.tool_input.file_path
if (-not $fp) { exit 0 }

# Dispara apenas para arquivos .cs dentro de ZimerfeldTree\src
if ($fp -notmatch 'ZimerfeldTree\\src.*\.cs$') { exit 0 }

@{
    hookSpecificOutput = @{
        hookEventName   = 'PostToolUse'
        additionalContext = "LEMBRETE OBRIGATÓRIO: você acabou de editar '$fp' (fonte do ZimerfeldTree). Atualize C:\NUGET\ZimerfeldTree\README.md para refletir todas as funcionalidades atuais do projeto antes de encerrar a tarefa."
    }
} | ConvertTo-Json -Compress
