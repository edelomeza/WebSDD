# Plantilla Tasks (`T-ID → R-ID` testeables)

- Spec: `specs/NNN-slug/spec.md` | Plan: `specs/NNN-slug/plan.md`
- Trazabilidad obligatoria: toda tarea cita `→FR-xxx/NFR-xxx` que cubre; sin tareas huérfanas.

## Fase A — (alcance)
- [ ] T-001 Descripción. Acepta: criterio verificable.

## Fase B — (alcance)
- [ ] T-010 Descripción. Acepta: criterio verificable.

## Verificación
- [ ] `dotnet build -c Release -p:TreatWarningsAsErrors=true` → 0/0
- [ ] `dotnet test` Unit/Integration/Security → verdes
- [ ] `gitleaks protect --staged -c .gitleaks.toml` → pass
- [ ] `curl /health/ready` → 200
