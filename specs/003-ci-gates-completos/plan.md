# PLAN-003 — CI Gates Completos

## 1. Archivos a tocar
- `.github/workflows/ci-pr.yml`: agregar jobs `critic`, `endpoints`, `sdd`, `coverage`, `delta-check`, `stryker`, `sonar` tras `build` (`needs: build`).
- `scripts/check_sdd.ps1` (creado): validación gobierno SDD en CI.

## 2. Contratos
- `shell: pwsh` para scripts `.ps1` (ubuntu-latest lo incluye).
- `python3` explícito (no `python`) para scripts `.py`.
- Sonar: `env.SONAR_TOKEN` + `if` + secreto `SONAR_TOKEN` ausente = skip.

## 3. Tests
- Validación: push en rama `003-*` y lectura de checks. Sin tests de código.

## 4. Gates
- `yaml.safe_load` del workflow antes del push.

## 5. Riesgos
- Minutos CI: Stryker advisory puede tardar; si excede, mover a nightly (registrar en Memoria).
- Sonar sin proyecto configurado: el job queda esqueleto con skip hasta darlo de alta (deuda).
