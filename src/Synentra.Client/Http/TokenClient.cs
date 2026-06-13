using System.Net.Http.Json;
using Synentra.Client.Abstractions;
using Synentra.Client.Internal;
using Synentra.Client.Models.Tokens;

namespace Synentra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="ISynentraTokenClient"/>.
/// </summary>
internal sealed class TokenClient : ISynentraTokenClient
{
    private readonly HttpClient _http;

    public TokenClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<GenerateTokenResult> GenerateAsync(
        GenerateTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _http.PostAsJsonAsync(
            "Tokens",
            request,
            SynentraJsonOptions.Default,
            cancellationToken);

        return await response.ReadAsAsync<GenerateTokenResult>(cancellationToken);
    }
}
