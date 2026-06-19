using System.Net;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging.Abstractions;
using Synentra.Client.Http;
using Synentra.Client.UnitTests.Helpers;

namespace Synentra.Client.UnitTests.Http;

public class ProxyClientTests
{
    [Fact]
    public async Task ExecuteAsync_Success_ReturnsResult()
    {
        // Arrange
        var expectedResponse = new JsonObject { ["status"] = "ok" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, expectedResponse.ToJsonString());
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new ProxyClient(httpClient, NullLogger<ProxyClient>.Instance);     
        var payload = new JsonObject { ["data"] = "test" };

        // Act
        var result = await client.ExecuteAsync("my-command", "POST", payload);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ok", result["status"]?.GetValue<string>());
        Assert.Single(handler.Requests);
        Assert.Equal("http://localhost/proxy/my-command", handler.Requests[0].RequestUri?.ToString());
        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
    }

    [Fact]
    public async Task ExecuteAsync_ApiError_ThrowsSynentraApiException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "Not Found");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new ProxyClient(httpClient, NullLogger<ProxyClient>.Instance);
        var payload = new JsonObject { ["data"] = "test" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.ExecuteAsync("my-command", "POST", payload));
        exception.Message.Should().Contain("HTTP 404");
    }

    [Fact]
    public async Task ExecuteAsync_NoContent_ReturnsNull()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new ProxyClient(httpClient, NullLogger<ProxyClient>.Instance);
        var payload = new JsonObject { ["data"] = "test" };

        // Act
        var result = await client.ExecuteAsync("my-command", "POST", payload);

        // Assert
        result.Should().BeNull();
    }
}
