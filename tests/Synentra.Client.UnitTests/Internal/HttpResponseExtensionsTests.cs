using System.Net;
using System.Text;
using System.Text.Json;
using Vectra.Client.Exceptions;
using Vectra.Client.Internal;
using Vectra.Client.Models.Common;
using Vectra.Client.UnitTests.Helpers;

namespace Vectra.Client.UnitTests.Internal;

public sealed class HttpResponseExtensionsTests
{
    [Fact]
    public async Task ReadAsAsync_ReturnsDeserializedObject_OnSuccess()
    {
        var expected = new VectraApiError { Message = "test", StatusCode = 200 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.OK, expected);

        var result = await response.ReadAsAsync<VectraApiError>();

        result.Message.Should().Be("test");
    }

    [Fact]
    public async Task ReadAsAsync_ThrowsVectraApiException_WhenBodyIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        await Assert.ThrowsAsync<VectraApiException>(() => response.ReadAsAsync<VectraApiError>());
    }

    [Fact]
    public async Task EnsureSuccessAsync_DoesNotThrow_OnSuccessStatus()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        await response.EnsureSuccessAsync(); // should not throw
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraAuthenticationException_On401()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var ex = await Assert.ThrowsAsync<VectraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraAuthenticationException_On403()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Forbidden);

        var ex = await Assert.ThrowsAsync<VectraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraApiException_WithStructuredError_On400()
    {
        var errorBody = new { message = "Bad input", code = "BAD_INPUT", statusCode = 400 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.BadRequest, errorBody);

        var ex = await Assert.ThrowsAsync<VectraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(400);
        ex.ApiError!.Code.Should().Be("BAD_INPUT");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraApiException_WithRawText_WhenBodyIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("internal error", Encoding.UTF8, "text/plain")
        };

        var ex = await Assert.ThrowsAsync<VectraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(500);
        ex.Message.Should().Contain("internal error");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraApiException_WithFallbackMessage_WhenBodyIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<VectraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(500);
        ex.Message.Should().Contain("500");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsVectraAuthenticationException_WithApiErrorMessage_On401()
    {
        var errorBody = new { message = "Token expired", statusCode = 401 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.Unauthorized, errorBody);

        var ex = await Assert.ThrowsAsync<VectraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.Message.Should().Be("Token expired");
    }
}
