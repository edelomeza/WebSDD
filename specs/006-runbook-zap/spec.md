# SPEC-006 — Runbook Release y ZAP Manual

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador
- Orden: paralelo desde 000. Depende de: 000.
- Alcance: release manual repetible + DAST manual. Sin CD, sin negocio.

## 1. Objetivo
`docs/runbook-release.md` ejecuta publish+env+curl+tag sin improvisar; `dast-manual.yml` corre ZAP solo con `workflow_dispatch`.

## 2. Fuera de alcance
- CD automático, hosting, negocio.

## 3. Requisitos
### FR-001 Runbook
- Pasos: `publish -c Release -o ./publish` + env `ApiSettings__BaseUrl`, `DataProtection__KeysPath` + `curl /health/ready` + login + 401 + headers + tag `vX.Y.Z-sliceN` + rollback (re-tag anterior; sin hosting: N/A documentado).

### FR-002 DAST manual
- `.github/workflows/dast-manual.yml` solo `workflow_dispatch`: levanta compose y corre ZAP baseline informativo (no bloquea hasta URL estable).

## 4. Aceptación
- [ ] Dry-run hasta publish local + `yaml.safe_load` del workflow.

## 5. Aprobación y Control de Cambios
Estado: Borrador
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
