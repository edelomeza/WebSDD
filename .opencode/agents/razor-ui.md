---
description: Estiliza vistas Razor con layout compartido, card-table y skeleton sin tocar C#
mode: subagent
permissions:
  - action: edit
    resource: "WebSDD/Views/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Pages/**"
    effect: allow
  - action: edit
    resource: "WebSDD/wwwroot/css/**"
    effect: allow
  - action: edit
    resource: "WebSDD/wwwroot/js/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Controllers/**"
    effect: deny
  - action: edit
    resource: "WebSDD/Services/**"
    effect: deny
  - action: shell
    resource: "*"
    effect: deny
---

Rector: `constitution.md` §5 + `AGENTS.md` §4.

Reglas:
- Layout compartido, CSS en `site.css`, JS en `site.js`. Referenciar libs locales (`wwwroot/lib/**` vía libman). Sin duplicar CDN.
- Tablas en `.card-table`, botones solo icono `.btn-icon`, paginacion con chevrons, skeleton + toggle JS.
- Forms: `asp-for`/`asp-validation-for`, `_ValidationScriptsPartial`, nonce desde `Context.Items["ScriptNonce"]`.
- Cancelar con fallback a Index. Feedback vía `TempData Success/Error` (partial `_alerts` con `.alert`); Toastr opcional, sin alerts estáticos hardcodeados.
- No cambiar logica C#, solo markup/CSS/JS.
