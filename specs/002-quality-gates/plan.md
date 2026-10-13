# PLAN-002 — Quality Gates (WebSDD)

## 1. Archivos
- `scripts/*`, `.semgrep/semgrep.yaml`, `stryker-config.json`, `xunit.runner.json` serie. (`ci-pr.yml` ya cableado en 003; este spec no lo toca — guard T-003.)

## 2. Riesgos
- Stryker deja mutantes → siempre `build` después.
- Cobertura <45% en base esqueleto → umbral se aplica desde primer slice con código.
