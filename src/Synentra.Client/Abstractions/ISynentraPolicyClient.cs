using Synentra.Client.Models.Policies;

namespace Synentra.Client.Abstractions;

/// <summary>
/// Provides read-only access to policies configured in the Synentra gateway.
/// </summary>
public interface ISynentraPolicyClient
{
    /// <summary>
    /// Lists all available policies with optional pagination.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based). Defaults to 1.</param>
    /// <param name="pageSize">The number of policies per page. Defaults to 25.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A list of policy summaries.</returns>
    Task<IReadOnlyList<PolicySummary>> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full details of a specific policy, including its rules.
    /// </summary>
    /// <param name="policyName">The name of the policy to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The full <see cref="PolicyDetails"/> for the requested policy.</returns>
    Task<PolicyDetails> GetAsync(
        string policyName,
        CancellationToken cancellationToken = default);
}
