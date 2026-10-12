# CHECKLIST_PR — WebSDD

- [ ] Spec en `specs/NNN-slug` conciliada con implementado
- [ ] `dotnet build -c Release -p:TreatWarningsAsErrors=true` 0/0
- [ ] `dotnet test` Unit/Integration/Security verdes
- [ ] `gitleaks protect --staged` pass
- [ ] `curl /health/ready` 200
- [ ] `Memoria.md` actualizada (estado, decisiones, errores)
- [ ] 1 revisor. `main` protegida.
