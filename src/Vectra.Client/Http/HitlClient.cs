using System.Net.Http.Json;
using Vectra.Client.Abstractions;
using Vectra.Client.Internal;
using Vectra.Client.Models.Common;
using Vectra.Client.Models.Hitl;

namespace Vectra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="IVectraHitlClient"/>.
/// </summary>
internal sealed class HitlClient : IVectraHitlClient
{
    private readonly HttpClient _http;

    public HitlClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PendingHitlRequest>> GetAllPendingAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"Hitls?page={page}&pageSize={pageSize}", cancellationToken);
        var paged = await response.ReadAsAsync<PagedResult<PendingHitlRequest>>(cancellationToken);
        return paged.Items;
    }

    /// <inheritdoc />
    public async Task<HitlStatusResponse> GetStatusAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _http.GetAsync(
            $"Hitls/{Uri.EscapeDataString(id)}",
            cancellationToken);

        return await response.ReadAsAsync<HitlStatusResponse>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ApproveAsync(
        string id,
        ReviewDecisionRequest? decision = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _http.PostAsJsonAsync(
            $"Hitls/{Uri.EscapeDataString(id)}/approve",
            decision ?? new ReviewDecisionRequest(),
            VectraJsonOptions.Default,
            cancellationToken);

        await response.EnsureSuccessAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DenyAsync(
        string id,
        ReviewDecisionRequest? decision = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var response = await _http.PostAsJsonAsync(
            $"Hitls/{Uri.EscapeDataString(id)}/deny",
            decision ?? new ReviewDecisionRequest(),
            VectraJsonOptions.Default,
            cancellationToken);

        await response.EnsureSuccessAsync(cancellationToken);
    }
}
