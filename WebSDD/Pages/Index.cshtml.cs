using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSDD.Pages
{
    // SPEC-007 FR-005: principal interna, auth requerida (fallback).
    [Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            // Base esqueleto SPEC-000: sin lógica; la página solo renderiza (S1186).
        }
    }
}
