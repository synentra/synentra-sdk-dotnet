using System.Net.Http.Json;
using Synentra.Client.Abstractions;
using Synentra.Client.Internal;
using Synentra.Client.Models.Agents;
using Synentra.Client.Models.Common;

namespace Synentra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="ISynentraAgentClient"/>.
/// </summary>
internal sealed class AgentClient : ISynentraAgentClient
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
            SynentraJsonOptions.Default,
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
            SynentraJsonOptions.Default,
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
