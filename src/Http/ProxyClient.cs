using Microsoft.Extensions.Logging;
using Synentra.Client.Abstractions;
using Synentra.Client.Internal;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace Synentra.Client.Http;

internal sealed class ProxyClient : ISynentraProxyClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ProxyClient> _logger;

    public ProxyClient(HttpClient http, ILogger<ProxyClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>
    /// Posts JSON and returns the response body as a JsonNode.
    /// Handles empty responses (null return) and throws enriched exceptions on failure.
    /// </summary>
    public async Task<JsonNode?> ExecuteAsync(
        string path,
        string method,
        JsonNode payload,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var uri = $"proxy/{path.TrimStart('/')}";
        using var request = new HttpRequestMessage(new HttpMethod(method), uri)
        {
            Content = JsonContent.Create(payload, options: SynentraJsonOptions.Default)
        };

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        HttpResponseMessage response = await ProxyAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string? errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("HTTP {StatusCode} from {Path}: {ErrorBody}",
                (int)response.StatusCode, path, errorBody);

            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} from {path}: {errorBody}");
        }

        if (response.Content.Headers.ContentLength == 0)
            return null;

        return await response.Content.ReadFromJsonAsync<JsonNode>(
            SynentraJsonOptions.Default, cancellationToken);
    }

    /// <summary>
    /// Sends a raw HTTP request to the gateway and returns the response.
    /// This is the core proxy primitive.
    /// </summary>
    private async Task<HttpResponseMessage> ProxyAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // Clone the request because HttpClient.SendAsync disposes the request.
        // Alternatively, you can require the caller to not reuse it.
        using var clonedRequest = await CloneHttpRequestMessageAsync(request);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(clonedRequest, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Proxy request to {Path} failed", request.RequestUri?.PathAndQuery);
            throw new HttpRequestException($"Request to {request.RequestUri} failed", ex);
        }

        return response;
    }

    // Helper to clone a request (required because HttpClient disposes the original)
    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(
        HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);
            if (request.Content.Headers != null)
                foreach (var header in request.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        // Copy request options / properties if needed
        foreach (var property in request.Options)
            clone.Options.TryAdd(property.Key, property.Value);

        return clone;
    }
}