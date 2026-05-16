namespace Vectra.Client.Abstractions;

/// <summary>
/// The primary entry point for interacting with the Vectra Intent-Aware Governance Gateway.
/// </summary>
/// <remarks>
/// Obtain an instance via dependency injection using
/// <c>services.AddVectraClient(options => ...)</c> and then inject
/// <see cref="IVectraClient"/> into your services.
/// </remarks>
/// <example>
/// <code>
/// var agents = await vectraClient.Agents.ListAsync();
/// var token  = await vectraClient.Tokens.GenerateAsync(new GenerateTokenRequest { ... });
/// </code>
/// </example>
public interface IVectraClient
{
    /// <summary>Gets the client for managing AI agents.</summary>
    IVectraAgentClient Agents { get; }

    /// <summary>Gets the client for reading governance policies.</summary>
    IVectraPolicyClient Policies { get; }

    /// <summary>Gets the client for managing Human-in-the-Loop review requests.</summary>
    IVectraHitlClient Hitl { get; }

    /// <summary>Gets the client for obtaining JWT access tokens.</summary>
    IVectraTokenClient Tokens { get; }
}
