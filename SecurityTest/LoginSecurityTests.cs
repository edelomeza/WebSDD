using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSDD.Services;

namespace SecurityTest;

// SPEC-007 T-011 →NFR-001,NFR-002. Antiforgery/CSP/cookies/rate reales; API externa doblada.
public class LoginSecurityTests : IClassFixture<LoginSecurityFactory>
{
    private readonly LoginSecurityFactory _factory;

    public LoginSecurityTests(LoginSecurityFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostLogin_WithoutAntiforgery_Returns400()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Usuario"] = "ana",
            ["Input.Password"] = "Secreta123",
        });

        using var response = await client.PostAsync("/Login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostLogin_RapidPosts_Returns429WithRetryAfter()
    {
        // La policy "Login" cuenta solo POST: 1 GET del token (no consume) + 2 POST ok, 3er POST 429.
        // Doble Invalid: el 429 es del middleware, sin depender de la API externa.
        using var factory = _factory.WithWebHostBuilder(builder => builder
            .UseSetting("RateLimiting:Login:PermitLimit", "2")
            .UseSetting("RateLimiting:Login:WindowSeconds", "300")
            .ConfigureTestServices(services =>
            {
                services.RemoveAll<IAuthService>();
                services.AddSingleton<IAuthService>(new DenyAuthService());
            }));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await Antiforgery.GetTokenAsync(client);

        using var first = await Antiforgery.PostLoginAsync(client, token, "nadie", "Nada123");
        using var second = await Antiforgery.PostLoginAsync(client, token, "nadie", "Nada123");
        using var third = await Antiforgery.PostLoginAsync(client, token, "nadie", "Nada123");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        third.StatusCode.Should().Be((HttpStatusCode)429);
        third.Headers.Should().ContainSingle(h => h.Key == "Retry-After");
        (await third.Content.ReadAsStringAsync()).Should().Contain("Demasiados intentos");
    }

    [Fact]
    public async Task PostLogin_ExternalReturnUrl_RedirectsHome()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService>(new AllowAuthService());
        }));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await Antiforgery.GetTokenAsync(client);

        using var response = await Antiforgery.PostLoginAsync(client, token, "ana", "Secreta123", "https://evil.example/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        LocationPath(response.Headers).Should().Be("/");
    }

    [Fact]
    public async Task LoginPage_CspNonce_DiffersPerRequest()
    {
        using var client = _factory.CreateClient();

        var first = NonceOf(await client.GetAsync("/Login"));
        var second = NonceOf(await client.GetAsync("/Login"));

        first.Should().NotBeNullOrEmpty();
        second.Should().NotBeNullOrEmpty();
        second.Should().NotBe(first);
    }

    [Fact]
    public async Task Login_SetsSecureCookieFlags()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService>(new AllowAuthService());
        }));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await Antiforgery.GetTokenAsync(client);

        using var response = await Antiforgery.PostLoginAsync(client, token, "ana", "Secreta123");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var cookie = AuthCookie(response);
        cookie.Should().NotBeNull();
        cookie.Should().ContainEquivalentOf("HttpOnly").And.ContainEquivalentOf("Secure").And.ContainEquivalentOf("SameSite=Strict");
    }

    [Fact]
    public async Task Login_NetworkFailure_LeaksNoDetails()
    {
        using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService>(new NetworkFailAuthService());
        }));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await Antiforgery.GetTokenAsync(client);

        using var response = await Antiforgery.PostLoginAsync(client, token, "ana", "Secreta123");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("No logramos conectarnos");
        body.Should().NotContain("Exception").And.NotContain("stack").And.NotContain("tok-");
        body.Should().NotContain("Secreta123");
    }

    private static string? LocationPath(HttpResponseHeaders headers)
    {
        if (headers.Location is not { } uri)
        {
            return null;
        }

        return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.ToString();
    }

    private static string? AuthCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return null;
        }

        foreach (var value in values)
        {
            if (value.StartsWith(".WebSDD.Auth=", StringComparison.Ordinal))
            {
                return value;
            }
        }

        return null;
    }

    private static string? NonceOf(HttpResponseMessage response)
    {
        using (response)
        {
            if (!response.Headers.TryGetValues("Content-Security-Policy", out var values))
            {
                return null;
            }

            foreach (var value in values)
            {
                var index = value.IndexOf("nonce-", StringComparison.Ordinal);
                if (index >= 0)
                {
                    var start = index + "nonce-".Length;
                    var end = value.IndexOfAny(['\'', ';', ' '], start);
                    return end >= 0 ? value[start..end] : value[start..];
                }
            }

            return null;
        }
    }

    private sealed class AllowAuthService : IAuthService
    {
        public Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(AuthResult.Ok(usuario, "tok-test", DateTimeOffset.UtcNow.AddHours(1)));
        }

        public Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class NetworkFailAuthService : IAuthService
    {
        public Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(AuthResult.Fail(AuthFailureKind.Network));
        }

        public Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class DenyAuthService : IAuthService
    {
        public Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken)
        {
            return Task.FromResult(AuthResult.Fail(AuthFailureKind.Invalid));
        }

        public Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }
    }

    private static class Antiforgery
    {
        public static async Task<TokenPair> GetTokenAsync(HttpClient client)
        {
            using var response = await client.GetAsync("/Login");
            response.EnsureSuccessStatusCode();
            var cookies = ReadCookies(response);
            var body = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(
                body,
                "__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
                RegexOptions.None,
                TimeSpan.FromSeconds(5));
            match.Success.Should().BeTrue();
            return new TokenPair(match.Groups[1].Value, cookies);
        }

        public static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, TokenPair token, string usuario, string password, string? returnUrl = null)
        {
            var url = string.IsNullOrEmpty(returnUrl) ? "/Login" : "/Login?returnUrl=" + Uri.EscapeDataString(returnUrl);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Cookie", token.Cookies);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token.Value,
                ["Input.Usuario"] = usuario,
                ["Input.Password"] = password,
                ["Input.Recordarme"] = "false",
            });
            return await client.SendAsync(request);
        }

        private static string ReadCookies(HttpResponseMessage response)
        {
            var pairs = new List<string>();
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                foreach (var value in values)
                {
                    pairs.Add(value.Split(';', 2)[0]);
                }
            }

            return string.Join("; ", pairs);
        }

        public sealed record TokenPair(string Value, string Cookies);
    }
}
