using Vectra.Client.Exceptions;

namespace Vectra.Client.UnitTests.Exceptions;

public sealed class VectraExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new VectraException("something went wrong");

        ex.Message.Should().Be("something went wrong");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInner()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new VectraException("outer", inner);

        ex.Message.Should().Be("outer");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void IsSubclassOfException()
    {
        var ex = new VectraException("test");

        ex.Should().BeAssignableTo<Exception>();
    }
}
