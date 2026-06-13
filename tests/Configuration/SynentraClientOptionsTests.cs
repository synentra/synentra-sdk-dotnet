using Synentra.Client.Configuration;
using Synentra.Client.Models.Common;

namespace Synentra.Client.UnitTests.Configuration;

public sealed class SynentraClientOptionsTests
{
    [Fact]
    public void DefaultTimeout_IsThirtySeconds()
    {
        var options = new SynentraClientOptions { BaseUrl = "http://localhost" };

        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void DefaultThrowOnError_IsTrue()
    {
        var options = new SynentraClientOptions { BaseUrl = "http://localhost" };

        options.ThrowOnError.Should().BeTrue();
    }

    [Fact]
    public void DefaultBearerToken_IsNull()
    {
        var options = new SynentraClientOptions { BaseUrl = "http://localhost" };

        options.BearerToken.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new SynentraClientOptions
        {
            BaseUrl = "http://synentra:7080",
            BearerToken = "tok",
            Timeout = TimeSpan.FromSeconds(60),
            ThrowOnError = false
        };

        options.BaseUrl.Should().Be("http://synentra:7080");
        options.BearerToken.Should().Be("tok");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.ThrowOnError.Should().BeFalse();
    }
}
