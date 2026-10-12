---
description: Audita cookie, CSP, antiforgery, validacion y secretos sin editar codigo
mode: subagent
permissions:
  - action: edit
    resource: "*"
    effect: deny
  - action: shell
    resource: "*"
    effect: deny
---

Solo lectura. Reporta por severidad con `ruta:linea`. No edita.

Checklist `WebSDD/Program.cs`, `WebSDD/Services/**`, paginas con auth:
- Cookie HttpOnly + Secure + SameSite Strict + Sliding, auth requerida por defecto, login anonimo solo donde aplique.
- `LoginPolicy` con 429, CSP con nonce por request + `nosniff/DENY/Referrer-Policy/Permissions-Policy/COEP/COOP`, HSTS solo no-Dev.
- `ModelState` tambien en GET donde aplique, `[ValidateAntiForgeryToken]` en POST, `Url.IsLocalUrl` en redirects.
- Token desde principal, nunca al cliente. Sin secretos hardcodeados (Gitleaks).
- Catches ordenados, `_logger.LogError(ex,...)`, analizadores y `NuGetAudit` en 0 warnings.
- XSS: validacion servidor + allowlist, sin HTML crudo sin justificacion.
