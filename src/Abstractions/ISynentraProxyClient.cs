using System.Text.Json.Nodes;

namespace Synentra.Client.Abstractions;

/// <summary>
/// Provides access to the Synentra Gateway proxy API.
/// </summary>
public interface ISynentraProxyClient
{
    /// <summary>
    /// Executes a command on the Synentra Gateway.
    /// </summary>
    /// <param name="path">The path to the command.</param>
    /// <param name="method">The HTTP method to use for the command.</param>
    /// <param name="payload">The payload for the command.</param>
    /// <param name="headers">Optional headers to include in the request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    Task<JsonNode?> ExecuteAsync(string path, 
        string method, 
        JsonNode payload, 
        Dictionary<string, string>? headers = null, 
        CancellationToken cancellationToken = default);
}