# TASKS-000 — Base Configuración (WebSDD)

## Fase A — Solución
- [ ] T-000 `git init + remote + protección `main``. Acepta: `git remote -v` responde y `main` protegida. Bloquea T-032 (sin remote no hay PR/CI/firma).
- [ ] T-001 `Directory.Build.props + .gitignore + README.md`. Acepta: `dotnet build` encuentra props.
- [ ] T-002 `WebSDD.csproj` net10.0 + `Http.Resilience`. Acepta: restore OK.
- [ ] T-003 Carpetas vacías con `.gitkeep`. Acepta: árbol plan §1 existe.
- [ ] T-004 `specs/templates/spec-template.md + docs/` esqueleto. Acepta: docs vivos enlazables.

## Fase B — Runtime mínimo
- [ ] T-010 `Program.cs` plantilla + `/health`, `/health/ready`. Acepta: `run` responde `/` y `/health` 200.
- [ ] T-011 `appsettings.json + Example + launchSettings + libman.json`. Acepta: `BaseUrl` lee sin throw con compose.
- [ ] T-012 `Pages/Index + Error` mínimos sin auth. Acepta: `/Error` captura 500.

## Fase C — Tests esqueleto
- [ ] T-020 `UnitTest/ + IntegrationTest/ + SecurityTest/` + `Smoke_Pass` c/u. Acepta: `dotnet test` 3/3 verde.

## Fase D — Docker + Pipeline
- [ ] T-030 `Dockerfile` multi-stage. Acepta: `docker build` OK.
- [ ] T-031 `compose.yml`. Acepta: `up webapp` 200 en `/`.
- [ ] T-032 `ci-pr.yml` 5 jobs + cache + guards. Acepta: push rama test verde sin secrets.

## Verificación
- [ ] T-000: remote + `main` protegida (bloquea CI/firma)
- [ ] `dotnet build -c Release -p:TreatWarningsAsErrors=true` → 0/0
- [ ] `dotnet test` → pass
- [ ] `grep -r "Login|Authorize|ReturnUrl|TempData|RowVersion|ConnectionStrings" WebSDD/ --include="*.cs" --include="*.cshtml"` → vacío
