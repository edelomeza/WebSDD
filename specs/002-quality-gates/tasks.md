# TASKS-002 — Quality Gates (WebSDD)

- [x] T-001 scripts coverage + delta + critic + endpoints + sdd. Acepta: PASS/OK local. (cerrado 2026-10-13 en `spec002`: coverage PASS 89.0% fusionado ≥45% tras fix peor-de-reportes; delta advisory exit 0; critic PASS; slnx PASS; endpoints OK; sdd PASS)
- [x] T-002 `.semgrep/semgrep.yaml + stryker-config.json + xunit.runner.json` válidos. Acepta: `yaml.safe_load`/json parse OK; semgrep 0 nuevos donde haya CLI. (cerrado 2026-10-13: 4 parses OK; sin CLI semgrep local → 0 nuevos pendiente CI)
- [x] T-003 Guard: `ci-pr.yml` intacto en este spec (el cableado de jobs es 003/004). Acepta: diff vacío en workflow. (verificado 2026-10-13: `git diff --name-only` sin `github/workflows`)
