using Vectra.Client.Models.Common;

namespace Vectra.Client.Exceptions;

/// <summary>
/// Thrown when the Vectra API responds with an HTTP error status code.
/// </summary>
public sealed class VectraApiException : VectraException
{
    /// <summary>Gets the HTTP status code returned by the server.</summary>
    public int StatusCode { get; }

    /// <summary>Gets the structured API error payload, if the server returned one.</summary>
    public VectraApiError? ApiError { get; }

    /// <summary>
    /// Initializes a new instance from a raw HTTP status code and message.
    /// </summary>
    public VectraApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance from a structured API error response.
    /// </summary>
    public VectraApiException(VectraApiError apiError)
        : base(apiError.Message)
    {
        StatusCode = apiError.StatusCode;
        ApiError = apiError;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"VectraApiException [HTTP {StatusCode}]: {Message}";
}
