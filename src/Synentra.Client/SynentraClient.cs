using Synentra.Client.Abstractions;

namespace Synentra.Client;

/// <summary>
/// The default implementation of <see cref="ISynentraClient"/>.
/// </summary>
/// <remarks>
/// Do not instantiate this class directly. Use
/// <c>services.AddSynentraClient(...)</c> and inject <see cref="ISynentraClient"/>.
/// </remarks>
public sealed class SynentraClient : ISynentraClient
{
    /// <inheritdoc />
    public ISynentraAgentClient Agents { get; }

    /// <inheritdoc />
    public ISynentraPolicyClient Policies { get; }

    /// <inheritdoc />
    public ISynentraHitlClient Hitl { get; }

    /// <inheritdoc />
    public ISynentraTokenClient Tokens { get; }

    /// <summary>
    /// Initializes a new <see cref="SynentraClient"/> with injected sub-clients.
    /// </summary>
    public SynentraClient(
        ISynentraAgentClient agents,
        ISynentraPolicyClient policies,
        ISynentraHitlClient hitl,
        ISynentraTokenClient tokens)
    {
        Agents = agents;
        Policies = policies;
        Hitl = hitl;
        Tokens = tokens;
    }
}
