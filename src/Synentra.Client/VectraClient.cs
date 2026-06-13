using Vectra.Client.Abstractions;

namespace Vectra.Client;

/// <summary>
/// The default implementation of <see cref="IVectraClient"/>.
/// </summary>
/// <remarks>
/// Do not instantiate this class directly. Use
/// <c>services.AddVectraClient(...)</c> and inject <see cref="IVectraClient"/>.
/// </remarks>
public sealed class VectraClient : IVectraClient
{
    /// <inheritdoc />
    public IVectraAgentClient Agents { get; }

    /// <inheritdoc />
    public IVectraPolicyClient Policies { get; }

    /// <inheritdoc />
    public IVectraHitlClient Hitl { get; }

    /// <inheritdoc />
    public IVectraTokenClient Tokens { get; }

    /// <summary>
    /// Initializes a new <see cref="VectraClient"/> with injected sub-clients.
    /// </summary>
    public VectraClient(
        IVectraAgentClient agents,
        IVectraPolicyClient policies,
        IVectraHitlClient hitl,
        IVectraTokenClient tokens)
    {
        Agents = agents;
        Policies = policies;
        Hitl = hitl;
        Tokens = tokens;
    }
}
