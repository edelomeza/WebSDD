using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Globalization;
using System.Threading.RateLimiting;
using WebSDD.Logging;
using WebSDD.Middleware;
using WebSDD.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl missing");
builder.Services.AddHttpClient("api", c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
    c.Timeout = TimeSpan.FromSeconds(30);
}).AddStandardResilienceHandler();

// T-001: HSTS 365d solo no-Dev.
builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
});

// T-001: CORS 1 origen.
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"]
    ?? throw new InvalidOperationException("Cors:AllowedOrigin missing");
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(allowedOrigin)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// T-002: RateLimiter global antes de Auth + policy Login (SPEC-007 T-003: 5/5min).
var permitLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 1000);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
var loginPermitLimit = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 5);
var loginWindowSeconds = builder.Configuration.GetValue("RateLimiting:Login:WindowSeconds", 300);
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (context, cancellationToken) =>
    {
        // 007-login: el 429 nunca va vacío (Chrome muestra página genérica):
        // cuerpo legible + Retry-After del cupo que rechazó (Login 300s, global 60s).
        var isLogin = string.Equals(
            context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName,
            "Login",
            StringComparison.Ordinal);
        context.HttpContext.Response.Headers.RetryAfter = (isLogin ? loginWindowSeconds : windowSeconds)
            .ToString(CultureInfo.InvariantCulture);
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Demasiados intentos. Por favor, espera unos minutos e inténtalo de nuevo.",
            cancellationToken).ConfigureAwait(false);
    };
    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true,
        });
    });
    o.AddPolicy("Login", httpContext =>
    {
        // 007-login: solo los POST consumen cupo (el GET del formulario/antiforgery no cuenta,
        // si no 2 intentos reales agotan el límite de 5 y el usuario ve un 429 vacío).
        if (!HttpMethods.IsPost(httpContext.Request.Method))
        {
            return RateLimitPartition.GetNoLimiter("login-get");
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = loginPermitLimit,
            Window = TimeSpan.FromSeconds(loginWindowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true,
        });
    });
});

// SPEC-007 T-003: Cookie HttpOnly+Secure+Strict+Sliding 24h, auth requerida por defecto.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = ".WebSDD.Auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Strict;
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(24);
        o.LoginPath = "/Login";
        o.LogoutPath = "/Logout";
        o.AccessDeniedPath = "/Denegado";
    });
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// SPEC-007 T-002: servicio de Auth (único acceso a la API externa de Auth).
builder.Services.AddScoped<IAuthService, AuthService>();

// T-002: timeout de request desde config (60s).
var timeoutSeconds = builder.Configuration.GetValue("RequestTimeoutSeconds", 60);
builder.Services.AddRequestTimeouts(o => o.DefaultPolicy = new RequestTimeoutPolicy
{
    Timeout = TimeSpan.FromSeconds(timeoutSeconds),
});

// T-002: límites Kestrel.
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxConcurrentConnections = 100;
    o.Limits.MaxConcurrentUpgradedConnections = 100;
    o.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(60);
    o.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60);
});

// T-004: DataProtection KeysPath fuera de wwwroot, solo no-Dev.
if (!builder.Environment.IsDevelopment())
{
    var keysPath = builder.Configuration["DataProtection:KeysPath"];
    if (!string.IsNullOrWhiteSpace(keysPath))
    {
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
    }
}

// T-003: Serilog JSON + filtro de secretos.
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Filter.ByExcluding(evt => SecretFilter.ShouldExclude(
        evt.RenderMessage(CultureInfo.InvariantCulture),
        evt.Properties.Select(p => (p.Key, (string?)p.Value.ToString())))));

var app = builder.Build();

// T-001: SecurityHeaders outermost (constitución §4).
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseRequestTimeouts();
// SPEC-007 T-003: RateLimiter antes de Auth (constitución §4).
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages().WithStaticAssets();
// /health exento de rate-limit y de auth por defecto: medición fiable.
app.MapHealthChecks("/health").AllowAnonymous().DisableRateLimiting();
app.MapHealthChecks("/health/ready").AllowAnonymous().DisableRateLimiting();
await app.RunAsync();
