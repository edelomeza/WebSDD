# SPEC-003 — CI Gates Completos

- Versión: 1.0.2 | Fecha: 2026-10-13 | Estado: Firmado
- Orden: 5º. Depende de: 000, 002.
- Alcance: cablear en `ci-pr.yml` los gates ya normados. Sin negocio.

## 1. Objetivo
`ci-pr.yml` ejecuta critic, endpoints, coverage, delta, Stryker advisory y Sonar informativo además de los 5 jobs base.

## 2. Fuera de alcance
- Trivy/SBOM/Dependabot (004), pre-commit/CODEOWNERS (005), runbook/ZAP manual (006), negocio.

## 3. Requisitos
### FR-001 Jobs critic + endpoints
- `critic` con `shell: pwsh` ejecuta `critic-guardrails.ps1 + check_slnx.ps1` → PASS.
- `endpoints` ejecuta `check_endpoints.ps1` → OK (docs en base; `/health` con app corriendo).

### FR-002 Coverage + delta
- `coverage` ejecuta `python3 scripts/check_coverage.py` (advisory sin reporte en base).
- `delta-check` ejecuta `python3 scripts/check_delta_coverage.py --min 80 --advisory` (nunca bloquea).

### FR-003 Stryker + Sonar
- `stryker` advisory (`since:main`, `break:0`, `continue-on-error:true`, reporter `cleartext`).
- `sonar` informativo con guard `env.SONAR_TOKEN` a nivel de step (no vale en `jobs.<id>.if`); skip documentado sin token.

### FR-004 SDD lint
- Job `sdd` ejecuta `scripts/check_sdd.ps1` → PASS (plantilla 7 secciones + bloque aprobación + NNN).

## 4. Aceptación
- [x] PR de prueba muestra jobs nuevos verdes o skip con causa. (PR #6: 13/13 verde)
- [x] Sin tokens: sonar skip, resto verde. (verificado PR #6; mutación local 54.21% advisory)

## 5. Aprobación y Control de Cambios
Estado: Firmado
Revisores: aprobado por revisor 2026-10-13 (merge PR #5 `spec003` + PR #6 sonar guard + PR #7 stryker reporter)
Fecha: 2026-10-13
Detalle: implementado y conciliado.
Reconciliación 2026-10-13 en rama `spec003`: 7 jobs agregados a `ci-pr.yml` (`critic`, `endpoints`, `sdd`, `coverage`, `delta-check`, `stryker` advisory, `sonar` con guard), `yaml.safe_load` OK. Checks reales pendientes del PR.
Cierre 2026-10-13: guard sonar movido a nivel de step (PR #6, `env` no existe en `jobs.<id>.if`); reporter Stryker `cleartext` (PR #7); PR #6 con 13/13 checks verdes; `yaml.safe_load` OK con 12 jobs.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Job sdd + orden/dependencias |
| 1.0.2 | 2026-10-13 |  | Firma tras revisión: guard sonar a step, reporter cleartext, PR #5/#6/#7 |
