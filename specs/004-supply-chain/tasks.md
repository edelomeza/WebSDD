# TASKS-004 — Supply Chain

- [x] T-000 Spike 15min: confirmar pins/tags actions contra registry; registrar en Memoria. Acepta: tabla pin→tag. →Plan §2 (cerrado 2026-10-13 en `spec004`: tabla en `Memoria.md`; existentes se mantienen, nuevos pineados al latest)
- [x] T-001 `.github/dependabot.yml` nuget+actions semanal. Acepta: `yaml.safe_load` OK. →FR-001 (creado 2026-10-13; `safe_load` OK con ambos ecosistemas)
- [x] T-002 Jobs `trivy-config + trivy-image` HIGH/CRITICAL. Acepta: fallan ante fixture vulnerable de prueba. →FR-002 (cerrado con CI 2026-10-13, run `38442293259`: ambos en verde; `trivy-config` falló primero por DS002 real —sin `HEALTHCHECK`—, corregido en Dockerfile, no era fixture)
- [x] T-003 SBOM CycloneDX + artifact + Hadolint. Acepta: artifact descargable, Dockerfile sin DL críticos. →FR-002 (cerrado con CI 2026-10-13: artifact `sbom-cyclonedx` vigente 1670 bytes; Hadolint verde)

## Verificación
- [x] CI verde + allowlists documentadas (PR #13: 18/18 verde; sin allowlists — cero falsos positivos)
