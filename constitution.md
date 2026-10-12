---
version: 2.6.0-web-generic-sdd
status: active
source_of_truth: ./constitution.md
---
# Constitución — WebSDD
> Rector técnico. Manda ante conflicto.
> Web ASP.NET Core 10. Sin DB directa obligatoria.
> Fuera de alcance: AWS/CD, Redis, OTel full, Chaos.

## 1. Propósito y alcance
Web Razor Pages + MVC stateless 1-instancia. Dependencias externas solo vía servicios inyectados. Entregable con `dotnet publish` listo, sin desplegar aún.

## 2. Principios
1. Seguridad primero, fail-closed 400/401/403/404/409/422/429/500.
2. Sin secretos en código/logs/errores.
3. Sin estado oculto: DI compartido; lo estático expone `Reset()` + `lock`.
4. Fail-fast, health real, Detail solo no-prod. Sin tragar excepciones.
5. Real-first: auth/http/hash reales en integración. Prohibido fake.
6. Empírico antes que teórico: medir timeouts/umbrales antes de fijar. Límite en Memoria, prohibido workaround sin issue.

## 3. Stack
| Capa | Tecnología |
|---|---|
| Runtime | .NET 10, Razor Pages + MVC |
| HTTP externo | `HttpClient` + resiliencia estándar + timeout explícito |
| UI | Bootstrap 5, jQuery, Toastr, FA, `site.css/site.js` |
| Tests | xUnit + Moq + FluentAssertions + `Mvc.Testing` |
| Auth | Cookie HttpOnly+Secure+SameSite Strict + Sliding, auth requerida por defecto |
| Logs | Serilog JSON + CorrelationId, filtro secretos |
| Calidad | Analizadores Roslyn + Sonar + CodeQL + Semgrep + NuGetAudit, 0 warnings |
| CI | GitHub Actions solo `ci-pr.yml`, sin deploy |

No nueva librería sin evaluar licencia/mantenimiento/seguridad.

## 4. Arquitectura
Capas: `Páginas/Controladores -> Servicios -> Dependencias externas`. Sin acceso directo a infraestructura desde UI. Registro en `Program.cs` con `HttpClient` + resiliencia cuando haya HTTP. `IOptionsMonitor` lazy si hay overrides WAF. Nada de `new` para servicios clave.
Pipeline fijo: SecurityHeaders outermost→Exception→Forwarded→HSTS no-Dev→Https→CORS 1 origen→RateLimiter antes Auth→Auth→Authorization→Endpoints. Health `/health` + `/health/ready` sin check SQL. Sin `health-ui`/`metrics` full.

## 5. Código y contrato UI
Código inglés, mensajes UX español. Complejidad ≤10 método/≤15 clase. `.editorconfig` CA3000+ error. S1135 caza todo. Validación servidor obligatoria + cliente parcial, antiforgery en POST, `ModelState` verificado, redirect solo con `IsLocalUrl`. `TempData Success/Error` para feedback. Concurrencia con token donde aplique, 409 mensaje estándar. Mapeo errores externos→`ModelState` sin filtrar detalles. Sin `try/catch` ad-hoc en UI.

## 6. Config
`ApiSettings:BaseUrl` si hay API externa, `Cors:AllowedOrigin`, `RequestTimeoutSeconds`, `DataProtection:KeysPath` fuera de `wwwroot`. `appsettings.Example.json` versionado, `appsettings.json` ignorado, `.gitleaks.toml`. `xunit.runner.json` en serie donde haya captura de convención.

## 7. Gobernanza SDD
Specs en `specs/NNN-slug` con plantilla 7 secciones + Aprobación. Sin cambio de contrato UI sin spec. Rama `NNN-slug`. `CHECKLIST_PR.md` obligatorio. 1 revisor. `main` protegida.
Docs vivos en `docs/` (`endpoints-ui.md`, `rate-limit-matrix.md`, `asvs-web-checklist.md`, `runbook-release.md`); las specs solo enlazan, no duplican.

## 8. Gates DevSecOps
Local: `pre-commit install` (format+gitleaks) + `format --verify-no-changes` + build `TreatWarningsAsErrors` 0/0 + tests + `gitleaks protect --staged`.
CI-PR orden: restore → build → unit → integration → security → critic → endpoints → sdd → coverage → delta-check → stryker(advisory) → sonar(informativo, guard) → semgrep, `--no-build` Release. Tokens opcionales con `env.TOKEN + if: env.TOKEN != ''`, `continue-on-error:true` sin `|| true`, pin major `@v1`. YAML validado `yaml.safe_load`. Dependabot (nuget+github-actions) semanal. SBOM CycloneDX como artifact. Trivy config-scan (Dockerfile/compose/workflows) + Hadolint en base.
CD: Trivy imagen HIGH/CRITICAL + ZAP DAST bloquean, SBOM requerido. ZAP baseline manual (`workflow_dispatch`) hasta URL estable.
Headers: CSP con nonce por request (`Context.Items["ScriptNonce"]`), `nosniff,DENY,Referrer-Policy,Permissions-Policy,COEP,COOP`, HSTS 365d solo no-Dev. Kestrel límites + timeout 60s. Login anti-enumeración respuesta idéntica + lockout fail-closed. Password nunca en caché/logs. Allowlist donde aplique.

## 9. Testing y QA
Pirámide Unit > Integration > Security > E2E. Mocks con valores default tipados, sin nulls ambiguos. Externos con `Skip` si requieren red. Cobertura ≥45% `scripts/check_coverage.py`. Puerta incremental advisory (no bloquea): `check_delta_coverage.py --min 80 --advisory` sobre el delta + higiene de generados. Stryker advisory `since:main` (`break:0`, `continue-on-error`); tras él siempre `build` antes de `--no-build`. Semgrep `--error` 0 nuevos.

## 10. CI y release manual
Sin CD. Release manual según `docs/runbook-release.md`: `dotnet publish -c Release -o ./publish` + env `ApiSettings__BaseUrl`, `DataProtection__KeysPath` + `curl /health/ready` + login + 401 + headers + tag `vX.Y.Z-sliceN`. PR con `.github/pull_request_template.md` + `CODEOWNERS` (1 revisor).

## 11. Memoria
`Memoria.md` es bitácora por fase/slice con formato canónico `specs/templates/memoria-entry-template.md`: `## Fase-N <nombre> ejecutada — <specs> en verde (fecha)` + `Estado:` métricas (tests x/y por suite, build 0/0 en N proyectos, cobertura % ≥umbral, critic/endpoints/sdd-lint/semgrep, estado firma specs) + `Decisiones:` con porqué técnico y versiones + `Deuda:` explícita + `A evitar:` con causa. Máx 10000 líneas: resumir lo que no aporte. Sin datos sensibles.
