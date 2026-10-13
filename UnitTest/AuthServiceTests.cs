using System.Globalization;
using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebSDD.Services;

namespace UnitTest;

// SPEC-007 T-009 →FR-003,FR-004.
public class AuthServiceTests
{
    private static AuthService CreateService(StubHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.Is<string>(s => s == "api")))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") });
        return new AuthService(factory.Object, Mock.Of<ILogger<AuthService>>());
    }

    [Fact]
    public async Task Login_Ok_ReturnsTokenAndExpiry()
    {
        var expires = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc);
        var handler = new StubHandler(JsonContent(expires));
        var service = CreateService(handler);

        var result = await service.LoginAsync("ana", "Secreta123", CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.Kind.Should().Be(AuthFailureKind.None);
        result.UserName.Should().Be("ana");
        result.Token.Should().Be("tok-123");
        result.Expires.Should().Be(new DateTimeOffset(expires));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, AuthFailureKind.Invalid)]
    [InlineData(HttpStatusCode.Unauthorized, AuthFailureKind.Invalid)]
    [InlineData(HttpStatusCode.Forbidden, AuthFailureKind.Invalid)]
    [InlineData(HttpStatusCode.NotFound, AuthFailureKind.Invalid)]
    [InlineData(HttpStatusCode.Locked, AuthFailureKind.Locked)]
    [InlineData((HttpStatusCode)429, AuthFailureKind.RateLimited)]
    [InlineData(HttpStatusCode.InternalServerError, AuthFailureKind.Network)]
    public async Task Login_Status_MapsToKind(HttpStatusCode status, AuthFailureKind expected)
    {
        var handler = new StubHandler(new HttpResponseMessage(status));
        var service = CreateService(handler);

        var result = await service.LoginAsync("ana", "Secreta123", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Kind.Should().Be(expected);
    }

    [Fact]
    public async Task Login_NetworkError_MapsNetwork()
    {
        var handler = new StubHandler { ErrorToThrow = new HttpRequestException("red caída") };
        var service = CreateService(handler);

        var result = await service.LoginAsync("ana", "Secreta123", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Kind.Should().Be(AuthFailureKind.Network);
    }

    [Fact]
    public async Task Login_CanceledToken_MapsNetwork()
    {
        var handler = new StubHandler(JsonContent(DateTime.UtcNow.AddHours(1)));
        var service = CreateService(handler);

        using var canceled = new CancellationTokenSource();
        await canceled.CancelAsync();
        var result = await service.LoginAsync("ana", "Secreta123", canceled.Token);

        result.Succeeded.Should().BeFalse();
        result.Kind.Should().Be(AuthFailureKind.Network);
    }

    [Fact]
    public async Task Login_EmptyToken_MapsNetwork()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"strToken\":\"\",\"dteExpira\":\"2026-10-14T00:00:00Z\"}",
                Encoding.UTF8,
                "application/json"),
        });
        var service = CreateService(handler);

        var result = await service.LoginAsync("ana", "Secreta123", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Kind.Should().Be(AuthFailureKind.Network);
    }

    [Fact]
    public async Task Logout_EmptyToken_ReturnsTrueWithoutCall()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        (await service.LogoutAsync(null, CancellationToken.None)).Should().BeTrue();
        (await service.LogoutAsync("  ", CancellationToken.None)).Should().BeTrue();
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Logout_Ok_SendsBearerAndReturnsTrue()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var service = CreateService(handler);

        (await service.LogoutAsync("tok-123", CancellationToken.None)).Should().BeTrue();

        var request = handler.Requests.Should().ContainSingle().Subject;
        request.Method.Should().Be(HttpMethod.Post);
        var authorization = request.Headers.Authorization;
        authorization.Should().NotBeNull();
        authorization?.Scheme.Should().Be("Bearer");
        authorization?.Parameter.Should().Be("tok-123");
    }

    [Fact]
    public async Task Logout_ApiError_ReturnsFalse()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(handler);

        (await service.LogoutAsync("tok-123", CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task Logout_NetworkError_ReturnsFalse()
    {
        var handler = new StubHandler { ErrorToThrow = new HttpRequestException("red caída") };
        var service = CreateService(handler);

        (await service.LogoutAsync("tok-123", CancellationToken.None)).Should().BeFalse();
    }

    private static HttpResponseMessage JsonContent(DateTime expires)
    {
        var json = "{\"strToken\":\"tok-123\",\"dteExpira\":\"" + expires.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) + "\"}";
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public StubHandler(HttpResponseMessage response)
        {
            ResponseToReturn = response;
        }

        public StubHandler()
        {
            ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK);
        }

        public HttpResponseMessage ResponseToReturn { get; set; }

        public Exception? ErrorToThrow { get; set; }

        public List<HttpRequestMessage> Requests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (ErrorToThrow is not null)
            {
                throw ErrorToThrow;
            }

            return Task.FromResult(ResponseToReturn);
        }
    }
}
