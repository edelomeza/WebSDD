using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebSDD.Services;

namespace IntegrationTest;

// SPEC-007 T-010 →FR-004,FR-005,FR-006. Cookie real; API externa doblada en el borde.
public class LoginFlowTests : IClassFixture<HardeningFactory>
{
    private readonly HardeningFactory _factory;

    public LoginFlowTests(HardeningFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LoginPage_Anonymous_ReturnsForm()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Inicio de sesión")
            .And.Contain("bi-person-circle")
            .And.Contain("bi-lock")
            .And.Contain("type=\"password\"")
            .And.Contain("__RequestVerificationToken");
    }

    [Fact]
    public async Task Root_Anonymous_RedirectsToLogin()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Cookies.LocationPath(response.Headers).Should().StartWith("/Login");
    }

    [Fact]
    public async Task Login_Valid_CreatesCookieAndShowsWelcome()
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("ana", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);

        using var login = await session.PostLoginAsync("ana", "Secreta123", recordarme: true);

        login.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Cookies.LocationPath(login.Headers).Should().Be("/");
        var authCookie = Cookies.AuthCookie(login);
        authCookie.Should().NotBeNull();
        authCookie.Should().ContainEquivalentOf("HttpOnly").And.ContainEquivalentOf("SameSite=Strict");
        authCookie.Should().ContainEquivalentOf("expires=");
        session.UpdateFrom(login);

        using var home = await session.GetAsync("/");
        home.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await home.Content.ReadAsStringAsync();
        body.Should().Contain("Bienvenido ana!!").And.Contain("Usuario: ana");
    }

    [Fact]
    public async Task Login_WithoutRememberMe_IssuesSessionCookie()
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("ana", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);

        using var login = await session.PostLoginAsync("ana", "Secreta123", recordarme: false);

        login.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var authCookie = Cookies.AuthCookie(login);
        authCookie.Should().NotBeNull();
        authCookie.Should().NotContainEquivalentOf("expires=");
    }

    [Fact]
    public async Task Login_Authenticated_GetRedirectsHome()
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("ana", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);
        using var login = await session.PostLoginAsync("ana", "Secreta123", recordarme: false);
        session.UpdateFrom(login);

        using var again = await session.GetAsync("/Login");

        again.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Cookies.LocationPath(again.Headers).Should().Be("/");
    }

    [Theory]
    [InlineData("invalida", "Usuario o contraseña no válidos.")]
    [InlineData("bloqueada", "Usuario o contraseña no válidos.")]
    [InlineData("sinred", "No logramos conectarnos. Por favor, verifica tu conexión a internet o inténtalo más tarde.")]
    public async Task Login_Failure_ShowsGenericMessage(string usuario, string expected)
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (u, _) => MapFailure(u),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);

        using var response = await session.PostLoginAsync(usuario, "Secreta123", recordarme: false);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var decoded = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        decoded.Should().Contain(expected);
    }

    [Fact]
    public async Task Login_EmptyModel_DoesNotCallService()
    {
        var fake = new FakeAuthService();
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);

        using var response = await session.PostLoginAsync(string.Empty, string.Empty, recordarme: false);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fake.LoginCalls.Should().Be(0);
    }

    [Fact]
    public async Task UserName_WithHtml_IsEncoded()
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("<b>evil</b>", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);
        using var login = await session.PostLoginAsync("<b>evil</b>", "Secreta123", recordarme: false);
        session.UpdateFrom(login);

        using var home = await session.GetAsync("/");
        var body = await home.Content.ReadAsStringAsync();
        body.Should().Contain("&lt;b&gt;evil&lt;/b&gt;").And.NotContain("<b>evil</b>");
    }

    [Fact]
    public async Task Denegado_Anonymous_ReturnsGenericMessage()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/Denegado");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Acceso denegado");
    }

    [Fact]
    public async Task Logout_Get_RedirectsToLogin()
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("ana", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);
        using var login = await session.PostLoginAsync("ana", "Secreta123", recordarme: false);
        session.UpdateFrom(login);

        using var response = await session.GetAsync("/Logout");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Cookies.LocationPath(response.Headers).Should().Be("/Login");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Logout_Post_ClearsCookieAndRedirects(bool apiOk)
    {
        var fake = new FakeAuthService
        {
            LoginHandler = (_, _) => AuthResult.Ok("ana", "tok-1", DateTimeOffset.UtcNow.AddHours(2)),
            LogoutHandler = _ => apiOk,
        };
        using var factory = WithAuth(fake);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var session = await LoginPage.GetAsync(client);
        using var login = await session.PostLoginAsync("ana", "Secreta123", recordarme: false);
        session.UpdateFrom(login);
        await session.RefreshTokenAsync("/");

        using var logout = await session.PostAsync("/Logout");

        logout.StatusCode.Should().Be(HttpStatusCode.Redirect);
        Cookies.LocationPath(logout.Headers).Should().Be("/Login");
        var authCookie = Cookies.AuthCookie(logout);
        authCookie.Should().NotBeNull();
        authCookie.Should().StartWith(".WebSDD.Auth=;");
        fake.LogoutCalls.Should().Be(1);

        session.UpdateFrom(logout);
        using var home = await session.GetAsync("/");
        home.StatusCode.Should().Be(HttpStatusCode.Redirect);
    }

    private static AuthResult MapFailure(string usuario)
    {
        if (usuario == "bloqueada")
        {
            return AuthResult.Fail(AuthFailureKind.Locked);
        }

        if (usuario == "sinred")
        {
            return AuthResult.Fail(AuthFailureKind.Network);
        }

        return AuthResult.Fail(AuthFailureKind.Invalid);
    }

    private WebApplicationFactory<Program> WithAuth(FakeAuthService fake)
    {
        return _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService>(fake);
        }));
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Func<string, string, AuthResult> LoginHandler { get; set; } =
            (_, _) => AuthResult.Fail(AuthFailureKind.Invalid);

        public Func<string?, bool> LogoutHandler { get; set; } = _ => true;

        public int LoginCalls { get; private set; }

        public int LogoutCalls { get; private set; }

        public Task<AuthResult> LoginAsync(string usuario, string password, CancellationToken cancellationToken)
        {
            LoginCalls++;
            return Task.FromResult(LoginHandler(usuario, password));
        }

        public Task<bool> LogoutAsync(string? token, CancellationToken cancellationToken)
        {
            LogoutCalls++;
            return Task.FromResult(LogoutHandler(token));
        }
    }

    private static class Cookies
    {
        public static string? LocationPath(HttpResponseHeaders headers)
        {
            if (headers.Location is not { } uri)
            {
                return null;
            }

            return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.ToString();
        }

        public static string? AuthCookie(HttpResponseMessage response)
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
    }

    private sealed class LoginPage
    {
        private readonly HttpClient _client;
        private readonly Dictionary<string, string> _cookies = new(StringComparer.Ordinal);
        private string _token = string.Empty;

        private LoginPage(HttpClient client)
        {
            _client = client;
        }

        public static async Task<LoginPage> GetAsync(HttpClient client)
        {
            var page = new LoginPage(client);
            using var response = await client.GetAsync("/Login");
            response.EnsureSuccessStatusCode();
            page.UpdateFrom(response);
            page._token = ExtractToken(await response.Content.ReadAsStringAsync());
            return page;
        }

        public async Task RefreshTokenAsync(string url)
        {
            using var response = await GetAsync(url);
            response.EnsureSuccessStatusCode();
            UpdateFrom(response);
            _token = ExtractToken(await response.Content.ReadAsStringAsync());
        }

        private static string ExtractToken(string body)
        {
            var match = Regex.Match(
                body,
                "__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
                RegexOptions.None,
                TimeSpan.FromSeconds(5));
            match.Success.Should().BeTrue();
            return match.Groups[1].Value;
        }

        public void UpdateFrom(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
            {
                foreach (var value in values)
                {
                    var pair = value.Split(';', 2)[0];
                    var index = pair.IndexOf('=');
                    if (index > 0)
                    {
                        _cookies[pair[..index]] = pair[(index + 1)..];
                    }
                }
            }
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyCookies(request);
            return _client.SendAsync(request);
        }

        public Task<HttpResponseMessage> PostAsync(string url)
        {
            var pairs = new List<KeyValuePair<string, string>> { new("__RequestVerificationToken", _token) };
            return SendFormAsync(url, pairs);
        }

        public Task<HttpResponseMessage> PostLoginAsync(string usuario, string password, bool recordarme)
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new("__RequestVerificationToken", _token),
                new("Input.Usuario", usuario),
                new("Input.Password", password),
                new("Input.Recordarme", recordarme ? "true" : "false"),
            };
            return SendFormAsync("/Login", pairs);
        }

        private async Task<HttpResponseMessage> SendFormAsync(string url, List<KeyValuePair<string, string>> pairs)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            ApplyCookies(request);
            request.Content = new FormUrlEncodedContent(pairs);
            var response = await _client.SendAsync(request);
            UpdateFrom(response);
            return response;
        }

        private void ApplyCookies(HttpRequestMessage request)
        {
            if (_cookies.Count == 0)
            {
                return;
            }

            var header = string.Join("; ", _cookies.Select(kv => $"{kv.Key}={kv.Value}"));
            request.Headers.Add("Cookie", header);
        }
    }
}
