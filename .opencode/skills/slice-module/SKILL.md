---
name: "slice-module"
description: "Guia generica archivo-por-archivo para un slice segun spec, sin CRUD fijo"
license: "MIT"
---
# Slice Module generico

## Estructura segun spec
- `Models/` dominio + VMs por accion definida en spec. `Services/I* + *`. `Controllers|Pages/` + `Views/` segun spec. Menu + DI como modifies. Reusar `OperationResult`, paginado y logger seguro si existen.
- Orden/dependencias del spec (`Orden:`, `Depende de:`) mandan sobre este orden sugerido. Docs vivos solo se enlazan, no se duplican.

## Reglas
- N acciones segun spec, no 8 fijas. `[Authorize]` por defecto, anonimo solo spec.
- VMs con DataAnnotations + mensajes español, `JsonPropertyName` si hay JSON.
- POST con antiforgery + `returnUrl?` + `ViewData` + `IsLocalUrl`.
- Servicio: interfaz N metodos con `ct`, `Bearer` si aplica, 409 solo si hay concurrencia.
- Catalogos: solo lectura si spec los pide, `SelectList + Dict` anti N+1, `pageSize` por spec.

## Checklist
Spec → dominio/VMs → servicio → DI → UI → menu → tests → conciliar spec/plan → `Memoria.md` (entrada Fase-N) → checklist (Borrador con evidencia). Sin `ContractTestData` fijo.
