# TASKS-001 — Hardening Base (WebSDD)

- [x] T-001 Headers + CSP nonce + HSTS + CORS. Acepta: curl headers OK + decisión de paquete registrada en Memoria.
- [x] T-002 RateLimiter global + Kestrel/timeout. Acepta: 429 uniforme en umbral test.
- [x] T-003 Serilog + CorrelationId + filtro secretos. Acepta: log JSON sin PII.
- [x] T-004 `.editorconfig` + `.gitleaks.toml` + `KeysPath`. Acepta: build 0/0 + gitleaks pass (gitleaks vía CI, sin CLI local).
- [x] T-005 Verificar `docs/` esqueleto de 000 (sin duplicar). Acepta: specs enlazan.
