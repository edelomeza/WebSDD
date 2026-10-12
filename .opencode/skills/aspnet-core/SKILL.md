---
name: "aspnet-core"
description: "ASP.NET Core web generico: UI delgada, servicios HttpClient, auth cookie, validacion"
license: "MIT"
---
# ASP.NET Core generico

## Capas
- `Controllers|Pages/` UI delgada. `Services/I* + *` dependencias externas. `Models/` dominio + VMs por spec.
- Sin acceso directo a infraestructura desde UI. Registro en `Program.cs` con timeout + resiliencia si hay HTTP.
- Paquete nuevo: evaluar licencia/mantenimiento/seguridad antes de referenciar (constitución §3), registrar decisión en Memoria.

## Routing
- Definido por spec. Redirects solo con `IsLocalUrl`, sino Index. `RedirectToPage()` en Pages.

## Auth
- Cookie HttpOnly+Secure+SameSite Strict + Sliding, auth requerida por defecto. Anonimo solo donde spec indique.
- Token externo desde principal, nunca al cliente. Null = Fail logueado.

## Servicios
- `AddHttpClient + handler + resiliencia`. `SendAsync(ct)`, `EscapeDataString`, `ct` siempre.
- Catches: `OperationCanceled > HttpRequest > Exception`, log `ex` primero.
- `IOptionsMonitor` lazy si hay overrides WAF. Nada de `new` para servicios clave.

## Observabilidad
- Serilog JSON + `CorrelationId`, filtro secretos. Health: mapear `/health` + `/health/ready`.

## UI
- `ModelState` verificado, antiforgery en POST, `ViewData ReturnUrl` + `IsLocalUrl`, `TempData Success/Error`.
- Concurrencia solo si spec la exige.

## Pipeline
`Exception/Hsts → Https → Static → Headers/CSP nonce → Routing → RateLimiter → Auth → Authorization → Endpoints` + `/health`, `/health/ready`.
