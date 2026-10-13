# PLAN-000 — Base Configuración (WebSDD)

## 1. Árbol objetivo
```text
WebSDD.slnx
Directory.Build.props
.gitignore
README.md
.github/workflows/ci-pr.yml
compose.yml
WebSDD/
  WebSDD.csproj
  Program.cs
  appsettings.json
  appsettings.Example.json
  libman.json
  Dockerfile
  Controllers/ Models/ Services/ Views/ Pages/ wwwroot/ (.gitkeep)
UnitTest/
IntegrationTest/
SecurityTest/
specs/templates/spec-template.md
docs/endpoints-ui.md
docs/rate-limit-matrix.md
docs/asvs-web-checklist.md
```

## 2. `Program.cs` plantilla
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks(); // Requerido por MapHealthChecks (verificado 2026-10-13: sin esto InvalidOperationException al arrancar).
builder.Services.AddHttpClient("api", c => {
  c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl missing"));
  c.Timeout = TimeSpan.FromSeconds(30);
}).AddStandardResilienceHandler();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
await app.RunAsync();
```

## 3. Config files
- `Directory.Build.props`: `NuGetAudit true/all` + `TreatWarningsAsErrors` + `SecurityCodeScan + SonarAnalyzer`.
- `Dockerfile`: `sdk:10.0` restore/build/publish → `aspnet:10.0`, `8080`, non-root.
- `compose.yml`: `webapp` env `ApiSettings__BaseUrl=http://mock-api:7227/`, `mock-api` wiremock + `/__admin/health`.
- `ci-pr.yml`: `setup-dotnet@v5` + cache `~/.nuget/packages` hash `**/*.csproj`, build `--warnaserror`, test `--no-build`. Sonar/CodeQL con `if: env.TOKEN != ''`.
- `Properties/launchSettings.json`: conservar perfiles http/https del template + documentar puertos en Memoria (no hardcodear).

## 4. Riesgos
- `BaseUrl` ausente → fail-fast `InvalidOperationException`. Documentar en `Memoria.md`.
- IPv6 roto → diferir a fase red, no en base.
- Auth/CSP negocio → diferidos a `001` y slices. No verde falso.
- Sin remote no hay CI/firma/PR: T-000 bloquea T-032.
