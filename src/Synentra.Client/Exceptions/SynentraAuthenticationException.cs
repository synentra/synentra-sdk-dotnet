namespace Synentra.Client.Exceptions;

/// <summary>
/// Thrown when an authentication or authorization attempt with the Synentra API fails.
/// This includes invalid credentials, expired tokens, and revoked agents.
/// </summary>
public sealed class SynentraAuthenticationException : SynentraException
{
    /// <summary>Gets the HTTP status code returned by the server (typically 401 or 403).</summary>
    public int StatusCode { get; }

    /// <summary>
    /// Initializes a new instance with the given status code and message.
    /// </summary>
    public SynentraAuthenticationException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"SynentraAuthenticationException [HTTP {StatusCode}]: {Message}";
}
