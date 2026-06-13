using Synentra.Client.Models.Common;

namespace Synentra.Client.Exceptions;

/// <summary>
/// Thrown when the Synentra API responds with an HTTP error status code.
/// </summary>
public sealed class SynentraApiException : SynentraException
{
    /// <summary>Gets the HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>Gets the structured API error payload, if the server returned one.</summary>
    public SynentraApiError? ApiError { get; }

    /// <summary>
    /// Initializes a new instance from a raw HTTP status code and message.
    /// </summary>
    public SynentraApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance from a structured API error response.
    /// </summary>
    public SynentraApiException(SynentraApiError apiError)
        : base(apiError.Message)
    {
        StatusCode = apiError.StatusCode;
        ApiError = apiError;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"SynentraApiException [HTTP {StatusCode}]: {Message}";
}
