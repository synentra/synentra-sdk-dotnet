using Synentra.Client.Exceptions;

namespace Synentra.Client.UnitTests.Exceptions;

public sealed class SynentraAuthenticationExceptionTests
{
    [Fact]
    public void Constructor_SetsStatusCodeAndMessage()
    {
        var ex = new SynentraAuthenticationException(401, "Unauthorized");

        ex.StatusCode.Should().Be(401);
        ex.Message.Should().Be("Unauthorized");
    }

    [Fact]
    public void ToString_IncludesStatusCodeAndMessage()
    {
        var ex = new SynentraAuthenticationException(403, "Forbidden");

        ex.ToString().Should().Contain("403").And.Contain("Forbidden");
    }

    [Fact]
    public void IsSubclassOfSynentraException()
    {
        var ex = new SynentraAuthenticationException(401, "err");

        ex.Should().BeAssignableTo<SynentraException>();
    }
}
