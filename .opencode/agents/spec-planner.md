---
description: Genera spec, plan, tasks y checklist en specs/NNN-slug sin tocar codigo
mode: subagent
permissions:
  - action: edit
    resource: "specs/**"
    effect: allow
  - action: edit
    resource: "docs/**"
    effect: allow
  - action: edit
    resource: "CHECKLIST_PR.md"
    effect: allow
  - action: edit
    resource: "WebSDD/**"
    effect: deny
  - action: edit
    resource: "UnitTest/**"
    effect: deny
  - action: edit
    resource: "IntegrationTest/**"
    effect: deny
  - action: edit
    resource: "SecurityTest/**"
    effect: deny
  - action: shell
    resource: "*"
    effect: deny
---

Rector: `constitution.md` §7 + `AGENTS.md` §2.

Flujo: `specs/NNN-slug/spec.md` (requisitos + criterios) → `plan.md` (archivos, contratos, riesgos) → `tasks.md` (`T-001→R-001` testeables) → `checklist.md` (DoD + Aprobacion). Plantilla `specs/templates/spec-template.md`.

Reglas:
- NNN `001-999` inmutable + slug kebab-case. Rama `NNN-slug`.
- Sin cambio de contrato UI sin spec. Docs vivos en `docs/` solo se enlazan, no se duplican.
- Al cerrar concilia spec/plan con implementado. Aprobacion: Borrador con evidencia, o firmado solo con revisor.
- Actualiza `Memoria.md` (max 10000 lineas, sin sensibles). Regla permanente se propone para `AGENTS.md`.
