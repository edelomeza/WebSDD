#Requires -Version 5.1
# critic-guardrails.ps1 → PASS si la base no viola reglas no negociables.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$fail = @()
$hits = Get-ChildItem "$root/WebSDD" -Recurse -Include *.cs, *.cshtml -File -ErrorAction SilentlyContinue |
  Select-String -Pattern 'ConnectionStrings' -ErrorAction SilentlyContinue
if ($hits) { $fail += "ConnectionStrings prohibido (constitution §6): $($hits.Count) hallazgos" }
$predictable = @('123456', 'password = "', 'pwd = "')
foreach ($p in $predictable) {
  $h = Get-ChildItem "$root/WebSDD" -Recurse -Include *.cs -File -ErrorAction SilentlyContinue |
    Select-String -Pattern $p -SimpleMatch -ErrorAction SilentlyContinue
  if ($h) { $fail += "Secreto predecible '$p': $($h.Count) hallazgos" }
}
if (-not (Test-Path "$root/WebSDD/appsettings.Example.json")) { $fail += 'Falta appsettings.Example.json' }
if ($fail.Count -gt 0) { $fail | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'PASS critic-guardrails'
