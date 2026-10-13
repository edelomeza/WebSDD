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
- Notas de alcance 2026-10-13: `Permissions-Policy` incluye además `payment=()`; HSTS sin `Preload` (sin sumisión a lista); `ForwardedHeaders` sin wirear (sin proxy en base).

### FR-002 Observabilidad + calidad
- Serilog JSON + `CorrelationId`, filtro secretos. `.editorconfig` CA3000+ error. S1135.
- `.gitleaks.toml` + hook `protect --staged`. `DataProtection:KeysPath` fuera `wwwroot`.

### FR-003 Docs vivos esqueleto
- `docs/endpoints-ui.md`, `rate-limit-matrix.md`, `asvs-web-checklist.md` esqueleto enlazable.
- Ver `docs/endpoints-ui.md`, `docs/rate-limit-matrix.md`, `docs/asvs-web-checklist.md` (canónicos; esta spec no duplica).

## 4. Aceptación
- [ ] `curl -I` muestra headers + CSP nonce distinto por request.
- [ ] `gitleaks protect --staged` pass. Build 0/0.

## 5. Aprobación y Control de Cambios
Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-12
Detalle: pendiente de implementación. Evidencia en `specs/001-hardening-base/checklist.md` + `Memoria.md`.
Reconciliación 2026-10-13 en rama `spec001`: T-001–T-005 implementados y verificados (headers curl + nonce único, HSTS 365d vía WAF, 429 en umbral test, Serilog JSON + CorrelationId, build 0/0, tests 12/12). Gitleaks/semgrep vía CI.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 1.0.0 | 2026-10-12 |  | Creación inicial |
| 1.0.1 | 2026-10-12 |  | Alineación: orden/dependencias, evaluación previa §3, dedup docs |
