#Requires -Version 5.1
# check_sdd.ps1 → PASS si los specs cumplen gobierno: NNN-slug, plantilla 7 secciones y bloque de aprobación.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$fail = @()
if (-not (Test-Path "$root/specs/templates/spec-template.md")) { $fail += 'Falta specs/templates/spec-template.md' }
$specs = Get-ChildItem "$root/specs" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d{3}-' }
foreach ($s in $specs) {
  $spec = Join-Path $s.FullName 'spec.md'
  if (-not (Test-Path $spec)) { $fail += "$($s.Name) sin spec.md"; continue }
  $t = Get-Content $spec -Raw
  if ($t -notmatch 'Aprobación y Control de Cambios') { $fail += "$($s.Name) sin bloque de aprobación" }
  if ($t -notmatch '(?m)^Estado:') { $fail += "$($s.Name) sin Estado" }
}
if ($fail.Count -gt 0) { $fail | ForEach-Object { Write-Host "FAIL: $_" }; exit 1 }
Write-Host 'PASS check_sdd'
