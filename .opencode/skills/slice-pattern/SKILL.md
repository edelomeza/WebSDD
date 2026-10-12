---
name: "slice-pattern"
description: "Orden, Result pattern y tests genericos para slices"
license: "MIT"
---
# Slice Pattern generico

## Orden
`Models → Services → DI → UI → menu → Unit → Integration → Security` salvo `Orden:` del spec.
Toda tarea cita `→FR-xxx/NFR-xxx`; sin tareas huérfanas.

## Result
`Ok/Fail(message)/Fail(fieldErrors)` con `Success,ErrorMessage,FieldErrors`. UI mapea a `ModelState` + `TempData`.

## Listados
Si hay filtros: cargar catalogos + `SearchAsync` sino `GetAllAsync`. Null = warning + error UI. Paginado/defaults por spec.

## Tests
- Unit: `default(T)`, `_` no usados, asserts `TempData` por spec.
- Integration: fixture HTTP mockeado, `UsingGet/WithBodyAsJson` por endpoint spec.
- Security: auth requerida → redirect, validacion rechaza vacio/XSS con `MemberData` por spec, antiforgery real.
- Comandos: `build -c Release --warnaserror`, `test UnitTest|IntegrationTest|SecurityTest -c Release --no-build --filter ~Nombre`.

## Convenciones
`Async`, `ct`, `TempData`, `returnUrl+IsLocalUrl`. Sin `Str*`, sin `pageSize` fijo, sin `RowVersion` obligatoria.
