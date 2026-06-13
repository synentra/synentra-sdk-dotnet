using Synentra.Client.Abstractions;
using Synentra.Client.Internal;
using Synentra.Client.Models.Common;
using Synentra.Client.Models.Policies;

namespace Synentra.Client.Http;

/// <summary>
/// HTTP implementation of <see cref="ISynentraPolicyClient"/>.
/// </summary>
internal sealed class PolicyClient : ISynentraPolicyClient
{
    private readonly HttpClient _http;

    public PolicyClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PolicySummary>> ListAsync(
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync(
            $"Policies?page={page}&pageSize={pageSize}",
            cancellationToken);

        var paged = await response.ReadAsAsync<PagedResult<PolicySummary>>(cancellationToken);
        return paged.Items;
    }

    /// <inheritdoc />
    public async Task<PolicyDetails> GetAsync(
        string policyName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var response = await _http.GetAsync(
            $"Policies/{Uri.EscapeDataString(policyName)}",
            cancellationToken);

        return await response.ReadAsAsync<PolicyDetails>(cancellationToken);
    }
}
