using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSDD.Pages
{
    // SPEC-007: Privacy conserva acceso anónimo (excepción consciente al fallback).
    [AllowAnonymous]
    public class PrivacyModel : PageModel
    {
        public void OnGet()
        {
            // Base esqueleto SPEC-000: sin lógica; la página solo renderiza (S1186).
        }
    }

}
