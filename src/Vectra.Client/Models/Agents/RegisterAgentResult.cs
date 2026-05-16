namespace Vectra.Client.Models.Agents;

/// <summary>
/// The result returned after successfully registering a new agent.
/// </summary>
public sealed class RegisterAgentResult
{
    /// <summary>Gets the unique identifier of the newly created agent.</summary>
    public Guid AgentId { get; init; }
}
