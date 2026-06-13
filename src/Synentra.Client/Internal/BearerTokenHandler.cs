using System.Net.Http.Headers;
using Synentra.Client.Configuration;
using Microsoft.Extensions.Options;

namespace Synentra.Client.Internal;

/// <summary>
/// A <see cref="DelegatingHandler"/> that attaches a static Bearer token from
/// <see cref="SynentraClientOptions.BearerToken"/> to every outgoing request.
/// </summary>
internal sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<SynentraClientOptions> _options;

    public BearerTokenHandler(IOptionsMonitor<SynentraClientOptions> options)
    {
        _options = options;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _options.CurrentValue.BearerToken;

        if (!string.IsNullOrWhiteSpace(token)
            && request.Headers.Authorization is null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
