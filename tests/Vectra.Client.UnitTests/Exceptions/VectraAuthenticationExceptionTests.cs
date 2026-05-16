using Vectra.Client.Exceptions;

namespace Vectra.Client.UnitTests.Exceptions;

public sealed class VectraAuthenticationExceptionTests
{
    [Fact]
    public void Constructor_SetsStatusCodeAndMessage()
    {
        var ex = new VectraAuthenticationException(401, "Unauthorized");

        ex.StatusCode.Should().Be(401);
        ex.Message.Should().Be("Unauthorized");
    }

    [Fact]
    public void ToString_IncludesStatusCodeAndMessage()
    {
        var ex = new VectraAuthenticationException(403, "Forbidden");

        ex.ToString().Should().Contain("403").And.Contain("Forbidden");
    }

    [Fact]
    public void IsSubclassOfVectraException()
    {
        var ex = new VectraAuthenticationException(401, "err");

        ex.Should().BeAssignableTo<VectraException>();
    }
}
