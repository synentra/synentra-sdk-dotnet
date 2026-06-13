namespace Vectra.Client.Models.Hitl;

/// <summary>
/// Represents a HITL request that is currently waiting for a human reviewer decision.
/// </summary>
public sealed class PendingHitlRequest
{
    /// <summary>Gets the unique identifier of the HITL request.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Gets the HTTP method of the intercepted request (e.g. <c>POST</c>).</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Gets the full URL of the intercepted request.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>Gets the HTTP headers from the intercepted request.</summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the request body, if any.</summary>
    public string? Body { get; init; }

    /// <summary>Gets the human-readable reason the request was intercepted.</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>Gets the identifier of the agent that sent the original request.</summary>
    public Guid AgentId { get; init; }

    /// <summary>Gets when the request was intercepted.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Gets when the HITL review window expires.</summary>
    public DateTime ExpiresAt { get; init; }
}
