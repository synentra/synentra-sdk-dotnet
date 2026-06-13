namespace Vectra.Client.Models.Agents;

/// <summary>
/// Represents a summary of an AI agent returned in list responses.
/// </summary>
public sealed class AgentSummary
{
    /// <summary>Gets the unique identifier of the agent.</summary>
    public Guid AgentId { get; init; }

    /// <summary>Gets the display name of the agent.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the owner identifier of the agent.</summary>
    public string? OwnerId { get; init; }

    /// <summary>Gets the current status of the agent.</summary>
    public AgentStatus Status { get; init; }

    /// <summary>Gets the name of the policy assigned to this agent, if any.</summary>
    public string? PolicyName { get; init; }
}
