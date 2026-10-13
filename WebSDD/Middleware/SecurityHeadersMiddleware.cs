using System.Security.Cryptography;

namespace WebSDD.Middleware;

// T-001: headers de seguridad + nonce CSP por request. Outermost en el pipeline.
// Decisión (constitución §3): middleware propio, sin paquete externo.
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public const string NonceKey = "ScriptNonce";

    public Task InvokeAsync(HttpContext context)
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        context.Items[NonceKey] = nonce;

        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        headers.ContentSecurityPolicy =
            $"default-src 'self'; script-src 'self' 'nonce-{nonce}'; style-src 'self'; " +
            "img-src 'self' data:; font-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'";

        return next(context);
    }
}
