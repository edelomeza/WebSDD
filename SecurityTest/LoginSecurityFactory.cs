using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SecurityTest;

// Factoría WAF con defaults espejo de WebSDD/appsettings.Example.json (patrón 001).
// IAuthService real llama a la API externa: los tests que la necesitan la
// reemplazan con un doble vía WithWebHostBuilder (sin red).
public class LoginSecurityFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ApiSettings:BaseUrl", "http://mock-api:7227/");
        builder.UseSetting("Cors:AllowedOrigin", "https://localhost:7227");
    }
}
