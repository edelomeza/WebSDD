namespace WebSDD.Services;

// SPEC-007 FR-003/FR-004/FR-006: único punto de acceso a la API externa de Auth.
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken);

    Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken);
}
