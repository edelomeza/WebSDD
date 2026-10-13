# TASKS-004 — Supply Chain

- [x] T-000 Spike 15min: confirmar pins/tags actions contra registry; registrar en Memoria. Acepta: tabla pin→tag. →Plan §2 (cerrado 2026-10-13 en `spec004`: tabla en `Memoria.md`; existentes se mantienen, nuevos pineados al latest)
- [x] T-001 `.github/dependabot.yml` nuget+actions semanal. Acepta: `yaml.safe_load` OK. →FR-001 (creado 2026-10-13; `safe_load` OK con ambos ecosistemas)
- [x] T-002 Jobs `trivy-config + trivy-image` HIGH/CRITICAL. Acepta: fallan ante fixture vulnerable de prueba. →FR-002 (agregados 2026-10-13: `severity: HIGH,CRITICAL` + `exit-code: 1`; verificación real en CI del PR — `trivy-image` exige daemon, ausente local)
- [x] T-003 SBOM CycloneDX + artifact + Hadolint. Acepta: artifact descargable, Dockerfile sin DL críticos. →FR-002 (agregados 2026-10-13: `anchore/sbom-action` formato `cyclonedx-json` + `upload-artifact`; Hadolint con `failure-threshold: error`; verificación real en CI del PR)

## Verificación
- [ ] CI verde + allowlists documentadas (pendiente PR de `spec004`)
