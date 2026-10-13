using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WebSDD.Models;

namespace WebSDD.Services;

// SPEC-007: Pages/Controladores nunca llaman HttpClient directo (constitución §4).
// Secretos (strPWD/strToken) jamás en logs: solo Kind + status code.
public sealed class AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger) : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly Action<ILogger, Exception?> LogLoginCanceled =
        LoggerMessage.Define(LogLevel.Warning, new EventId(701, "LoginCanceled"), "Login hacia la API cancelado o con timeout.");

    private static readonly Action<ILogger, Exception?> LogLoginNetwork =
        LoggerMessage.Define(LogLevel.Warning, new EventId(702, "LoginNetwork"), "Login hacia la API falló por red.");

    private static readonly Action<ILogger, Exception?> LogLoginUnexpected =
        LoggerMessage.Define(LogLevel.Error, new EventId(703, "LoginUnexpected"), "Login hacia la API falló de forma inesperada.");

    private static readonly Action<ILogger, Exception?> LogLoginBodyCanceled =
        LoggerMessage.Define(LogLevel.Warning, new EventId(704, "LoginBodyCanceled"), "Lectura de respuesta de login cancelada.");

    private static readonly Action<ILogger, Exception?> LogLoginBadJson =
        LoggerMessage.Define(LogLevel.Warning, new EventId(705, "LoginBadJson"), "Respuesta de login con JSON inválido.");

    private static readonly Action<ILogger, Exception?> LogLoginNoToken =
        LoggerMessage.Define(LogLevel.Warning, new EventId(706, "LoginNoToken"), "Respuesta de login sin token.");

    private static readonly Action<ILogger, int, Exception?> LogLoginRejected =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(707, "LoginRejected"), "Login rechazado por la API con {StatusCode}.");

    private static readonly Action<ILogger, int, Exception?> LogLogoutApi =
        LoggerMessage.Define<int>(LogLevel.Warning, new EventId(708, "LogoutApi"), "Logout en la API respondió {StatusCode}.");

    private static readonly Action<ILogger, Exception?> LogLogoutCanceled =
        LoggerMessage.Define(LogLevel.Warning, new EventId(709, "LogoutCanceled"), "Logout hacia la API cancelado o con timeout.");

    private static readonly Action<ILogger, Exception?> LogLogoutNetwork =
        LoggerMessage.Define(LogLevel.Warning, new EventId(710, "LogoutNetwork"), "Logout hacia la API falló por red.");

    private static readonly Action<ILogger, Exception?> LogLogoutUnexpected =
        LoggerMessage.Define(LogLevel.Error, new EventId(711, "LogoutUnexpected"), "Logout hacia la API falló de forma inesperada.");

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(usuario);
        ArgumentNullException.ThrowIfNull(password);

        var request = new LoginRequestDto { StrNombreUsuario = usuario, StrPWD = password };

        HttpResponseMessage response;
        try
        {
            var client = _httpClientFactory.CreateClient("api");
            response = await client.PostAsJsonAsync("api/v1/Auth/login", request, JsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            LogLoginCanceled(_logger, ex);
            return AuthResult.Fail(AuthFailureKind.Network);
        }
        catch (HttpRequestException ex)
        {
            LogLoginNetwork(_logger, ex);
            return AuthResult.Fail(AuthFailureKind.Network);
        }
        catch (Exception ex)
        {
            LogLoginUnexpected(_logger, ex);
            return AuthResult.Fail(AuthFailureKind.Network);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                LoginResponseDto? body;
                try
                {
                    body = await response.Content.ReadFromJsonAsync<LoginResponseDto>(JsonOptions, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex)
                {
                    LogLoginBodyCanceled(_logger, ex);
                    return AuthResult.Fail(AuthFailureKind.Network);
                }
                catch (JsonException ex)
                {
                    LogLoginBadJson(_logger, ex);
                    return AuthResult.Fail(AuthFailureKind.Network);
                }

                if (body is null || string.IsNullOrWhiteSpace(body.StrToken))
                {
                    LogLoginNoToken(_logger, null);
                    return AuthResult.Fail(AuthFailureKind.Network);
                }

                var expires = body.DteExpira == default
                    ? DateTimeOffset.UtcNow.AddHours(24)
                    : new DateTimeOffset(DateTime.SpecifyKind(body.DteExpira, DateTimeKind.Utc));
                return AuthResult.Ok(usuario, body.StrToken, expires);
            }

            return response.StatusCode switch
            {
                HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.NotFound
                    => FailLogged(AuthFailureKind.Invalid, response.StatusCode),
                HttpStatusCode.Locked => FailLogged(AuthFailureKind.Locked, response.StatusCode),
                (HttpStatusCode)429 => FailLogged(AuthFailureKind.RateLimited, response.StatusCode),
                _ => FailLogged(AuthFailureKind.Network, response.StatusCode),
            };
        }
    }

    public async Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return true;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("api");
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/Auth/logout");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                LogLogoutApi(_logger, (int)response.StatusCode, null);
                return false;
            }

            return true;
        }
        catch (OperationCanceledException ex)
        {
            LogLogoutCanceled(_logger, ex);
            return false;
        }
        catch (HttpRequestException ex)
        {
            LogLogoutNetwork(_logger, ex);
            return false;
        }
        catch (Exception ex)
        {
            LogLogoutUnexpected(_logger, ex);
            return false;
        }
    }

    private AuthResult FailLogged(AuthFailureKind kind, HttpStatusCode status)
    {
        LogLoginRejected(_logger, (int)status, null);
        return AuthResult.Fail(kind);
    }
}
