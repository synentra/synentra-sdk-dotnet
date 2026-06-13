using Synentra.Client.Models.Tokens;

namespace Synentra.Client.Abstractions;

/// <summary>
/// Provides token authentication operations for the Synentra gateway.
/// </summary>
public interface ISynentraTokenClient
{
    /// <summary>
    /// Exchanges an agent's credentials for a JWT bearer token.
    /// </summary>
    /// <param name="request">The credential request containing the agent ID and client secret.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="GenerateTokenResult"/> containing the JWT access token.</returns>
    Task<GenerateTokenResult> GenerateAsync(
        GenerateTokenRequest request,
        CancellationToken cancellationToken = default);
}
