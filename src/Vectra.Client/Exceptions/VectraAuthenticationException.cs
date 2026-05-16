namespace Vectra.Client.Exceptions;

/// <summary>
/// Thrown when an authentication or authorization attempt with the Vectra API fails.
/// This includes invalid credentials, expired tokens, and revoked agents.
/// </summary>
public sealed class VectraAuthenticationException : VectraException
{
    /// <summary>Gets the HTTP status code returned by the server (typically 401 or 403).</summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance with the given status code and message.
    /// </summary>
    public VectraAuthenticationException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"VectraAuthenticationException [HTTP {StatusCode}]: {Message}";
}
