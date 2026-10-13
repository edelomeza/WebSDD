# TASKS-007 — Login (`T-ID → R-ID` testeables)

- Spec: `specs/007-login/spec.md` | Plan: `specs/007-login/plan.md`
- Trazabilidad obligatoria: toda tarea cita `→FR-xxx/NFR-xxx` que cubre; sin tareas huérfanas.

## Fase A — Base Auth + servicio (orden numérico T-001→T-003, sin dependencias cruzadas)

- [x] T-001 Lib `bi-*` + estilo. `→FR-001`. Acepta: `libman restore` deja `wwwroot/lib/bootstrap-icons/` local, cshtml sin `<link cdn>`; `.btn-login` en `site.css` con `#88E788`, sin `style=""`.
- [x] T-002 Modelos + `IAuthService/AuthService`. `→FR-003,FR-004`. Acepta: `LoginInput{Usuario [Required,StringLength(64)],Password [Required,StringLength(256)],Recordarme}` (solo máximos, sin mínimo UI) + DTOs exactos; `LoginAsync/LogoutAsync` usan `IHttpClientFactory("api")`, retornan `AuthResult{Ok,Invalid,Locked,RateLimited,Network}` (`Locked`→mismo genérico que `Invalid`), sin `strPWD/strToken` en logs.
- [x] T-003 Cookie + fallback + rate en `Program.cs`. `→FR-004,NFR-001`. Acepta: flags `HttpOnly/Secure/Strict`, `Sliding 24h`, `LoginPath=/Login, LogoutPath=/Logout, AccessDeniedPath=/Denegado`, `UseRateLimiter→Auth→Authorization`; `POST /Login` 6º → `429+Retry-After`.

## Fase B — UI + flujo

- [x] T-004 `Pages/Login` GET/POST. `→FR-001,FR-003`. Acepta: `h1 Inicio de sesión`, `input-group bi-person-circle/bi-lock`, `type=password` + toggle `bi-eye`, `Recordarme`, `asp-validation-for` (`StringLength` 64/256), antiforgery; `ModelState` inválido no llama servicio; `Invalid/Locked` mismo mensaje idéntico `Usuario o contraseña no válidos.`; `Network` mensaje red exacto; `Ok→SignIn(min(24h,dteExpira))+LocalRedirect IsLocalUrl`.
- [x] T-005 JS `site.js` loading. `→FR-002`. Acepta: `submit` deshabilita botón + muestra `spinner-border-sm`; eye-toggle alterna `password/text` + icono; sin `<script>` inline sin nonce.
- [x] T-006 Principal `Index` protegida + `Denegado`. `→FR-005`. Acepta: `Index` con `[Authorize]` (sin nueva `Home`), navbar `Bienvenido [x]!!`, footer `Usuario: [x]`, `User.Identity.Name` null-safe, encode verificado con `<b>` inerte; `Pages/Denegado` `[AllowAnonymous]` mensaje 403 genérico.
- [x] T-007 `Pages/Logout` POST (+`OnGet`→302). `→FR-006`. Acepta: `bi-box-arrow-right`, antiforgery, envía `Bearer`, `SignOutAsync` siempre, `302 /Login` + `no-store`; fallo API no 500; `GET /Logout` → `302 /Login`.
- [x] T-008 Docs vivos. `→FR-004,FR-005,NFR-001`. Acepta: `docs/endpoints-ui.md` filas `/Login,/Logout,/Denegado` + auth; `rate-limit-matrix.md` ya cubre Login (enlace); `asvs-web-checklist.md` marca validación/antiforgery/`IsLocalUrl`/no-secretos.

## Fase C — Seguridad + verificación

- [x] T-009 Tests Unit (`UnitTest/AuthServiceTests.cs`). `→FR-003,FR-004`. Acepta: mapeo `Ok/Invalid/Locked(genérico idéntico)/Network/RateLimited`, `min(24h,dteExpira)`, `IsPersistent=Recordarme`.
- [x] T-010 Tests Integration WAF (`IntegrationTest/LoginFlowTests.cs`). `→FR-004,FR-005,FR-006`. Acepta: anon `/Login 200` + `/ 302 Login`; authed `/ 200` con textos; `/Denegado` 200 anónimo; logout borra cookie; `GET /Logout→302 /Login`.
- [x] T-011 Tests Security WAF (`SecurityTest/LoginSecurityTests.cs`). `→NFR-001,NFR-002`. Acepta: antiforgery 400 sin token, `429` login, `returnUrl` externa→`/`, CSP nonce distinto, cookie flags, sin leak `Detail`, sin secretos en logs.
- [x] T-012 Gates + `Memoria.md`. `→NFR-003`. Acepta: DoD checklist (spec/plan conciliados, build 0/0, cobertura ≥45%, semgrep/gitleaks, firma Borrador).

## Verificación

- [x] `dotnet build -c Release -p:TreatWarningsAsErrors=true` → 0/0
- [x] `dotnet test` Unit/Integration/Security `-c Release --no-build` → verdes
- [x] `scripts/check_coverage.py` ≥45% + `delta --advisory` informa
- [x] `semgrep scan --config=auto --config=.semgrep/semgrep.yaml --error` → 0 nuevos
- [x] `gitleaks protect --staged -c .gitleaks.toml` → pass
- [x] `curl /health/ready` → 200 + `curl -I /Login` headers OK
