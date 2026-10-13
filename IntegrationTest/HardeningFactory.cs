using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTest;

// Factoría WAF con defaults espejo de WebSDD/appsettings.Example.json.
// appsettings.json real es gitignorado y no existe en CI: sin estos
// UseSetting el host falla en arranque (fail-fast de config requerida).
// Si Example cambia, actualizar estos valores (ver Memoria Fase-1).
public class HardeningFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ApiSettings:BaseUrl", "http://mock-api:7227/");
        builder.UseSetting("Cors:AllowedOrigin", "https://localhost:7227");
    }
}
