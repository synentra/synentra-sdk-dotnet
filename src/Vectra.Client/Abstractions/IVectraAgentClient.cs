using Vectra.Client.Models.Agents;

namespace Vectra.Client.Abstractions;

/// <summary>
/// Provides operations for managing AI agents in the Vectra gateway.
/// </summary>
public interface IVectraAgentClient
{
    /// <summary>
    /// Lists all registered agents with optional pagination.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of agents per page. Defaults to 25.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A paged list of agent summaries.</returns>
    Task<IReadOnlyList<AgentSummary>> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new AI agent in the Vectra gateway.
    /// </summary>
    /// <param name="request">The registration request containing name, owner, and client secret.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The result containing the new agent's unique identifier.</returns>
    Task<RegisterAgentResult> RegisterAsync(
        RegisterAgentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a policy to an existing agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent.</param>
    /// <param name="request">The request specifying which policy to assign.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task AssignPolicyAsync(
        Guid agentId,
        AssignPolicyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes an agent from the Vectra gateway.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task DeleteAsync(
        Guid agentId,
        CancellationToken cancellationToken = default);
}
