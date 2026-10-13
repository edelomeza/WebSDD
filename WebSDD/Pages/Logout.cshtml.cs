using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using WebSDD.Services;

namespace WebSDD.Pages;

// SPEC-007 FR-006: POST antiforgery, Bearer a la API, SignOut siempre, 302 /Login no-store.
[Authorize]
[EnableRateLimiting("Login")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class LogoutModel(IAuthService auth, ILogger<LogoutModel> logger) : PageModel
{
    private static readonly Action<ILogger, Exception?> LogLogoutApiFailed =
        LoggerMessage.Define(LogLevel.Warning, new EventId(730, "LogoutApiFailed"), "Logout en la API falló; se continúa con el cierre local.");

    private readonly IAuthService _auth = auth;
    private readonly ILogger<LogoutModel> _logger = logger;

    public IActionResult OnGet()
    {
        return RedirectToPage("/Login");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var token = User.FindFirstValue("access_token");
        var apiOk = await _auth.LogoutAsync(token, cancellationToken).ConfigureAwait(false);
        if (!apiOk)
        {
            LogLogoutApiFailed(_logger, null);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        TempData["Success"] = "Sesión cerrada correctamente.";
        return RedirectToPage("/Login");
    }
}
