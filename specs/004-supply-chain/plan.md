# PLAN-004 — Supply Chain

## 1. Archivos a tocar/crear
- `.github/dependabot.yml` (nuevo).
- `.github/workflows/ci-pr.yml`: jobs `trivy-config`, `trivy-image`, `sbom`, `hadolint`.
- `WebSDD/Dockerfile`, `compose.yml`: solo lectura salvo hallazgo (nuevo slice si hay que corregir).

## 2. Contratos
- Pins `@vX` a confirmar contra registry en implementación (T-000 spike 15min, registrar en Memoria; principio empírico §2.6).
- SBOM: `actions/upload-artifact@v4`.

## 3. Tests
- Validación: CI verde + artifact SBOM descargable.

## 4. Riesgos
- Falsos positivos Trivy → allowlist documentada en el job, nunca silencio sin causa.
- Tiempo build imagen en cada PR → evaluar cache o mover a `main` si excede (registrar).
