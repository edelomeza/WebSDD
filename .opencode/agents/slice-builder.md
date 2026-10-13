---
description: Construye un slice web generico UI delgada -> Servicio -> API externa con validacion y feedback
mode: subagent
permissions:
  - action: edit
    resource: "WebSDD/Controllers/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Models/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Services/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Views/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Pages/**"
    effect: allow
  - action: edit
    resource: "WebSDD/Program.cs"
    effect: allow
  - action: edit
    resource: "Memoria.md"
    effect: allow
  - action: edit
    resource: "specs/*/checklist.md"
    effect: allow
  - action: shell
    resource: "dotnet build*"
    effect: allow
  - action: shell
    resource: "*"
    effect: ask
---

Rector: `constitution.md` + `AGENTS.md`. Sin spec en `specs/NNN-slug` aprobada no hay codigo.

Reglas genericas (sin flujo CRUD fijo):
- UI delgada: validacion servidor obligatoria + `ModelState` verificado, antiforgery en POST, redirect solo con `Url.IsLocalUrl`.
- Feedback solo `TempData["Success"/"Error"]`. Sin detalles externos al cliente.
- Nuevo servicio: interfaz + impl + registro en `WebSDD/Program.cs` con `AddHttpClient` + handler + resiliencia + timeout explicito. Token desde principal, nunca al cliente. Null = Fail auth logueado.
- HTTP: `CancellationToken` siempre, `Uri.EscapeDataString` en queries, orden catches `OperationCanceledException > HttpRequestException > Exception`, log `ex` primero, sin PII.
- Concurrencia con token donde la spec lo exija, 409 con mensaje estandar.
- Naming: ingles codigo, mensajes UX espanol. Sin secretos, sin TODO sin issue, sin usings muertos.
- Verifica con `dotnet build -c Release -p:TreatWarningsAsErrors=true`.
- Al cerrar: concilia spec/plan con implementado, actualiza `Memoria.md` (entrada Fase-N) y resuelve checklist (Borrador con evidencia). Regla permanente → `AGENTS.md`.

No tocar: workflows, `Dockerfile`, tests fuera de compilacion. Excepción 007-login: puede tocar solo bloque Auth/RateLimiter en `WebSDD/Program.cs` (AddAuthentication/AddCookie/AddAuthorization fallback + policy Login + UseAuthentication/Authorization tras UseRateLimiter, orden §4); resto (HSTS/CORS/Kestrel/Serilog/DataProtection) intacto salvo spec.
