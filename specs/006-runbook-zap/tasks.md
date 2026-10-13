# TASKS-006 — Runbook Release y ZAP Manual

- [x] T-001 `docs/runbook-release.md` completo. Acepta: dry-run hasta publish local. →FR-001 (7 pasos: publish + 2 env + curl ready + login/401 N/A en base + headers + tag + rollback N/A; dry-run `dotnet publish` 0/0 + `WebSDD.dll` existe 2026-10-13)
- [x] T-002 `dast-manual.yml` solo manual + ZAP informativo. Acepta: `yaml.safe_load` OK. →FR-002 (`on` == solo `workflow_dispatch`, sin `push`/`pull_request`; `zaproxy/action-baseline@v0.14.0` validado en tags; `fail_action: false` + `continue-on-error`)

## Verificación
- [x] Enlace desde constitución §10 y AGENTS §9 válido (`constitution.md:48,60`, `AGENTS.md:67`, `README.md:15`; ruta inalterada)
