using System.Net;
using System.Text;
using System.Text.Json;

namespace Vectra.Client.UnitTests.Helpers;

/// <summary>
/// A simple fake <see cref="HttpMessageHandler"/> that returns a pre-configured response.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public List<HttpRequestMessage> Requests { get; } = [];

    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    public MockHttpMessageHandler(HttpStatusCode statusCode, object? body = null)
        : this(_ => CreateResponse(statusCode, body)) { }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_handler(request));
    }

    internal static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, object? body)
    {
        var response = new HttpResponseMessage(statusCode);
        if (body is not null)
        {
            var json = body is string s ? s : JsonSerializer.Serialize(body);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        return response;
    }
}
