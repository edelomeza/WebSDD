using System.ComponentModel.DataAnnotations;

namespace WebSDD.Models;

// SPEC-007 FR-003: solo máximos anti-abuso; sin mínimo UI para no rechazar credenciales válidas.
public sealed class LoginInput
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(64, ErrorMessage = "El usuario no puede superar los 64 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(256, ErrorMessage = "La contraseña no puede superar los 256 caracteres.")]
    public string Password { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}
