using Synentra.Client.Models.Hitl;

namespace Synentra.Client.Abstractions;

/// <summary>
/// Provides operations for managing Human-in-the-Loop (HITL) review requests in Synentra.
/// </summary>
public interface ISynentraHitlClient
{
    /// <summary>
    /// Retrieves all HITL requests currently awaiting human review.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of pending HITL requests.</returns>
    Task<IReadOnlyList<PendingHitlRequest>> GetAllPendingAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the full status and details of a specific HITL request.
    /// </summary>
    /// <param name="id">The unique identifier of the HITL request.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="HitlStatusResponse"/> describing the current state of the request.</returns>
    Task<HitlStatusResponse> GetStatusAsync(
        string id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending HITL request, which causes the intercepted request to be
    /// replayed to the upstream service.
    /// </summary>
    /// <param name="id">The unique identifier of the HITL request.</param>
    /// <param name="decision">An optional reviewer comment.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task ApproveAsync(
        string id,
        ReviewDecisionRequest? decision = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Denies a pending HITL request, blocking the intercepted request permanently.
    /// </summary>
    /// <param name="id">The unique identifier of the HITL request.</param>
    /// <param name="decision">An optional reviewer comment.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task DenyAsync(
        string id,
        ReviewDecisionRequest? decision = null,
        CancellationToken cancellationToken = default);
}
