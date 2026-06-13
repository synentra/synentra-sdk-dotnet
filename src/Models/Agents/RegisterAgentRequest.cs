using System.ComponentModel.DataAnnotations;

namespace Synentra.Client.Models.Agents;

/// <summary>
/// The request body used to register a new AI agent.
/// </summary>
public sealed class RegisterAgentRequest
{
    /// <summary>Gets or sets the display name of the agent.</summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>Gets or sets the owner identifier (e.g. a user or team ID).</summary>
    [Required]
    public required string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the client secret that the agent will use to authenticate.
    /// Store this securely — the Synentra server only stores a hash.
    /// </summary>
    [Required]
    public required string ClientSecret { get; set; }
}
