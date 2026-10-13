# TASKS-000 — Base Configuración (WebSDD)

## Fase A — Solución
- [x] T-000 `git init + remote + protección `main``. Acepta: `git remote -v` responde y `main` protegida. Bloquea T-032 (sin remote no hay PR/CI/firma).
- [x] T-001 `Directory.Build.props + .gitignore + README.md`. Acepta: `dotnet build` encuentra props.
- [x] T-002 `WebSDD.csproj` net10.0 + `Http.Resilience`. Acepta: restore OK.
- [x] T-003 Carpetas vacías con `.gitkeep`. Acepta: árbol plan §1 existe.
- [x] T-004 `specs/templates/spec-template.md + docs/` esqueleto. Acepta: docs vivos enlazables.

## Fase B — Runtime mínimo
- [x] T-010 `Program.cs` plantilla + `/health`, `/health/ready`. Acepta: `run` responde `/` y `/health` 200.
- [x] T-011 `appsettings.json + Example + launchSettings + libman.json`. Acepta: `BaseUrl` lee sin throw con compose.
- [x] T-012 `Pages/Index + Error` mínimos sin auth. Acepta: `/Error` captura 500.

## Fase C — Tests esqueleto
- [x] T-020 `UnitTest/ + IntegrationTest/ + SecurityTest/` + `Smoke_Pass` c/u. Acepta: `dotnet test` 3/3 verde.

## Fase D — Docker + Pipeline
- [ ] T-030 `Dockerfile` multi-stage. Acepta: `docker build` OK. (pendiente: daemon Docker apagado local 2026-10-13)
- [ ] T-031 `compose.yml`. Acepta: `up webapp` 200 en `/`. (pendiente: daemon apagado; `healthcheck` webapp añadido sin verificar)
- [ ] T-032 `ci-pr.yml` 5 jobs + cache + guards. Acepta: push rama test verde sin secrets. (pendiente: push/PR de `spec000`)

## Verificación
- [ ] T-000: remote + `main` protegida (bloquea CI/firma)
- [ ] `dotnet build -c Release -p:TreatWarningsAsErrors=true` → 0/0
- [ ] `dotnet test` → pass
- [ ] `grep -r "Login|Authorize|ReturnUrl|TempData|RowVersion|ConnectionStrings" WebSDD/ --include="*.cs" --include="*.cshtml"` → vacío
