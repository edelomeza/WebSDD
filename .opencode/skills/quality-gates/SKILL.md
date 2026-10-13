---
name: "quality-gates"
description: "Como correr y extender gates locales y CI: critic, sdd, coverage, semgrep, stryker, gitleaks"
license: "MIT"
---
# Quality Gates

## Local (requiere SDK; Python solo coverage; gitleaks instalado)
- `dotnet build -c Release -p:TreatWarningsAsErrors=true` → 0/0 antes de test.
- `dotnet test -c Release --no-build` (Unit, Integration, Security).
- `pwsh scripts/critic-guardrails.ps1` → PASS; `check_slnx.ps1` → PASS; `check_endpoints.ps1` → OK; `check_sdd.ps1` → PASS.
- `python3 scripts/check_coverage.py` ≥45% (advisory sin reporte); `check_delta_coverage.py --min 80 --advisory` nunca bloquea.
- `gitleaks protect --staged -c .gitleaks.toml` → pass.
- `pre-commit run --all-files` → hooks format+gitleaks en verde (requiere gitleaks + árbol normalizado; ver Memoria Fase-5).
- Semgrep/Stryker: solo con CLI; en base corren en CI.

## CI (`ci-pr.yml`, ubuntu)
- `shell: pwsh` para `.ps1`; `python3` explícito para `.py`.
- Stryker advisory (`break:0`, `continue-on-error`); tras él siempre `build`.
- Sonar informativo con guard `env.TOKEN`; Trivy HIGH/CRITICAL; SBOM artifact.
- DAST manual: dispatch `dast-manual` en Actions (informativo, nunca bloquea); compose + ZAP baseline.
- PRs Dependabot: esperar rebase/CI; major = tests verdes + breaking changes.

## Extender
- Nuevo gate: script en `scripts/` + job en `ci-pr.yml` + línea en `AGENTS.md` §4 + spec que lo norme. Sin gate sin spec.
