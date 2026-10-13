using System.Text.Json.Serialization;

namespace WebSDD.Models;

// DTOs exactos de APISDD v1 (Auth). Nombres de wirea via JsonPropertyName.
public sealed class LoginRequestDto
{
    [JsonPropertyName("strNombreUsuario")]
    public string StrNombreUsuario { get; set; } = string.Empty;

    [JsonPropertyName("strPWD")]
    public string StrPWD { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    [JsonPropertyName("strToken")]
    public string StrToken { get; set; } = string.Empty;

    [JsonPropertyName("dteExpira")]
    public DateTime DteExpira { get; set; }
}
