# PLAN-001 — Hardening Base (WebSDD)

## 1. Archivos a tocar
- `WebSDD/Program.cs`: insertar SecurityHeaders + HSTS + CORS + RateLimiter global antes de Auth (Auth aún passthrough).
- `WebSDD/appsettings*.json`: `Cors:AllowedOrigin`, `RequestTimeoutSeconds`, `DataProtection:KeysPath`.
- `.editorconfig`, `.gitleaks.toml`, `WebSDD/serilog.json` o sección Serilog.
- `docs/*.md` esqueleto.

## 2. Riesgos
- CSP rompe assets locales → allowlist `self` + nonce, verificar `/` carga CSS/JS.
- Rate global agresivo en dev → solo prod estricto, dev laxo documentado.

## 3. Evaluación previa (constitución §3)
- CSP: middleware propio vs paquete — evaluar licencia/mantenimiento/seguridad, registrar decisión en Memoria antes de referenciar (T-001).
- Serilog (`Serilog.AspNetCore`, sinks) y versiones a confirmar contra caché/registry en implementación.
