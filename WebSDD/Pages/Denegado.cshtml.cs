using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSDD.Pages;

// SPEC-007 FR-004: destino de AccessDeniedPath, siempre anónimo para evitar bucles.
[AllowAnonymous]
public class DenegadoModel : PageModel
{
    public void OnGet()
    {
        // Sin lógica; solo renderiza (S1186).
    }
}
