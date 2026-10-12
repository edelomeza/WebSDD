# SPEC-002 — Quality Gates (WebSDD)

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador con evidencia
- Orden: 4º. Depende de: 000.

## 1. Objetivo
Puertas de calidad no bloqueantes en base, bloqueantes en CD.

## 2. Requisitos
### FR-001 Scripts
- `scripts/check_coverage.py` ≥45%, `check_delta_coverage.py --min 80 --advisory` sobre delta, higiene generados.
- `critic-guardrails.ps1`, `check_slnx.ps1`, `check_endpoints.ps1` PASS/OK.
- `semgrep --config=auto --config=.semgrep/semgrep.yaml --error` 0 nuevos.
- Stryker advisory `since:main break:0 continue-on-error`; tras él siempre `build`.

### FR-002 Configuración (sin cablear CI; el cableado de jobs es 003/004)
- `xunit.runner.json` en serie, `stryker-config.json` advisory, `.semgrep/semgrep.yaml` válidos (`yaml.safe_load` / json parse).
- Trivy/ZAP/SBOM corresponden a 004/006, no a este spec.

## 3. Aceptación
- [ ] Delta advisory informa sin bloquear. Semgrep 0 nuevos. Coverage ≥45%.

## 4. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación. Evidencia en `specs/002-quality-gates/checklist.md` + `Memoria.md`.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Reacote: configs sin cablear CI (cableado en 003/004); orden/dependencias |
