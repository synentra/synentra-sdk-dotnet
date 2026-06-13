using System.ComponentModel.DataAnnotations;

namespace Synentra.Client.Models.Tokens;

/// <summary>
/// The request body used to exchange agent credentials for a JWT access token.
/// </summary>
public sealed class GenerateTokenRequest
{
    /// <summary>Gets or sets the agent's unique identifier.</summary>
    [Required]
    public required Guid AgentId { get; set; }

    /// <summary>Gets or sets the agent's client secret.</summary>
    [Required]
    public required string ClientSecret { get; set; }
}
