# PLAN-007 — Login (CÓMO)

- Spec: `specs/007-login/spec.md` | Rama: `007-login`

## 1. Árbol objetivo (archivos a crear/modificar)

Crear:
- `WebSDD/Pages/Login.cshtml`, `Login.cshtml.cs` (`LoginModel`)
- `WebSDD/Pages/Logout.cshtml`, `Logout.cshtml.cs` (POST + `OnGet` → `302 /Login`)
- `WebSDD/Pages/Denegado.cshtml`, `Denegado.cshtml.cs` (`[AllowAnonymous]`, mensaje 403 genérico)
- `WebSDD/Services/IAuthService.cs`, `AuthService.cs` (+ `AuthResult`)
- `WebSDD/Models/LoginInput.cs`, `WebSDD/Models/LoginDtos.cs` (`LoginRequestDto/ResponseDto`; `Usuario StringLength(64)`, `Password StringLength(256)`)
- `tests`: `UnitTest/AuthServiceTests.cs`, `IntegrationTest/LoginFlowTests.cs`, `SecurityTest/LoginSecurityTests.cs`

Modificar:
- `WebSDD/Program.cs` (cookie auth + fallback + rate login + `IAuthService`)
- `WebSDD/Pages/Index.cshtml(.cs)` protegida con `[Authorize]` (se reutiliza `Index` existente, sin nueva `Home`)
- `WebSDD/Pages/Shared/_Layout.cshtml` (slot navbar usuario + logout POST + footer `Usuario:`; sin duplicar navbar)
- `WebSDD/wwwroot/css/site.css` (`.btn-login{background:#88E788}`, `.login-card`)
- `WebSDD/wwwroot/js/site.js` (eye-toggle + anti-doble/spinner)
- `libman.json` (+ `bootstrap-icons@1.11.3`)
- `WebSDD/appsettings.Example.json` (+ `RateLimiting:Login`)
- `docs/endpoints-ui.md` (filas `/Login,/Logout,/Denegado`), `docs/rate-limit-matrix.md`, `docs/asvs-web-checklist.md` (solo filas/enlaces, sin duplicar spec)

## 2. Contratos (rutas, config, env, `ApiSettings:BaseUrl`)

- `POST {BaseUrl}/api/v1/Auth/login` JSON `{strNombreUsuario,strPWD}` → `200 {strToken,dteExpira}`; errores → `Result.Fail(kind)` mapeado a `401 genérico / 429 / Red`.
- `POST {BaseUrl}/api/v1/Auth/logout` header `Authorization: Bearer <strToken>` → `200`; fallo no bloquea `SignOutAsync`.
- Cookie: `Name=.WebSDD.Auth`, `HttpOnly`, `Always`, `Strict`, `Sliding 24h`, `LoginPath=/Login`, `LogoutPath=/Logout`.
- Rate: `Login` fixed-window `5/5min` por IP sobre `POST /Login` y `POST /Logout`; `429 + Retry-After`; `/health*` exentos.
- Pipeline: `SecurityHeaders → Exception → HSTS → Https → Static → Routing → CORS → RateLimiter → RequestTimeouts → Authentication → Authorization → Endpoints`.

## 3. Cambios por archivo (qué + porqué, sin código completo salvo plantilla mínima)

- `Program.cs`: `AddAuthentication/AddCookie/AddAuthorization(fallback)` + `AddRateLimiter` policy Login + `AddScoped<IAuthService,AuthService>` + `UseAuthentication/UseAuthorization` tras `UseRateLimiter`. Porqué: auth requerida por defecto (`constitution.md §3`), orden fijo `§4`.
- `Services/AuthService.cs`: único que usa `IHttpClientFactory("api")`; `LoginAsync(usuario,pwd,ct)` → deserializa `strToken/dteExpira`, nunca loguea `strPWD/strToken`; `LogoutAsync(token,ct)`. Retorna `AuthResult{Succeeded,Kind:Ok|Invalid|Locked|RateLimited|Network,UserName,Token,Expires}`. Porqué: sin infraestructura desde UI, sin `new`, sin `try/catch` en UI.
- `Login.cshtml.cs`: `[AllowAnonymous]`, antiforgery implícito Razor, verifica `ModelState` (`StringLength` 64/256), llama servicio, `Invalid/Locked/Network → ModelState/TempData` (Locked con mismo genérico que Invalid), `Ok → SignInAsync(claims Name+access_token, IsPersistent=Recordarme, ExpiresUtc=min(now+24h,dteExpira))` + `LocalRedirect(returnUrl?)` solo `IsLocalUrl` else `/`.
- `Login.cshtml`: `h1`, `input-group bi-*`, `asp-validation-for`, `TempData` alerts partial, `section Scripts` vacía (lógica en `site.js`).
- `Logout`: `[Authorize] [ValidateAntiForgeryToken] OnPostAsync`: lee `access_token` claim → `LogoutAsync` (ignora fallo, log sin token) → `SignOutAsync` → `RedirectToPage("/Login")` con `NoStore`; `OnGet` → `RedirectToPage("/Login")` (GET no expone formulario).
- `Denegado`: `[AllowAnonymous] OnGet` mensaje 403 genérico, sin detalles.
- `Index`: `[Authorize]`; lee `User.Identity.Name ?? ""`; muestra textos exactos.
- `_Layout`: navbar existente + `if User.Identity.IsAuthenticated` bloque `Bienvenido` + form logout; footer `if` bloque `Usuario:`. Evita segundo `fixed-top` duplicado: reutilizar navbar actual (añadir `fixed-top` + `padding-top` body en `site.css`).
- `site.css/js`: clase `.btn-login`, toggle `data-toggle="password"` genérico, submit handler `data-loading="spinner"`.
- `libman.json`: añade `bootstrap-icons` pin `1.11.3`; corre `libman restore` (no CDN externo en cshtml).
- `appsettings.Example.json`: `RateLimiting.Login{PermitLimit:5,WindowSeconds:300}`.
- Tests usan `TestDataFactory` común + WAF (`HardeningFactory` patrón 001) sin red real (mock `HttpMessageHandler`).

## 4. Tests (Unit/Integration/Security por requisito `R-xxx → T-xxx`)

- `FR-001 → T-001(convención IsConventional: h1, labels, bi-*, sin CDN), T-004(Recordarme→IsPersistent mock SignIn)` [archivos: `LoginFlowTests` convención + `AuthServiceTests`]
- `FR-002 → T-005(WAF o convención `data-loading` + revisión manual si no hay harness JS)`
- `FR-003 → T-002(ModelState inválido no llama servicio), T-009(mapeo Invalid/Locked→mismo genérico, Network→mensaje red, sin leak Detail)` [archivo: `AuthServiceTests`]
- `FR-004 → T-003(cookie flags + 24h + min(dteExpira)), T-010(anon /Login 200, / 302→Login; authed /Login→/, / 200; /Denegado 200 anónimo)` [archivo: `LoginFlowTests`]
- `FR-005 → T-006(WAF authed muestra Bienvenido/Usuario:, encode `"<b>"` no ejecuta)` [archivo: `LoginFlowTests`]
- `FR-006 → T-007(logout llama API Bearer + borra cookie + no-store; fallo API igual redirige; GET /Logout→302 /Login)` [archivo: `LoginFlowTests`]
- `NFR-001 → T-011(429 en 6º POST, headers+CSP nonce, antiforgery 400 sin token, open-redirect `returnUrl` externa→`/`))` [archivo: `LoginSecurityTests`]
- `NFR-003 → T-012(gates + Memoria + DoD)`

## 5. Gates (build 0/0, coverage, semgrep, gitleaks)

`dotnet restore` → `build -c Release TreatWarningsAsErrors` 0/0 → `test Unit/Integration/Security -c Release --no-build` verdes → `critic-guardrails.ps1` PASS, `check_slnx` PASS, `check_endpoints` OK (tras actualizar docs), `sdd` PASS → `check_coverage ≥45%`, `delta --min 80 --advisory` informa → `semgrep --config=auto --config=.semgrep/semgrep.yaml --error` 0 nuevos → `gitleaks protect --staged` pass → `curl /health/ready` 200 + `curl -I /Login` headers. Stryker advisory + `build` posterior antes de `--no-build`.

## 6. Riesgos y deudas documentadas

- `since:main` Stryker testeará todo si solo hay Razor (sin C# nuevo vs main): aceptado advisory.
- `access_token` en claim viaja en cookie protegida DataProtection (`KeysPath` no-Dev); alternativa `ITicketStore` queda como deuda si revisor lo exige.
- Contraste `#88E788`: si falla AA, deuda pagar en mismo PR (oscurecer texto/borde).
- `Index` pública hoy: si se protege, `docs/endpoints-ui.md` cambia `/ no → sí`; conciliar en checklist.

## 7. Orden de implementación (coherente con `tasks.md`)

Fase A en orden numérico (T-001→T-003, sin dependencias cruzadas): lib/estilo → servicio/modelos → cookie/rate → 2. UI Login (`T-004/T-005`) → 3. Principal + Denegado (`T-006`) → 4. Logout (`T-007`) → 5. Docs (`T-008`) → 6. Tests+gates (`T-009/T-010/T-011/T-012`).
