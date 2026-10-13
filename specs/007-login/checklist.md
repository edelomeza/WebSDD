# Checklist 007 — Login y flujo navegación

## DoD

- [x] `spec.md`/`plan.md` conciliados con implementado (rutas, criterios, archivos reales)
- [x] Build 0/0 + tests verdes + `/health/ready` 200 (CI verde pendiente del PR)
- [x] `critic-guardrails.ps1` PASS, `check_slnx` PASS, `check_endpoints` OK, `sdd` PASS, cobertura 90.6% ≥45% (semgrep/gitleaks sin CLI local, vía CI)
- [x] `docs/endpoints-ui.md`, `rate-limit-matrix.md`, `asvs-web-checklist.md` enlazados (sin duplicar)
- [x] `Memoria.md` actualizada (estado, decisiones con porqué, errores)
- [x] Sin secretos/TODO sin issue/usings muertos/código muerto
- [x] Sin `try/catch` ad-hoc en UI, sin `Session.Clear()`, sin hardcode `BaseUrl`, sin CDN duplicado

## Aprobación

- [x] Borrador con evidencia
- [ ] Firmado solo con revisor + `CHECKLIST_PR.md`
