using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using WebSDD.Models;
using WebSDD.Services;

namespace WebSDD.Pages
{
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
            return User.Identity?.IsAuthenticated == true
                ? RedirectToPage("/Index")
                : Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input is null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña no válidos.");
                return Page();
            }

            string usuario = Input.Usuario?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(usuario))
            {
                ModelState.AddModelError("Input.Usuario", "El usuario es obligatorio.");
                return Page();
            }

            Input.Usuario = usuario;
            string password = Input.Password ?? string.Empty;
            if (password.Length == 0)
            {
                ModelState.AddModelError("Input.Password", "La contraseña es obligatoria.");
                return Page();
            }

            AuthResult result = await _auth.LoginAsync(usuario, password, cancellationToken);

            if (result.Succeeded && result.UserName is not null && result.Token is not null)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                DateTimeOffset max = now.AddHours(24);
                DateTimeOffset expires = result.Expires is { } e && e < max ? e : max;
                if (expires <= now)
                {
                    LogLoginFailed(_logger, AuthFailureKind.Network, null);
                    ModelState.AddModelError(
                        string.Empty,
                        "No logramos conectarnos. Por favor, verifica tu conexión a internet o inténtalo más tarde.");
                    return Page();
                }

                ClaimsIdentity identity = new(
                    [
                        new Claim(ClaimTypes.Name, result.UserName),
                        new Claim("access_token", result.Token),
                    ],
                    CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new()
                {
                    IsPersistent = Input.Recordarme,
                    ExpiresUtc = expires,
                    AllowRefresh = true,
                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);

                return !string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
                    ? LocalRedirect(ReturnUrl)
                    : RedirectToPage("/Index");
            }

            AuthFailureKind effectiveKind = result.Succeeded ? AuthFailureKind.Network : result.Kind;
            LogLoginFailed(_logger, effectiveKind, null);
            if (effectiveKind == AuthFailureKind.Network)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No logramos conectarnos. Por favor, verifica tu conexión a internet o inténtalo más tarde.");
                return Page();
            }

            if (effectiveKind == AuthFailureKind.RateLimited)
            {
                Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ModelState.AddModelError(
                    string.Empty,
                    "Demasiados intentos. Por favor, inténtalo más tarde.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Usuario o contraseña no válidos.");
            return Page();
        }
    }
}
