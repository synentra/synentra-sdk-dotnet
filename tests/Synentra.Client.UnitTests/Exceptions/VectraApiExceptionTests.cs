using Vectra.Client.Exceptions;
using Vectra.Client.Models.Common;

namespace Vectra.Client.UnitTests.Exceptions;

public sealed class VectraApiExceptionTests
{
    [Fact]
    public void Constructor_WithStatusCodeAndMessage_SetsProperties()
    {
        var ex = new VectraApiException(404, "Not found");

        ex.StatusCode.Should().Be(404);
        ex.Message.Should().Be("Not found");
        ex.ApiError.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithApiError_SetsAllProperties()
    {
        var apiError = new VectraApiError
        {
            Message = "Bad request",
            Code = "BAD_REQ",
            StatusCode = 400,
            Details = "field error"
        };

        var ex = new VectraApiException(apiError);

        ex.StatusCode.Should().Be(400);
        ex.Message.Should().Be("Bad request");
        ex.ApiError.Should().NotBeNull();
        ex.ApiError!.Code.Should().Be("BAD_REQ");
        ex.ApiError.Details.Should().Be("field error");
    }

    [Fact]
    public void ToString_IncludesStatusCodeAndMessage()
    {
        var ex = new VectraApiException(500, "Server error");

        ex.ToString().Should().Contain("500").And.Contain("Server error");
    }

    [Fact]
    public void IsSubclassOfVectraException()
    {
        var ex = new VectraApiException(400, "err");

        ex.Should().BeAssignableTo<VectraException>();
    }
}
