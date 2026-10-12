#Requires -Version 5.1
# check_slnx.ps1 → PASS si slnx registra web + 3 suites.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$slnx = Get-Content "$root/WebSDD.slnx" -Raw
$expected = @('WebSDD/WebSDD.csproj', 'UnitTest/UnitTest.csproj', 'IntegrationTest/IntegrationTest.csproj', 'SecurityTest/SecurityTest.csproj')
$missing = $expected | Where-Object { $slnx -notmatch [regex]::Escape($_) }
if ($missing) { Write-Host "FAIL check_slnx: falta $($missing -join ', ')"; exit 1 }
Write-Host 'PASS check_slnx'
