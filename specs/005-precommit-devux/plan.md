# PLAN-005 — Pre-commit y DevUx

## 1. Archivos a crear
- `.pre-commit-config.yaml`, `CODEOWNERS`, `.github/pull_request_template.md`.
- README: `pre-commit install` en Setup (ya exige Python como prerrequisito).

## 2. Riesgos
- `gitleaks` binario ausente local → documentar instalación; el hook falla con mensaje claro, no silencioso.
- Hooks `local` no auto-instalan nada: cero sorpresas de red, a cambio requieren SDK+gitleaks presentes.
