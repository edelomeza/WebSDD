using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WebSDD.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    // SPEC-007: /Error siempre anónimo para evitar bucles con el fallback de auth.
    [AllowAnonymous]
#pragma warning disable S4502 // La página de error debe renderizar tras POSTs fallidos; antiforgery no aplica aquí.
    [IgnoreAntiforgeryToken]
#pragma warning restore S4502
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }

}
