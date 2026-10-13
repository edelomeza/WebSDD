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
- [x] Dry-run hasta publish local + `yaml.safe_load` del workflow. (publish 0/0 + `WebSDD.dll` existe; `safe_load` OK + trigger dispatch-only + pins validados 2026-10-13; compose/ZAP sin ejecutar: daemon Docker apagado + dispatch manual pendiente — Deuda en Memoria)

## 5. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-13
Detalle: implementado localmente (runbook completo + workflow nuevo). Evidencia en `specs/006-runbook-zap/checklist.md` + `Memoria.md`. Firma pendiente de revisor.
Reconciliación 2026-10-13: `docs/runbook-release.md` esqueleto→procedimiento (7 pasos FR-001, login/401 N/A en base sin ruta de login, condición de bloqueo ZAP en §6); `.github/workflows/dast-manual.yml` nuevo (dispatch-only, compose + espera ready + baseline `fail_action: false`, pin `@v0.14.0` validado en tags — `releases/latest` decía `v0.15.0` sin tag resoluble). Sin cambios de código.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
| 1.0.2 | 2026-10-13 |  | Implementación: runbook completo + dast-manual.yml |
