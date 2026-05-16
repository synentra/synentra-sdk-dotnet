using Microsoft.Extensions.Options;
using Vectra.Client.Abstractions;
using Vectra.Client.Configuration;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Tokens;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 02 — Token Authentication
///
/// Demonstrates how to exchange agent credentials for a JWT bearer token
/// and how to apply that token to all subsequent SDK calls.
///
/// Patterns covered:
///   • One-shot token generation
///   • Injecting the token back into VectraClientOptions at runtime
///   • A reusable token-refresh helper class
///   • Handling authentication errors gracefully
///
/// Prerequisites:
///   • Vectra gateway running
///   • An agent already registered (agent ID + client secret)
/// </summary>
public sealed class TokenAuthenticationExample(
    IVectraClient vectra,
    IOptionsMonitor<VectraClientOptions> optionsMonitor)
{
    public async Task RunAsync(string agentSecret, CancellationToken ct = default)
    {
        // ── 1. One-shot token generation ──────────────────────────────────────
        Section("1. One-shot token generation");

        // We first need an agent ID. Grab the first active agent as an example.
        var agents = await vectra.Agents.ListAsync(cancellationToken: ct);
        if (agents.Count == 0)
        {
            Out("  No agents found. Register an agent first (example 03).");
            return;
        }

        var agent = agents[0];
        Out($"  Using agent: {agent.Name} ({agent.AgentId})");

        try
        {
            var result = await vectra.Tokens.GenerateAsync(new GenerateTokenRequest
            {
                AgentId      = agent.AgentId,
                ClientSecret = agentSecret
            }, ct);

            Out($"  ✓ Token received (first 40 chars): {result.AccessToken[..Math.Min(40, result.AccessToken.Length)]}...");

            // ── 2. Apply token to all future SDK calls ────────────────────────
            Section("2. Apply token to SDK options");

            optionsMonitor.CurrentValue.BearerToken = result.AccessToken;
            Out("  ✓ BearerToken updated in VectraClientOptions.");
            Out("    All subsequent SDK calls will include: Authorization: Bearer <token>");
        }
        catch (VectraAuthenticationException ex)
        {
            Out($"  ✗ Authentication failed [{ex.StatusCode}]: {ex.Message}");
            Out("    Check that the agentSecret in Program.cs matches the registered secret.");
            return;
        }

        // ── 3. Token refresh helper pattern ──────────────────────────────────
        Section("3. Token refresh helper pattern");

        Out("  The TokenRefresher class wraps token lifecycle management:");
        Out("    • Fetches a new token before the first call");
        Out("    • Re-fetches automatically when the token expires (configurable)");
        Out("    • Thread-safe via SemaphoreSlim");

        var refresher = new TokenRefresher(vectra, optionsMonitor, agent.AgentId, agentSecret);
        await refresher.EnsureValidTokenAsync(ct);
        Out("  ✓ TokenRefresher ensured a valid token is active.");

        // ── 4. Error scenarios ────────────────────────────────────────────────
        Section("4. Authentication error scenarios");

        try
        {
            await vectra.Tokens.GenerateAsync(new GenerateTokenRequest
            {
                AgentId      = agent.AgentId,
                ClientSecret = "wrong-secret-deliberately"
            }, ct);
        }
        catch (VectraAuthenticationException ex)
        {
            Out($"  ✓ Caught VectraAuthenticationException as expected:");
            Out($"      Status : {ex.StatusCode}");
            Out($"      Message: {ex.Message}");
        }
        catch (VectraApiException ex)
        {
            Out($"  ✓ Caught VectraApiException [{ex.StatusCode}]: {ex.Message}");
        }
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}

/// <summary>
/// A reusable helper that manages JWT token lifecycle for a single agent.
/// Use this in long-running services where tokens can expire.
/// </summary>
public sealed class TokenRefresher(
    IVectraClient vectra,
    IOptionsMonitor<VectraClientOptions> optionsMonitor,
    Guid agentId,
    string clientSecret)
{
    private readonly SemaphoreSlim _lock        = new(1, 1);
    private          DateTime      _expiresAt   = DateTime.MinValue;

    /// <summary>
    /// Ensures a valid token is set in <see cref="VectraClientOptions"/>.
    /// Fetches a new one if none is present or if <paramref name="refreshMargin"/>
    /// before expiry has been reached.
    /// </summary>
    /// <param name="refreshMargin">How early before expiry to refresh. Defaults to 2 minutes.</param>
    public async Task EnsureValidTokenAsync(
        CancellationToken ct = default,
        TimeSpan? refreshMargin = null)
    {
        var margin = refreshMargin ?? TimeSpan.FromMinutes(2);

        if (DateTime.UtcNow < _expiresAt - margin)
            return; // still valid

        await _lock.WaitAsync(ct);
        try
        {
            // Double-checked locking
            if (DateTime.UtcNow < _expiresAt - margin)
                return;

            var result = await vectra.Tokens.GenerateAsync(new GenerateTokenRequest
            {
                AgentId      = agentId,
                ClientSecret = clientSecret
            }, ct);

            optionsMonitor.CurrentValue.BearerToken = result.AccessToken;

            // Vectra JWTs are typically valid for 1 hour; adjust if your config differs.
            _expiresAt = DateTime.UtcNow.AddHours(1);
        }
        finally
        {
            _lock.Release();
        }
    }
}
