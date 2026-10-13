# SPEC-007 — Login y flujo navegación (WebSDD)

- Versión: 0.2.0 | Fecha: 2026-10-13 | Estado: Borrador con evidencia | Rama: `007-login`
- Orden: tras 006. Depende de: 001 (headers/CSP/rate base, Serilog, DataProtection).

## 1. Contexto

Base endurecida 001–006 en verde; `Program.cs` sin Auth (passthrough). API externa `APISDD v1` expone `POST /api/v1/Auth/login {strNombreUsuario,strPWD} → {strToken,dteExpira}` y `POST /api/v1/Auth/logout` (Bearer global). Docs vivos: `docs/endpoints-ui.md` sin `/Login`, `docs/rate-limit-matrix.md` ya prevé `Login 5/5min`, `docs/asvs-web-checklist.md` exige validación+antiforgery+`IsLocalUrl`. CSP `style-src 'self'` + `script-src nonce` bloquea inline sin nonce.

## 2. Objetivo

Login Razor Pages contra API externa con cookie `HttpOnly+Secure+Strict+Sliding 24h`, redirección a principal con `Bienvenido [x]!!` / `Usuario: [x]`, y logout POST con limpieza + retorno a `/Login`.

## 3. Alcance / Fuera de alcance

Incluye: `/Login`, `/Logout`, protección raíz/principal por defecto, `AuthService`, cookie 24h + `Recordarme`, eye-toggle + anti-doble-submit, mapeo errores a `ModelState`/`TempData`, rate login, iconos `bi-*` vía libman.
Fuera: CRUD `SDDUsuarios`, JWT propio, `Session state`, Redis/OTel, deploy (solo `dotnet publish` listo).

## 4. Requisitos (trazables a test y código)

### FR-001 UI Login (`→ T-001,T-004`)
- `h1.fw-bold.text-center.mb-4` texto exacto `Inicio de sesión`; layout centrado `d-flex align-items-center justify-content-center`.
- `label Usuario → Input.Usuario`, `input-group` prefijo `bi-person-circle`; `label Contraseña → Input.Password type=password`, prefijo `bi-lock`, botón sufijo `bi-eye/bi-eye-slash` alterna `password/text` desde `site.js` (sin inline sin nonce).
- Color primario `#88E788` solo como clase en `site.css` (ej. `.btn-login`), nunca `style=""` (CSP). Contraste verificado.
- Checkbox `Recordarme` (`IsPersistent` solo si marcado).
- Iconos: `bootstrap-icons@1.11.3` vía `libman.json` (cdnjs, pin exacto, copia local, licencia MIT evaluada per `constitution.md §3`); prohibido CDN duplicado.

### FR-002 Anti-doble + spinner (`→ T-005`)
- `site.js`: al `submit`, `button.disabled=true`, oculta texto, muestra `div.spinner-border.spinner-border-sm[role=status]` dentro del botón.

### FR-003 Validación y errores (`→ T-002,T-004,T-009`)
- Servidor obligatorio: `LoginInput.Usuario [Required, StringLength(64)]`, `LoginInput.Password [Required, StringLength(256)]` (solo máximos anti-abuso; sin mínimo UI para no rechazar credenciales válidas) + `asp-validation-for` + cliente parcial (`jquery-validation` existente); `ModelState` verificado; antiforgery en POST; `redirect` solo `IsLocalUrl`.
- Sin `try/catch` ad-hoc en UI (`constitution.md §5`): `PageModel → IAuthService → HttpClient "api"`; servicio retorna `Result`; PageModel mapea a `ModelState`/`TempData Error` sin filtrar detalles (`Detail` solo no-prod).
- Red/timeout (`HttpRequestException/TaskCanceledException` en servicio): mensaje genérico `No logramos conectarnos. Por favor, verifica tu conexión a internet o inténtalo más tarde.`
- Credencial inválida, cuenta bloqueada y fallo genérico: respuesta idéntica anti-enumeración (mismo mensaje `Usuario o contraseña no válidos.`) + `429` uniforme en umbral; lockout fail-closed (detalles de bloqueo solo en servicio, nunca al cliente).

### FR-004 Sesión cookie 24h (`→ T-002,T-003,T-010`)
- `AddAuthentication(Cookie).AddCookie`: `HttpOnly`, `SecurePolicy.Always`, `SameSite.Strict`, `SlidingExpiration=true`, `ExpireTimeSpan=24h`, `LoginPath=/Login`, `LogoutPath=/Logout`, `AccessDeniedPath=/Denegado`; `AddAuthorization` fallback requiere auth; `UseRateLimiter` antes `UseAuthentication/UseAuthorization`.
- Claims: `ClaimTypes.Name=strNombreUsuario` + claim privada `access_token=strToken`; expiración efectiva `min(cookie 24h, dteExpira)`; password nunca en caché/logs (`SecretFilter`).
- Raíz `/` con auth requerida redirige a principal sin parpadeo si cookie vigente; anónimo solo `/Login`, `/Denegado`, `/health*`, `/Error` (`GET /Logout` redirige a `/Login`).

### FR-005 Principal interna (`→ T-006,T-010`)
- Recupera `User.Identity.Name` (nullable-safe). Navbar `navbar fixed-top`: texto exacto `Bienvenido [nombre]!!`. Footer izquierda: `Usuario: [nombre]`. Encode Razor por defecto, sin `Html.Raw`.

### FR-006 Logout (`→ T-007,T-010`)
- Botón/enlace `Cerrar Sesión` + `bi-box-arrow-right`, formulario `POST` antiforgery.
- Servidor: `POST /api/v1/Auth/logout` con `Bearer strToken`, luego `SignOutAsync` siempre (aunque falle API, sin 500); sin `Session.Clear()` (no hay `AddSession`, stateless).
- Salida: `RedirectToPage("/Login")` + `ResponseCache(NoStore)` / `Cache-Control: no-store` anti-back.

### NFR-001 Seguridad (`→ T-003,T-011`)
Headers outermost, HSTS 365d no-Dev, CORS 1 origen, CSP nonce intactos; Kestrel+`RequestTimeoutSeconds` 60s, `HttpClient api` 30s; rate `Login 5/5min` + global 1000/min/IP.

### NFR-002 Observabilidad (`→ T-011`)
Serilog JSON + `CorrelationId`, filtro secretos; sin `strPWD/strToken` en logs.

### NFR-003 Calidad (`→ T-012`)
`TreatWarningsAsErrors` 0/0, cobertura ≥45%, `IsConventional` sin CDN duplicado.

## 5. Contratos (rutas, config, env)

| Ruta | Auth | Notas |
|---|---|---|
| `/Login` GET/POST | anónima | `LoginModel(Input.Usuario,Input.Password,Recordarme)`; `GET /Logout` → `302 /Login` (sin vista) |
| `/Logout` POST | auth | antiforgery, `no-store` |
| `/Denegado` GET | anónima | mensaje 403 genérico (destino `AccessDeniedPath`) |
| `/` (Index/principal) | auth fallback | `Bienvenido` + `Usuario:` (se reutiliza `Index` existente) |
| `/health`, `/health/ready` | no | exentas rate-limit |
| Ver canónicos | — | `docs/endpoints-ui.md`, `docs/rate-limit-matrix.md`, `docs/asvs-web-checklist.md` (solo enlazan) |

Config: `ApiSettings:BaseUrl` (sin hardcodear `https://localhost:7229/`), `Cors:AllowedOrigin`, `RequestTimeoutSeconds=60`, `RateLimiting:Login 5/5min`, `DataProtection:KeysPath` fuera `wwwroot`. DTOs exactos: `LoginRequestDto{strNombreUsuario,strPWD}`, `LoginResponseDto{strToken,dteExpira}`.

## 6. Criterios de aceptación verificables

- [ ] `GET /Login` 200 anónimo con `h1 Inicio de sesión`, `bi-person-circle/bi-lock`, `type=password`, `__RequestVerificationToken` presente.
- [ ] Credencial válida (mock/WAF): `302` a `/`, cookie `HttpOnly; Secure; SameSite=Strict`, `Expires ≈24h`; `GET /` muestra `Bienvenido [x]!!` y `Usuario: [x]`; `GET /Login` estando autenticado redirige a `/`.
- [ ] Credencial inválida: mismo mensaje genérico, sin enumeración, `ModelState` con `alert-danger`/`TempData Error`.
- [ ] API caída/timeout: mensaje red exacto, sin 500.
- [ ] 6 POST rápidos a `/Login`: `429` + `Retry-After`.
- [ ] `POST /Logout`: llama API con `Bearer`, borra cookie, `302 /Login`, `Cache-Control: no-store`; atrás no reentra sin login.
- [ ] `GET /Logout` → `302 /Login` (no expone formulario por GET).
- [ ] `?returnUrl=https://evil.com` → redirige a `/` (open-redirect bloqueado por `IsLocalUrl`).
- [ ] `GET /Denegado` anónimo 200 con mensaje genérico; `curl -I` headers + CSP `nonce` distinto por request; `/health/ready` 200.

## 7. Riesgos y deudas documentadas

- `dteExpira` API <24h: se usa `min()`; documentar en `plan.md`.
- Contraste `#88E788` sobre blanco puede fallar AA: verificar y oscurecer texto/borde si no pasa.
- `bootstrap-icons` nuevo: si revisor rechaza, fallback a FA (mismo patrón libman).
- ZAP/Docker pendientes de base: revalidar tras slice.

## Aprobación y Control de Cambios

Estado: Borrador con evidencia
Revisores:
Fecha: 2026-10-13
Detalle: implementado y conciliado (12/12 tasks; build 0/0, tests 47/47, cobertura 90.6%, gates locales PASS, vivo verificado con curl). Evidencia en `specs/007-login/checklist.md` + `Memoria.md` Fase-7. Firma pendiente de revisor.
| Versión | Fecha | Autor | Cambio |
|---|---|---|---|
| 0.1.0 | 2026-10-13 | | Creación ajustada a constitución/AGENTS/docs + OpenAPI Auth |
| 0.1.1 | 2026-10-13 | | Ajuste análisis tasks: NFR-003→T-012, StringLength 64/256, Locked genérico, /Denegado, GET /Logout→302, caso open-redirect |
| 0.2.0 | 2026-10-13 | | Implementación conciliada: sin deriva (extras documentados: Privacy/Error/Denegado AllowAnonymous, `SecurityTest` +WAF, `HardeningTests` a /Login) |
