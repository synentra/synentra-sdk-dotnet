using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Synentra.Client.Configuration;
using Synentra.Client.Internal;
using Synentra.Client.UnitTests.Helpers;

namespace Synentra.Client.UnitTests.Internal;

public sealed class BearerTokenHandlerTests
{
    private static IOptionsMonitor<SynentraClientOptions> CreateMonitor(SynentraClientOptions options)
    {
        var monitor = new StaticOptionsMonitor(options);
        return monitor;
    }

    private static HttpClient BuildPipeline(MockHttpMessageHandler innerHandler, SynentraClientOptions options)
    {
        var handler = new BearerTokenHandler(CreateMonitor(options))
        {
            InnerHandler = innerHandler
        };
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
    }

    [Fact]
    public async Task SendAsync_AttachesAuthorizationHeader_WhenTokenIsSet()
    {
        var inner = new MockHttpMessageHandler(HttpStatusCode.OK);
        var client = BuildPipeline(inner, new SynentraClientOptions { BaseUrl = "http://localhost", BearerToken = "test-token" });

        await client.GetAsync("/test");

        inner.Requests[0].Headers.Authorization.Should().NotBeNull();
        inner.Requests[0].Headers.Authorization!.Scheme.Should().Be("Bearer");
        inner.Requests[0].Headers.Authorization.Parameter.Should().Be("test-token");
    }

    [Fact]
    public async Task SendAsync_DoesNotOverrideExistingAuthorizationHeader()
    {
        var inner = new MockHttpMessageHandler(HttpStatusCode.OK);
        var client = BuildPipeline(inner, new SynentraClientOptions { BaseUrl = "http://localhost", BearerToken = "sdk-token" });

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "custom-token");
        await client.SendAsync(request);

        inner.Requests[0].Headers.Authorization!.Parameter.Should().Be("custom-token");
    }

    [Fact]
    public async Task SendAsync_DoesNotAttachAuthorizationHeader_WhenTokenIsNull()
    {
        var inner = new MockHttpMessageHandler(HttpStatusCode.OK);
        var client = BuildPipeline(inner, new SynentraClientOptions { BaseUrl = "http://localhost", BearerToken = null });

        await client.GetAsync("/test");

        inner.Requests[0].Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_DoesNotAttachAuthorizationHeader_WhenTokenIsWhitespace()
    {
        var inner = new MockHttpMessageHandler(HttpStatusCode.OK);
        var client = BuildPipeline(inner, new SynentraClientOptions { BaseUrl = "http://localhost", BearerToken = "   " });

        await client.GetAsync("/test");

        inner.Requests[0].Headers.Authorization.Should().BeNull();
    }

    private sealed class StaticOptionsMonitor : IOptionsMonitor<SynentraClientOptions>
    {
        public StaticOptionsMonitor(SynentraClientOptions value) => CurrentValue = value;
        public SynentraClientOptions CurrentValue { get; }
        public SynentraClientOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<SynentraClientOptions, string?> listener) => null;
    }
}
