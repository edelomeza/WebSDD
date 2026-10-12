# SPEC-001 — Hardening Base (WebSDD)

- Versión: 1.0.0 | Fecha: 2026-10-12 | Estado: Borrador con evidencia
- Orden: 3º (recomendado tras 005). Depende de: 000.

## 1. Objetivo
Endurecer base sin negocio: headers, HSTS, CORS, rate global, Serilog, editorconfig, secretos.

## 2. Fuera de alcance
- Login/cookie negocio, JWT, CRUD, jTable de uso, paginado de uso.

## 3. Requisitos
### FR-001 Headers + red
- SecurityHeaders outermost, HSTS 365d solo no-Dev, CORS 1 origen, CSP base con nonce por request (`Context.Items["ScriptNonce"]`), `nosniff,DENY,Referrer-Policy,Permissions-Policy,COEP,COOP`.
- Kestrel límites + timeout 60s. `RateLimiter` global antes de Auth.

### FR-002 Observabilidad + calidad
- Serilog JSON + `CorrelationId`, filtro secretos. `.editorconfig` CA3000+ error. S1135.
- `.gitleaks.toml` + hook `protect --staged`. `DataProtection:KeysPath` fuera `wwwroot`.

### FR-003 Docs vivos esqueleto
- `docs/endpoints-ui.md`, `rate-limit-matrix.md`, `asvs-web-checklist.md` esqueleto enlazable.

## 4. Aceptación
- [ ] `curl -I` muestra headers + CSP nonce distinto por request.
- [ ] `gitleaks protect --staged` pass. Build 0/0.

## 5. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación. Evidencia en `specs/001-hardening-base/checklist.md` + `Memoria.md`.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Alineación: orden/dependencias, evaluación previa §3, dedup docs |
