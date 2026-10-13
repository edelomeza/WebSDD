using FluentAssertions;
using WebSDD.Logging;

namespace UnitTest;

public class SecretFilterTests
{
    [Fact]
    public void ShouldExclude_PasswordProperty_ReturnsTrue()
    {
        var properties = new List<(string Name, string? Value)> { ("Password", "hunter2") };

        SecretFilter.ShouldExclude("User login ok", properties).Should().BeTrue();
    }

    [Fact]
    public void ShouldExclude_SecretInMessage_ReturnsTrue()
    {
        var properties = new List<(string Name, string? Value)> { ("Elapsed", "12") };

        SecretFilter.ShouldExclude("Connecting with connectionstring xyz", properties).Should().BeTrue();
    }

    [Fact]
    public void ShouldExclude_TokenValue_ReturnsTrue()
    {
        var properties = new List<(string Name, string? Value)> { ("Auth", "bearer abc123") };

        SecretFilter.ShouldExclude("Request finished", properties).Should().BeTrue();
    }

    [Fact]
    public void ShouldExclude_BenignEvent_ReturnsFalse()
    {
        var properties = new List<(string Name, string? Value)>
        {
            ("Elapsed", "12"),
            ("CorrelationId", "abc123"),
        };

        SecretFilter.ShouldExclude("HTTP GET / responded 200", properties).Should().BeFalse();
    }
}
