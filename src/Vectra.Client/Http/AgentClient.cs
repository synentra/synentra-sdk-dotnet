using System.Net.Http.Json;
using Vectra.Client.Abstractions;
using Vectra.Client.Internal;
using Vectra.Client.Models.Agents;
using Vectra.Client.Models.Common;

namespace Vectra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="IVectraAgentClient"/>.
/// </summary>
internal sealed class AgentClient : IVectraAgentClient
{
    private readonly HttpClient _http;

    public AgentClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentSummary>> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"Agents?page={page}&pageSize={pageSize}",
            cancellationToken);

        var paged = await response.ReadAsAsync<PagedResult<AgentSummary>>(cancellationToken);
        return paged.Items;
    }

    /// <inheritdoc />
    public async Task<RegisterAgentResult> RegisterAsync(
        RegisterAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _http.PostAsJsonAsync(
            "Agents",
            request,
            VectraJsonOptions.Default,
            cancellationToken);

        return await response.ReadAsAsync<RegisterAgentResult>(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AssignPolicyAsync(
        Guid agentId,
        AssignPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _http.PutAsJsonAsync(
            $"Agents/{agentId}/policy",
            request,
            VectraJsonOptions.Default,
            cancellationToken);

        await response.EnsureSuccessAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(
            $"Agents/{agentId}",
            cancellationToken);

        await response.EnsureSuccessAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task LiftQuarantineAsync(
        Guid agentId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.PostAsync(
            $"Agents/{agentId}/lift-quarantine",
            content: null,
            cancellationToken);

        await response.EnsureSuccessAsync(cancellationToken);
    }
}
