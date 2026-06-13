using System.Net;
using System.Text;
using System.Text.Json;
using Synentra.Client.Exceptions;
using Synentra.Client.Internal;
using Synentra.Client.Models.Common;
using Synentra.Client.UnitTests.Helpers;

namespace Synentra.Client.UnitTests.Internal;

public sealed class HttpResponseExtensionsTests
{
    [Fact]
    public async Task ReadAsAsync_ReturnsDeserializedObject_OnSuccess()
    {
        var expected = new SynentraApiError { Message = "test", StatusCode = 200 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.OK, expected);

        var result = await response.ReadAsAsync<SynentraApiError>();

        result.Message.Should().Be("test");
    }

    [Fact]
    public async Task ReadAsAsync_ThrowsSynentraApiException_WhenBodyIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        await Assert.ThrowsAsync<SynentraApiException>(() => response.ReadAsAsync<SynentraApiError>());
    }

    [Fact]
    public async Task EnsureSuccessAsync_DoesNotThrow_OnSuccessStatus()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK);

        await response.EnsureSuccessAsync(); // should not throw
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraAuthenticationException_On401()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        var ex = await Assert.ThrowsAsync<SynentraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraAuthenticationException_On403()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Forbidden);

        var ex = await Assert.ThrowsAsync<SynentraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraApiException_WithStructuredError_On400()
    {
        var errorBody = new { message = "Bad input", code = "BAD_INPUT", statusCode = 400 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.BadRequest, errorBody);

        var ex = await Assert.ThrowsAsync<SynentraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(400);
        ex.ApiError!.Code.Should().Be("BAD_INPUT");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraApiException_WithRawText_WhenBodyIsNotJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("internal error", Encoding.UTF8, "text/plain")
        };

        var ex = await Assert.ThrowsAsync<SynentraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(500);
        ex.Message.Should().Contain("internal error");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraApiException_WithFallbackMessage_WhenBodyIsEmpty()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<SynentraApiException>(
            () => response.EnsureSuccessAsync());

        ex.StatusCode.Should().Be(500);
        ex.Message.Should().Contain("500");
    }

    [Fact]
    public async Task EnsureSuccessAsync_ThrowsSynentraAuthenticationException_WithApiErrorMessage_On401()
    {
        var errorBody = new { message = "Token expired", statusCode = 401 };
        var response = MockHttpMessageHandler.CreateResponse(HttpStatusCode.Unauthorized, errorBody);

        var ex = await Assert.ThrowsAsync<SynentraAuthenticationException>(
            () => response.EnsureSuccessAsync());

        ex.Message.Should().Be("Token expired");
    }
}
