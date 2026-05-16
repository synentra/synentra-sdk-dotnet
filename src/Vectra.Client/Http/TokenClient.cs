using System.Net.Http.Json;
using Vectra.Client.Abstractions;
using Vectra.Client.Internal;
using Vectra.Client.Models.Tokens;

namespace Vectra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="IVectraTokenClient"/>.
/// </summary>
internal sealed class TokenClient : IVectraTokenClient
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
            VectraJsonOptions.Default,
            cancellationToken);

        return await response.ReadAsAsync<GenerateTokenResult>(cancellationToken);
    }
}
