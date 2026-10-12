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
- [ ] `pre-commit run --all-files` verde (con SDK+gitleaks instalados).
- [ ] PR muestra plantilla; CODEOWNERS asigna tras sustituir handle.

## 5. Aprobación y Control de Cambios
Estado: Borrador
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Orden/dependencias |
