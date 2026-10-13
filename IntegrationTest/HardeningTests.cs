using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTest;

public class HardeningTests : IClassFixture<HardeningFactory>
{
    private readonly HardeningFactory _factory;

    public HardeningTests(HardeningFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SecurityHeaders_EmitsExpectedHeaders()
    {
        var client = _factory.CreateClient();

        // 007-login: `/` exige auth; headers se verifican en ruta anónima.
        var response = await client.GetAsync("/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainSingle(h => h.Key == "X-Content-Type-Options")
            .Which.Value.Should().Contain("nosniff");
        response.Headers.Should().ContainSingle(h => h.Key == "X-Frame-Options")
            .Which.Value.Should().Contain("DENY");
        response.Headers.Should().ContainSingle(h => h.Key == "Referrer-Policy");
        response.Headers.Should().ContainSingle(h => h.Key == "Permissions-Policy");
        response.Headers.Should().ContainSingle(h => h.Key == "Cross-Origin-Opener-Policy");
        response.Headers.Should().ContainSingle(h => h.Key == "Cross-Origin-Embedder-Policy");
        response.Headers.Should().ContainSingle(h => h.Key == "Content-Security-Policy")
            .Which.Value.Should().ContainSingle(v => v.Contains("nonce-", StringComparison.Ordinal));
        response.Headers.Should().ContainSingle(h => h.Key == "X-Correlation-Id");
    }

    [Fact]
    public async Task SecurityHeaders_CspNonce_DiffersPerRequest()
    {
        var client = _factory.CreateClient();

        // 007-login: `/` exige auth; nonce se verifica en ruta anónima.
        var first = ExtractNonce((await client.GetAsync("/Login")).Headers);
        var second = ExtractNonce((await client.GetAsync("/Login")).Headers);

        first.Should().NotBeNullOrEmpty();
        second.Should().NotBeNullOrEmpty();
        second.Should().NotBe(first);
    }

    [Fact]
    public async Task RateLimiter_Returns429_WhenLimitExceeded()
    {
        var factory = _factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("RateLimiting:Global:PermitLimit", "2");
            b.UseSetting("RateLimiting:Global:WindowSeconds", "60");
        });
        var client = factory.CreateClient();

        // 007-login: `/` exige auth; el global se verifica en ruta anónima.
        (await client.GetAsync("/Login")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/Login")).StatusCode.Should().Be(HttpStatusCode.OK);

        var limited = await client.GetAsync("/Login");
        limited.StatusCode.Should().Be((HttpStatusCode)429);
    }

    [Fact]
    public async Task Hsts_EmitsMaxAge365_InProduction()
    {
        var keysPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var factory = _factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Environment", "Production");
            b.UseSetting("DataProtection:KeysPath", keysPath);
        });
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://example.com"),
        });

        // 007-login: `/` exige auth; HSTS se verifica en ruta anónima.
        var response = await client.GetAsync("/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainSingle(h => h.Key == "Strict-Transport-Security")
            .Which.Value.Should().ContainSingle(v =>
                v.Contains("max-age=31536000", StringComparison.Ordinal) &&
                v.Contains("includesubdomains", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Health_IsExempt_FromRateLimiting()
    {
        var factory = _factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("RateLimiting:Global:PermitLimit", "1");
            b.UseSetting("RateLimiting:Global:WindowSeconds", "60");
        });
        var client = factory.CreateClient();

        // 007-login: `/` exige auth; el consumo global se verifica en ruta anónima.
        (await client.GetAsync("/Login")).StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/health/ready")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static string? ExtractNonce(System.Net.Http.Headers.HttpResponseHeaders headers)
    {
        if (!headers.TryGetValues("Content-Security-Policy", out var values))
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
