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
- [ ] PR Dependabot de prueba (o `dependabot.yml` válido).
- [ ] Trivy/SBOM/Hadolint verdes en CI. Falsos positivos con allowlist documentada.

## 5. Aprobación y Control de Cambios
Estado: Borrador
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
