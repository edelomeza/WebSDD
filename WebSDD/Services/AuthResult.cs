namespace WebSDD.Services;

// SPEC-007 FR-003/FR-004: resultado sin filtrar detalles externos a la UI.
public enum AuthFailureKind
{
    None,
    Invalid,
    Locked,
    RateLimited,
    Network,
}

public sealed class AuthResult
{
    public bool Succeeded { get; private init; }

    public AuthFailureKind Kind { get; private init; } = AuthFailureKind.None;

    public string? UserName { get; private init; }

    public string? Token { get; private init; }

    public DateTimeOffset? Expires { get; private init; }

    public static AuthResult Ok(string userName, string token, DateTimeOffset expires) => new()
    {
        Succeeded = true,
        Kind = AuthFailureKind.None,
        UserName = userName,
        Token = token,
        Expires = expires,
    };

    public static AuthResult Fail(AuthFailureKind kind) => new()
    {
        Succeeded = false,
        Kind = kind,
    };
}
