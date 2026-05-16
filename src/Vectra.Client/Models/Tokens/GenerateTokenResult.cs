namespace Vectra.Client.Models.Tokens;

/// <summary>
/// The result returned after successfully generating a JWT access token.
/// </summary>
public sealed class GenerateTokenResult
{
    /// <summary>Gets the JWT bearer token to use in subsequent API calls.</summary>
    public string AccessToken { get; init; } = string.Empty;
}
