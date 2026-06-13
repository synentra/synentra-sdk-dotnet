using Vectra.Client.Configuration;
using Vectra.Client.Models.Common;

namespace Vectra.Client.UnitTests.Configuration;

public sealed class VectraClientOptionsTests
{
    [Fact]
    public void DefaultTimeout_IsThirtySeconds()
    {
        var options = new VectraClientOptions { BaseUrl = "http://localhost" };

        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void DefaultThrowOnError_IsTrue()
    {
        var options = new VectraClientOptions { BaseUrl = "http://localhost" };

        options.ThrowOnError.Should().BeTrue();
    }

    [Fact]
    public void DefaultBearerToken_IsNull()
    {
        var options = new VectraClientOptions { BaseUrl = "http://localhost" };

        options.BearerToken.Should().BeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var options = new VectraClientOptions
        {
            BaseUrl = "http://vectra:7080",
            BearerToken = "tok",
            Timeout = TimeSpan.FromSeconds(60),
            ThrowOnError = false
        };

        options.BaseUrl.Should().Be("http://vectra:7080");
        options.BearerToken.Should().Be("tok");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        options.ThrowOnError.Should().BeFalse();
    }
}
