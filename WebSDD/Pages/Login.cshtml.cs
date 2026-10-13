using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using WebSDD.Models;
using WebSDD.Services;

namespace WebSDD.Pages;

// SPEC-007 FR-001/FR-003/FR-004: login delgado contra IAuthService, sin HttpClient directo.
[AllowAnonymous]
[EnableRateLimiting("Login")]
public class LoginModel(IAuthService auth, ILogger<LoginModel> logger) : PageModel
{
    private static readonly Action<ILogger, AuthFailureKind, Exception?> LogLoginFailed =
        LoggerMessage.Define<AuthFailureKind>(LogLevel.Warning, new EventId(720, "LoginFailed"), "Intento de inicio de sesión fallido: {Kind}.");

    private readonly IAuthService _auth = auth;
    private readonly ILogger<LoginModel> _logger = logger;

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _auth.LoginAsync(Input.Usuario, Input.Password, cancellationToken).ConfigureAwait(false);

        if (result.Succeeded && result.UserName is not null && result.Token is not null)
        {
            var expires = result.Expires is { } e && e < DateTimeOffset.UtcNow.AddHours(24)
                ? e
                : DateTimeOffset.UtcNow.AddHours(24);
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, result.UserName),
                    new Claim("access_token", result.Token),
                ],
                CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                IsPersistent = Input.Recordarme,
                ExpiresUtc = expires,
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return LocalRedirect(ReturnUrl);
            }

            return RedirectToPage("/Index");
        }

        LogLoginFailed(_logger, result.Kind, null);
        ModelState.AddModelError(
            string.Empty,
            result.Kind == AuthFailureKind.Network
                ? "No logramos conectarnos. Por favor, verifica tu conexión a internet o inténtalo más tarde."
                : "Usuario o contraseña no válidos.");
        return Page();
    }
}
