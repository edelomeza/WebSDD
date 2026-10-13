# TASKS-003 — CI Gates Completos

- [x] T-001 Jobs `critic + endpoints` (`shell: pwsh`). Acepta: PASS/OK en CI. →FR-001
- [x] T-002 Jobs `coverage + delta-check` (`python3`). Acepta: advisory visible, no bloquea. →FR-002
- [x] T-003 Jobs `stryker` advisory + `sonar` con guard. Acepta: skip documentado sin token. →FR-003
- [x] T-004 Job `sdd` (`scripts/check_sdd.ps1`). Acepta: PASS en CI. →FR-004

## Verificación
- [x] `yaml.safe_load` del workflow OK
- [x] PR de prueba con checks verdes/skip (cerrado 2026-10-13 en `specPendientes`: PR #6 MERGED con 13/13 checks verdes — sonar skip documentado sin token, stryker advisory)
