namespace Vectra.Client.Models.Agents;

/// <summary>
/// The status of a Vectra AI agent.
/// </summary>
public enum AgentStatus
{
    /// <summary>The agent is active and can authenticate.</summary>
    Active,

    /// <summary>The agent has been revoked and cannot authenticate.</summary>
    Revoked
}
