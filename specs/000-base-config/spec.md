# SPEC-000 — Base Configuración (WebSDD)

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador con evidencia
- Alcance: scaffolding + configuración. Prohibido agregar flujos.
- Orden: 1º. Depende de: nada (T-000 git habilita CI/firma).

## 1. Objetivo
Dejar el repo compilando, corriendo y con pipeline verde, sin ningún flujo de negocio.

## 2. Fuera de alcance (explícito)
- Login, cookies negocio, JWT server, `ITokenStore`, `[Authorize]` de uso, `ReturnUrl` de uso.
- Servicios de negocio, `OperationResult` de uso, paginado de uso, ViewModels de negocio.
- Vistas `Index/Create/Details/Update/Delete` de negocio, menú, jTable, skeleton de uso, Toastr de uso.
- `ConnectionStrings`, EF, migraciones, `RowVersion` de uso.

## 3. Requisitos
### FR-001 Estructura solución
- Reusar `WebSDD.slnx` + crear `Directory.Build.props` + `.gitignore` + `README.md`.
- Proyecto web `WebSDD/WebSDD.csproj`: `net10.0, Nullable enable, ImplicitUsings` + `Microsoft.Extensions.Http.Resilience`.
- Proyectos test esqueleto: `UnitTest/`, `IntegrationTest/`, `SecurityTest/` con `xUnit + Moq + FluentAssertions + Coverlet`.
- Carpetas web: `Controllers/ Models/ Services/ Views/ Pages/ wwwroot/css|js|lib/ Properties/` con `.gitkeep` donde estén vacías.

### FR-002 `Program.cs` mínimo
- `AddRazorPages + AddControllersWithViews + ResponseCompression + MemoryCache + HttpContextAccessor`.
- `AddHttpClient` base genérico: `BaseAddress=ApiSettings:BaseUrl, Timeout 30s + AddStandardResilienceHandler()`, fail-fast si falta `BaseUrl`.
- `IOptionsMonitor` lazy si hay overrides WAF.
- Middleware: `ExceptionHandler/Hsts → Compression → Https → Static → Routing → RateLimiter global → MapControllerRoute(default) → MapRazorPages → health → RunAsync()`.
- Sin `UseAuthentication/UseAuthorization` de negocio (se añade en hardening/slice). Sin CSP de negocio (solo headers base).

### FR-003 Configuración
- `WebSDD/appsettings.Example.json` + `appsettings.json`: `Logging + AllowedHosts + ApiSettings:BaseUrl` placeholder sin URL real.
- `libman.json`: Bootstrap 5 + jQuery solo instalación, sin uso.
- `Properties/launchSettings.json`: perfiles `https/http`.

### FR-004 Contenedores
- `WebSDD/Dockerfile` multi-stage: `restore → build → publish` solo csproj web, runtime `aspnet:10.0`, expone `8080`.
- `compose.yml`: `webapp + mock-api` con `healthcheck`, env `ApiSettings__BaseUrl`.

### FR-005 Pipeline base
- `.github/workflows/ci-pr.yml`: jobs `build, codeql, sast, sca, secret-scanning` + cache NuGet + guards `env.TOKEN != ''`. Sin ZAP/Trivy bloqueantes en base.

## 4. No-Funcionales
- NFR-001: `dotnet build -c Release -p:TreatWarningsAsErrors=true → 0/0`.
- NFR-002: `NuGetAuditMode all` + analizadores vía `Directory.Build.props`.
- NFR-003: `ILogger<T>` `Information/Warning/Error`, sin PII, `CorrelationId` base.
- NFR-004: `CancellationToken ct=default` en firmas async base.

## 5. Aceptación
- [ ] `dotnet restore + build + test` verde con `Smoke_Pass` por suite.
- [ ] `dotnet run` responde `/` y `/health` 200.
- [ ] `docker compose up webapp` 200 en `/`.
- [ ] Push dispara `ci-pr.yml` verde sin secrets.
- [ ] `grep Login|Authorize|ReturnUrl|TempData|RowVersion|ConnectionStrings` en `WebSDD/` → 0.

## 6. Riesgos
- `BaseUrl` ausente → fail-fast `InvalidOperationException` (registrar en `Memoria.md`).
- IPv6 roto → diferido a fase red, no en base.
- Auth/CSP negocio → diferidos a `001` y slices. No verde falso.

## 7. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-12
Detalle: infra creada y verificada (build 0/0, test 3/3, critic PASS). Evidencia en `specs/000-base-config/checklist.md` + `Memoria.md`.
Reconciliación 2026-10-13 en rama `spec000`: `Program.cs` plantilla plan §2 + `AddHealthChecks` (requerido empírico), `Http.Resilience 10.7.0`, `UseAuthorization` removido, `/`+`/health`+`/health/ready` 200 verificados, `healthcheck` webapp en compose. Docker build/up y CI verde pendientes (daemon apagado / sin push).
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Alineación: sección Riesgos (7 secciones) |
| 1.0.2 | 2026-10-12 |  | Alineación base: T-000 git, orden/dependencias |
