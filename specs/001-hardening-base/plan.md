# PLAN-001 — Hardening Base (WebSDD)

## 1. Archivos tocados (conciliado 2026-10-13)
- `WebSDD/Program.cs`: SecurityHeaders outermost + CorrelationId + Serilog + HSTS 365d + CORS 1 origen + RateLimiter global antes de Auth (sin `UseAuthentication`: passthrough en base) + timeouts + Kestrel + DataProtection no-Dev + `MapStaticAssets()/.WithStaticAssets()`.
- `WebSDD/Middleware/SecurityHeadersMiddleware.cs` (nuevo): nonce 16B/request + 7 headers.
- `WebSDD/Middleware/CorrelationIdMiddleware.cs` (nuevo): `X-Correlation-Id` + `LogContext`.
- `WebSDD/Logging/SecretFilter.cs` (nuevo): exclusión por nombre/valor.
- `WebSDD/Pages/Shared/_Layout.cshtml`: nonce en importmap.
- `WebSDD/appsettings.Example.json`: secciones `RateLimiting:Global` + `Serilog`.
- `WebSDD/*.csproj`: Serilog x4; `Mvc.Testing 10.0.12` + `ProjectReference` web en tests.
- `.editorconfig` (S1135 error), `compose.yml` (volumen `dpkeys`).
- Tests: `IntegrationTest/HardeningTests.cs`, `UnitTest/SecretFilterTests.cs`.
- `IntegrationTest/HardeningFactory.cs` (fix CI): factoría WAF con `UseSetting` espejo de Example (`ApiSettings:BaseUrl`, `Cors:AllowedOrigin`); `appsettings.json` es gitignorado y no existe en el runner.
- `docs/*.md` esqueleto (notas 001, sin duplicar).

## 2. Riesgos
- CSP rompe assets locales → allowlist `self` + nonce, verificar `/` carga CSS/JS.
- Rate global agresivo en dev → solo prod estricto, dev laxo documentado.

## 3. Evaluación previa (constitución §3)
- CSP: middleware propio vs paquete — evaluar licencia/mantenimiento/seguridad, registrar decisión en Memoria antes de referenciar (T-001).
- Serilog (`Serilog.AspNetCore`, sinks) y versiones a confirmar contra caché/registry en implementación.
- Resultado 2026-10-13 (ver `Memoria.md` Fase-1): CSP con middleware propio (sin paquete externo); Serilog `AspNetCore 10.0.0 + Console 6.1.1 + File 7.0.0 + Formatting.Compact 3.0.0` (caché local, Apache-2.0).
