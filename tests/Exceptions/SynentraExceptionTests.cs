using Synentra.Client.Exceptions;

namespace Synentra.Client.UnitTests.Exceptions;

public sealed class SynentraExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new SynentraException("something went wrong");

        ex.Message.Should().Be("something went wrong");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new SynentraException("outer", inner);

        ex.Message.Should().Be("outer");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void IsSubclassOfException()
    {
        var ex = new SynentraException("test");

        ex.Should().BeAssignableTo<Exception>();
    }
}
