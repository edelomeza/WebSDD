#Requires -Version 5.1
# check_endpoints.ps1 → OK si docs vivos existen (base: sin app corriendo).
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
foreach ($f in @('docs/endpoints-ui.md', 'docs/rate-limit-matrix.md', 'docs/asvs-web-checklist.md')) {
  if (-not (Test-Path "$root/$f")) { Write-Host "FAIL check_endpoints: falta $f"; exit 1 }
}
Write-Host 'OK check_endpoints (docs vivos presentes; /health se verifica con app corriendo)'
