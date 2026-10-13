# TASKS-005 — Pre-commit y DevUx

- [x] T-001 `.pre-commit-config.yaml` local (format+gitleaks). Acepta: `run --all-files` verde. →FR-001 (config `repo: local` + `language: system` + `pass_filenames: false` x2, `yaml.safe_load` OK; hooks despachan — `dotnet-format` exit 2 y `gitleaks-protect` exit 1 `Executable not found` — 2026-10-13; verde total condicionado a gitleaks instalado + árbol normalizado, ver Deuda en Memoria)
- [x] T-002 `CODEOWNERS` + nota de sustitución de handle. Acepta: sintaxis válida. →FR-002 (`.github/CODEOWNERS`: `* @REVISOR-PLACEHOLDER` + nota de sustitución, sintaxis validada 2026-10-13)
- [x] T-003 PR template + README Setup. Acepta: PR muestra plantilla. →FR-002 (`.github/pull_request_template.md` con links a spec/`CHECKLIST_PR.md`/`Memoria.md`/firma; `README.md` Setup con `pip install pre-commit` + nota de fallo claro)

## Verificación
- [x] Hook bloquea commit con formato roto (fixture) y lo permite tras `dotnet format` (`UnitTest/Fixture005Format.cs` LF con trailing-whitespace: `whitespace --verify` exit 2 → `dotnet format whitespace` exit 0 → re-verify exit 0; fixture eliminado 2026-10-13)
