namespace Vectra.Client.Models.Common;

/// <summary>
/// Represents a structured error response returned by the Vectra API.
/// </summary>
public sealed class VectraApiError
{
    /// <summary>Gets the human-readable error message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the error code, if provided by the server.</summary>
    public string? Code { get; init; }

    /// <summary>Gets the HTTP status code associated with this error.</summary>
    public int StatusCode { get; init; }

    /// <summary>Gets additional details about the error, if any.</summary>
    public string? Details { get; init; }
}
