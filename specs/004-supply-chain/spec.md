# SPEC-004 — Supply Chain

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador
- Orden: 6º. Depende de: 000, 003.
- Alcance: dependencias, imagen y SBOM. Sin negocio.

## 1. Objetivo
Dependabot semanal, Trivy config+imagen con fallo en HIGH/CRITICAL, SBOM CycloneDX como artifact y Hadolint al Dockerfile.

## 2. Fuera de alcance
- Gates 003, pre-commit (005), runbook/ZAP (006), negocio.

## 3. Requisitos
### FR-001 Dependabot
- `.github/dependabot.yml`: `nuget` + `github-actions`, `schedule weekly`, labels `dependencies`.

### FR-002 Trivy + Hadolint + SBOM
- Job `trivy-config` (fs + config) y `trivy-image` (build + scan) con fallo en HIGH/CRITICAL.
- SBOM CycloneDX subido como artifact del workflow.
- Hadolint al `Dockerfile` en base.

## 4. Aceptación
- [x] PR Dependabot de prueba (o `dependabot.yml` válido). (`dependabot.yml` válido 2026-10-13; PR de prueba pendiente de primer barrido semanal)
- [x] Trivy/SBOM/Hadolint verdes en CI. Falsos positivos con allowlist documentada. (PR #13, run `38442293259`: 18/18 verde; artifact `sbom-cyclonedx` verificado; sin allowlists)

## 5. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-13
Detalle: implementado en rama `spec004` (jobs + dependabot + spike). Evidencia en `specs/004-supply-chain/checklist.md` + `Memoria.md`.
Reconciliación 2026-10-13: `ci-pr.yml` 12→16 jobs (`trivy-config`, `trivy-image`, `sbom`, `hadolint`, todos `needs: build`); `.github/dependabot.yml` nuevo; `Dockerfile`/`compose.yml` intactos (solo lectura).
Cierre CI 2026-10-13 (PR #13, run `38442293259`): pin trivy `@v0.36.0` (el `@0.36.0` no resolvía); hallazgo real DS002 → `HEALTHCHECK` con curl en `Dockerfile` (plan B `.trivyignore` innecesario); 18/18 verde.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
| 1.0.2 | 2026-10-13 |  | Implementación: 4 jobs + dependabot + spike pins |
| 1.0.3 | 2026-10-13 |  | Cierre CI: pin v0.36.0, HEALTHCHECK DS002, 18/18 verde |
