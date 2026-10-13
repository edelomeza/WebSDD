# AGENTS — WebSDD
> Rector: `./constitution.md` manda ante conflicto. Este dice cómo trabajar. La constitución dice qué no se negocia.

## 1. Forma de trabajo
> Stack: ver constitution.md §3. Nada fuera sin excepción documentada.
Setup: `WebSDD/appsettings.Example.json` → `WebSDD/appsettings.json`; `ApiSettings__BaseUrl` apunta a API externa; `dotnet restore`; `pre-commit install` (hooks format+gitleaks); Docker solo para tests.
Comandos: `dotnet restore`, `dotnet build -c Release --no-restore`, `dotnet test <csproj> -c Release --no-build` (Unit, Integration, Security), `dotnet run --project WebSDD/WebSDD.csproj`, `dotnet publish -c Release -o ./publish`.
CI `ci-pr.yml`: restore → build → unit → integration → security → critic → endpoints → sdd → coverage → delta-check → semgrep. Sin deploy.
PR = 1 cambio; `CHECKLIST_PR.md` + 1 revisor + `Memoria.md` al día.
Naming: inglés código, mensajes UX español. Sin secretos, sin TODO sin issue, sin usings muertos, sin código muerto.

## 2. Memoria
Plantilla canónica `specs/templates/spec-template.md`. DoD por slice en su spec.
`AGENTS.md`: manual operativo vivo.
`Memoria.md`: estado, decisiones, lecciones por fase. Actualizar al cerrar cada fase.
Al empezar, lee `Memoria.md` para conocer estado y decisiones.
Al terminar una tarea, actualízalo: estado, decisiones con porqué y errores a evitar.
Al cerrar una fase, concilia su `spec.md`/`plan.md` con lo implementado y resuelve Aprobación: Borrador con evidencia, o firmado solo si hay revisor.
Breve (máx ~10000 líneas). Si es regla permanente, mover a `AGENTS.md`.
Nunca claves, tokens ni PII. `tests/MemoriaPruebas.md` es normativa para tests.
Entrada por fase según `specs/templates/memoria-entry-template.md`: `## Fase-N … ejecutada — … en verde (fecha)` + `Estado:` métricas + `Decisiones:` densas con porqué + `Deuda:` explícita + `A evitar:` con causa.

## 3. Límites — no hacer
No infraestructura directa desde UI; solo vía servicios.
No estado estático sin `Reset()`.
No hardcodear `BaseUrl`/keys.
No hash débil.
No tocar pipeline sin orden §4 constitución.
No relajar rate en prod.
No config eager en DI con overrides WAF — `IOptionsMonitor` lazy.
No duplicar docs vivos — canónico en `docs/`.
No `catch(Exception)` solo.
No `Invoke-WebRequest` sin `-UseBasicParsing` en PS5.1.
No `env`/`secrets` en `jobs.<id>.if` de Actions (contexto inexistente ahí; el guard va a nivel de step).
Pinear actions con el string exacto del tag (con `v` si la trae) y validarlo en `git/refs/tags/` — `releases/latest` no garantiza tag resolvible. En `0.x`, pin exacto `@v0.x.y` (nunca `@v0`/`@0` flotante: rompe entre minors).
`on:` sin comillas se parsea como booleano `True` en `yaml.safe_load`; validar triggers con `d.get('on', d.get(True))`.

## 4. Verificación por slice
`build` Release 0/0 antes de test.
Unit/Integration/Security `--no-build` verdes.
`critic-guardrails.ps1` → PASS, `check_slnx.ps1` → PASS, `check_endpoints.ps1` → OK, `check_coverage.py` ≥45%, `check_delta_coverage.py --min 80 --advisory` informa delta (no bloquea), `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error` 0 nuevos.
Stryker advisory (`since:main`, `break:0`, `continue-on-error`, reporter `cleartext` — `clear-text` con guion es de StrykerJS) + nightly opcional; tras él siempre `build` antes de `--no-build`.
SonarCloud informativo con guard `env.TOKEN`; Trivy config-scan + Hadolint en base; imagen HIGH/CRITICAL + SBOM CycloneDX + ZAP en CD.
`curl /health/ready` 200, 503 si cae dependencia.

## 5. Convenciones técnicas
Rutas Razor/Pages + MVC donde aplique; validación servidor + FluentValidation donde aplique; token de concurrencia donde aplique.
Health `/health`, `/health/ready`. Sin `health-ui`/`metrics` full.
Claves `IMemory` solo efímeras con TTL. Password nunca en caché.
Sin `health-ui`. Sin OTel/Prometheus/Grafana por ahora.

## 6. Seguridad DevSecOps
Cookie real HttpOnly+Secure+Strict + Sliding, fallback auth, `LoginPolicy` + 429 uniforme + `UseRateLimiter` antes de Auth.
Headers outermost + HSTS 365d no-Dev + CORS 1 origen. CSP con nonce por request.
Timeout 60s + Serilog filtro secretos.
SAST Semgrep bloqueante; Sonar informativo con guard; Dependabot (nuget+actions) activo. Trivy config/imagen + SBOM + ZAP bloquean en CD.

## 7. Testing quirks
xUnit+FluentAssertions+Moq; WAF para Integration/Security.
Reuse `TestDataFactory` común. Separa `reports/` si usas carga futura.
Convención de UI/casos `IsConventional`, no duplicar CDN.

## 8. Observabilidad mínima
Serilog JSON + consola, sin secretos. `CorrelationId` por request.
Sin OTel/Prometheus/Grafana por ahora.

## 9. Operación manual — sin hosting aún
`dotnet publish` listo, no desplegado. Release manual según `docs/runbook-release.md`. PR con `.github/pull_request_template.md` + `CODEOWNERS` (1 revisor). Env `ApiSettings__BaseUrl` + `KeysPath` + verificación post-subida cuando haya hosting. Backup/seed documentados fuera del repo si aplican.
Sin Redis: single-instance documentado. Sin AWS/SQS.
PRs Dependabot: `UNSTABLE` = CI en curso (esperar, no intervenir); tras avanzar `main`, esperar rebase automático; en saltos major exigir tests verdes + vistazo a breaking changes.

## 10. Lecciones
Fuente: `Memoria.md`. Si se vuelve regla, mover a `AGENTS.md`.

## 11. Sub-agentes
`.opencode/agents/` descubrimiento automático. Mínimo privilegio por recurso.
`slice-builder` slice UI→Servicio→Externo + cierre Memoria/checklist; `razor-ui` solo markup/CSS/JS sin C# ni shell; `sec-auditor` solo lectura reporta `ruta:línea`; `test-qa` solo tests + `dotnet test/build` + coverage advisory; `devsecops` calidad/CI/supply-chain/devux config (sin código ni tests); `spec-planner` solo `specs/**,docs/**` sin código.
En CI solo critic ligero bloqueantes 1-4.
