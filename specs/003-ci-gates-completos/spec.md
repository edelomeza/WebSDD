# SPEC-003 — CI Gates Completos

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador
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
- `stryker` advisory (`since:main`, `break:0`, `continue-on-error:true`).
- `sonar` informativo con `if: env.SONAR_TOKEN != ''`; skip documentado sin token.

### FR-004 SDD lint
- Job `sdd` ejecuta `scripts/check_sdd.ps1` → PASS (plantilla 7 secciones + bloque aprobación + NNN).

## 4. Aceptación
- [ ] PR de prueba muestra jobs nuevos verdes o skip con causa.
- [ ] Sin tokens: sonar skip, resto verde.

## 5. Aprobación y Control de Cambios
Estado: Borrador
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación.
Reconciliación 2026-10-13 en rama `spec003`: 7 jobs agregados a `ci-pr.yml` (`critic`, `endpoints`, `sdd`, `coverage`, `delta-check`, `stryker` advisory, `sonar` con guard), `yaml.safe_load` OK. Checks reales pendientes del PR.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Job sdd + orden/dependencias |
