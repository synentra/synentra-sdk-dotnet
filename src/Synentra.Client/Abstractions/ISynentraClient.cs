namespace Synentra.Client.Abstractions;

/// <summary>
/// The primary entry point for interacting with the Synentra Intent-Aware Governance Gateway.
/// </summary>
/// <remarks>
/// Obtain an instance via dependency injection using
/// <c>services.AddSynentraClient(options => ...)</c> and then inject
/// <see cref="ISynentraClient"/> into your services.
/// </remarks>
/// <example>
/// <code>
/// var agents = await synentraClient.Agents.ListAsync();
/// var token  = await synentraClient.Tokens.GenerateAsync(new GenerateTokenRequest { ... });
/// </code>
/// </example>
public interface ISynentraClient
{
    /// <summary>Gets the client for managing AI agents.</summary>
    ISynentraAgentClient Agents { get; }

    /// <summary>Gets the client for reading governance policies.</summary>
    ISynentraPolicyClient Policies { get; }

    /// <summary>Gets the client for managing Human-in-the-Loop review requests.</summary>
    ISynentraHitlClient Hitl { get; }

    /// <summary>Gets the client for obtaining JWT access tokens.</summary>
    ISynentraTokenClient Tokens { get; }
}
