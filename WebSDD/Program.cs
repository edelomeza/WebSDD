using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Globalization;
using System.Threading.RateLimiting;
using WebSDD.Logging;
using WebSDD.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("api", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl missing"));
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

// T-002: RateLimiter global antes de Auth (Auth passthrough en base: sin UseAuthentication).
var permitLimit = builder.Configuration.GetValue("RateLimiting:Global:PermitLimit", 1000);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:Global:WindowSeconds", 60);
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = (context, _) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = windowSeconds.ToString(CultureInfo.InvariantCulture);
        return ValueTask.CompletedTask;
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
});

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
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages().WithStaticAssets();
// /health exento de rate-limit: medición fiable.
app.MapHealthChecks("/health").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();
await app.RunAsync();
