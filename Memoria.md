# Memoria — WebSDD

> Bitácora viva por fase/slice según `specs/templates/memoria-entry-template.md`. Máx ~10000 líneas: resumir lo que no aporte. Sin claves, tokens ni PII. Si algo se vuelve regla permanente, mover a `AGENTS.md`.

## Fase-0 infra base ejecutada — 000 en verde (2026-10-12)
Estado: 3/3 tests (Unit 1 + Integration 1 + Security 1), build 0/0 en 4 proyectos, cobertura advisory (sin reporte en base esqueleto), critic PASS, check_slnx PASS, check_endpoints OK, semgrep pendiente CI (sin CLI local). Specs Borrador con evidencia (000 implementada, 001/002 pendientes).
Decisiones: web genérico sin flujos (constitución v2.5 + AGENTS 11 secciones); sin `ConnectionStrings`, BFF vía `ApiSettings:BaseUrl`; tests `UnitTest/|IntegrationTest/|SecurityTest/` xUnit 2.9.3 + Moq 4.20.72 + FA 7.2.2 (en caché); `EnforceCodeStyleInBuild=false` (IDE0008/0058/0160 chocan con hosting mínimo; CA + Sonar + SCS siguen error); `NoWarn CA1707` solo tests (convención `Smoke_Pass`); `await RunAsync()` (S6966), comentario S1186 en `OnGet` vacíos, pragma S4502 justificado en `Error`; SCS 5.6.7 sin CS8032 con Roslyn 4.14; Sonar 10.35.0.4138; CI `ci-pr.yml` sin deploy, Trivy/ZAP bloquean en CD.
Deuda: `Program.cs` plantilla full §2 (health, compresión, `HttpClient` base) pendiente de slice; Python ausente local → gates coverage solo CI; imagen `wiremock:3` sin pin minor (fijar tras primer pull); Auth/CSP negocio diferidos a `001` y slices.
A evitar: código sin spec NNN aprobada; secretos en `appsettings.json` real o logs; `@Html.Raw` sin justificación XSS; redirects sin `Url.IsLocalUrl`; `catch(Exception)` solo; `WithDocumentPerVersion` y APIs inexistentes (verificar contra SDK antes de usar).

## Fix gitleaks TOML — `.gitleaks.toml` en verde (2026-10-12)
Estado: `gitleaks-action@v2` abortaba con `toml: expected newline but got U+002A '*'` y `results.sarif does not exist` (secundario, sin SARIF generado).
Decisiones: allowlist a basic strings doble-quote (`paths`/`regexes` con `\\` y `\"`); `'` literal no escapa en TOML single-quote y cerraba el regex prematuramente. Verificado con `smol-toml@1.9.1`: sintaxis vieja falla, nueva parsea (3 paths + 1 regex `http://mock-api[^\s"']*`, compila).
A evitar: strings TOML single-quote con `'` o `\` dentro; usar doble-quote con escapes.
