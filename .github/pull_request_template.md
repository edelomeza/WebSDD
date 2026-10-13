# Pull Request — WebSDD

## Resumen
<!-- 1 cambio por PR (AGENTS.md §1). Describe qué cambia y por qué. -->

## Spec del slice
- Spec: `specs/NNN-slug/spec.md` <!-- reemplazar NNN-slug por el slice (p. ej. 005-precommit-devux) -->
- [ ] Spec conciliada con lo implementado

## Checklist
- [ ] `CHECKLIST_PR.md` revisado punto por punto
- [ ] `Memoria.md` actualizada (estado, decisiones, errores a evitar)

## Verificación
<!-- Pega evidencia: build 0/0, tests Unit/Integration/Security, gitleaks, /health/ready -->
- [ ] `dotnet build -c Release -p:TreatWarningsAsErrors=true` 0/0
- [ ] `dotnet test` Unit/Integration/Security verdes
- [ ] `gitleaks protect --staged -c .gitleaks.toml` pass
- [ ] `curl /health/ready` 200

## Firma
- Revisor: <!-- @handle (sustituir placeholder de CODEOWNERS) -->
- [ ] 1 aprobación. `main` protegida.
