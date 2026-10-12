# PLAN-006 — Runbook Release y ZAP Manual

## 1. Archivos a crear
- `docs/runbook-release.md` (pasos + verificación + rollback).
- `.github/workflows/dast-manual.yml` (`workflow_dispatch`, compose + ZAP baseline informativo).

## 2. Riesgos
- ZAP sin URL estable → informativo hasta endurecer (registrar condición de bloqueo futuro).
- Secretos en logs del workflow → `mask` + sin PII (fail-closed §2.2).
