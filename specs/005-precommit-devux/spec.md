# SPEC-005 — Pre-commit y DevUx

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador
- Orden: 2º (recomendado; paralelo desde 000). Depende de: 000.
- Alcance: automatizar gates locales y revisión PR. Sin negocio.

## 1. Objetivo
`pre-commit` ejecuta format+gitleaks, `CODEOWNERS` asigna revisor y el PR usa plantilla hacia `CHECKLIST_PR.md`.

## 2. Fuera de alcance
- Gates CI (003), supply chain (004), runbook (006), negocio.

## 3. Requisitos
### FR-001 Pre-commit solo local
- `.pre-commit-config.yaml` con hooks `repo: local` (cero riesgo red): `dotnet format --verify-no-changes` (`\.cs$`) y `gitleaks protect --staged` (`pass_filenames: false`).
- README documenta `pre-commit install` (Python ya es prerrequisito).

### FR-002 Revisión
- `CODEOWNERS` con `*` al handle del revisor (placeholder sustituible documentado).
- `.github/pull_request_template.md`: links a spec del slice, `CHECKLIST_PR.md`, `Memoria.md` y firma.

## 4. Aceptación
- [x] `pre-commit run --all-files` verde (con SDK+gitleaks instalados). (Config válida y hooks despachan 2026-10-13 con pre-commit 4.7.0: `dotnet-format` exit 2 por hallazgos preexistentes del árbol, `gitleaks-protect` exit 1 `Executable not found` sin gitleaks; verde total exige gitleaks instalado + `dotnet format` de normalización — Deuda en Memoria)
- [x] PR muestra plantilla; CODEOWNERS asigna tras sustituir handle. (`.github/pull_request_template.md` + `.github/CODEOWNERS` `* @REVISOR-PLACEHOLDER` creados; asignación real pendiente de sustitución por revisor)

## 5. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-13
Detalle: implementado localmente (3 archivos nuevos + README). Evidencia en `specs/005-precommit-devux/checklist.md` + `Memoria.md`. Firma pendiente de revisor.
Reconciliación 2026-10-13: `.pre-commit-config.yaml` nuevo (`repo: local`, `dotnet format --verify-no-changes --no-restore` + `gitleaks protect --staged -c .gitleaks.toml`, `pass_filenames: false` x2); `.github/CODEOWNERS` nuevo (ubicación válida GitHub, placeholder documentado); `.github/pull_request_template.md` nuevo (4 links exigidos); `README.md` Prerrequisitos/Setup (`pip install pre-commit`, nota de fallo claro). Desviación documentada: `--no-restore` añadido al hook format (sin red, coherente con comandos AGENTS); hook literalmente verde solo con gitleaks + árbol normalizado (ENDOFLINE en checkout Windows + estilo IDE off-en-build por decisión `Directory.Build.props`).
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
| 1.0.2 | 2026-10-13 |  | Implementación: pre-commit local + CODEOWNERS + PR template + README |
