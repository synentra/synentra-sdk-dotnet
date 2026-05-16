using Vectra.Client.Abstractions;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Agents;
using Vectra.Client.Models.Tokens;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 06 — Error Handling Patterns
///
/// Covers every exception the SDK can throw and the recommended
/// pattern for handling each one in production code.
///
/// Exception hierarchy:
///   VectraException                    (base — all SDK errors)
///   ├── VectraApiException             (non-2xx HTTP response)
///   └── VectraAuthenticationException  (401 / 403 responses)
///
/// Patterns covered:
///   • Catching by HTTP status code (when clause)
///   • Catching 401 vs 403 separately
///   • Reading the structured ApiError payload
///   • Handling network / timeout failures
///   • A recommended production-safe wrapper
///   • Centralized error logging helper
/// </summary>
public sealed class ErrorHandlingExample(IVectraClient vectra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. 404 — Resource not found ───────────────────────────────────────
        Section("1. 404 — Resource not found");

        try
        {
            await vectra.Policies.GetAsync("does-not-exist", ct);
        }
        catch (VectraApiException ex) when (ex.StatusCode == 404)
        {
            Out($"  ✓ Caught 404 with when-clause:");
            Out($"      ex.StatusCode : {ex.StatusCode}");
            Out($"      ex.Message    : {ex.Message}");
        }

        // ── 2. 401 — Unauthenticated ──────────────────────────────────────────
        Section("2. 401 — Invalid credentials");

        try
        {
            await vectra.Tokens.GenerateAsync(new GenerateTokenRequest
            {
                AgentId      = Guid.NewGuid(),   // random unknown agent
                ClientSecret = "wrong"
            }, ct);
        }
        catch (VectraAuthenticationException ex) when (ex.StatusCode == 401)
        {
            Out($"  ✓ Caught VectraAuthenticationException (401):");
            Out($"      Message: {ex.Message}");
            Out("      Recommended action: prompt user to re-enter credentials.");
        }
        catch (VectraApiException ex)
        {
            // Server may return 400 for invalid request instead of 401
            Out($"  ✓ Caught VectraApiException [{ex.StatusCode}]: {ex.Message}");
        }

        // ── 3. Structured API error body ──────────────────────────────────────
        Section("3. Reading the structured ApiError payload");

        try
        {
            // Force a validation error by supplying invalid data
            await vectra.Agents.RegisterAsync(new RegisterAgentRequest
            {
                Name         = "",   // likely triggers a validation error
                OwnerId      = "owner",
                ClientSecret = "secret"
            }, ct);
        }
        catch (VectraApiException ex)
        {
            Out($"  Caught VectraApiException [{ex.StatusCode}]");

            if (ex.ApiError is { } error)
            {
                Out($"      error.Message : {error.Message}");
                Out($"      error.Code    : {error.Code ?? "(none)"}");
                Out($"      error.Details : {error.Details ?? "(none)"}");
            }
            else
            {
                Out($"      Raw message   : {ex.Message}");
                Out("      (Server did not return a structured error body.)");
            }
        }

        // ── 4. 403 — Forbidden ────────────────────────────────────────────────
        Section("4. 403 — Forbidden (agent lacks permission)");

        Out("  This would be caught the same way as 401:");
        Out("""
          try
          {
              await vectra.Agents.DeleteAsync(protectedAgentId);
          }
          catch (VectraAuthenticationException ex) when (ex.StatusCode == 403)
          {
              // The authenticated agent does not have delete permission
              logger.LogWarning("Access denied: {Message}", ex.Message);
          }
        """);

        // ── 5. Network / timeout errors ───────────────────────────────────────
        Section("5. Network and timeout errors");

        Out("  SDK exceptions do NOT wrap HttpRequestException or TaskCanceledException.");
        Out("  These bubble up as-is so you can distinguish network from API errors:");
        Out("""
          try
          {
              await vectra.Agents.ListAsync();
          }
          catch (VectraApiException ex)
          {
              // Server responded with an error status
          }
          catch (TaskCanceledException)
          {
              // Request timed out (HttpClient.Timeout exceeded)
          }
          catch (HttpRequestException ex)
          {
              // No network / DNS failure / TLS error
              logger.LogError(ex, "Gateway unreachable");
          }
        """);

        // ── 6. Production-safe wrapper ────────────────────────────────────────
        Section("6. Recommended production-safe wrapper");

        var result = await SafeExecuteAsync(
            () => vectra.Policies.GetAsync("my-policy", ct),
            fallback: null);

        Out($"  Result via SafeExecuteAsync: {(result is null ? "(null — error occurred)" : result.Name)}");

        // ── 7. Base class catch-all ───────────────────────────────────────────
        Section("7. Base class catch-all");

        Out("  Catch VectraException as a catch-all for any SDK error:");
        Out("""
          catch (VectraException ex)
          {
              logger.LogError(ex, "Vectra SDK error: {Type}", ex.GetType().Name);
          }
        """);
    }

    /// <summary>
    /// A production-safe wrapper that returns a fallback value on any Vectra SDK error,
    /// allowing calling code to remain exception-free.
    /// </summary>
    private static async Task<T?> SafeExecuteAsync<T>(
        Func<Task<T>> operation,
        T? fallback = default)
    {
        try
        {
            return await operation();
        }
        catch (VectraAuthenticationException ex)
        {
            LogError("AUTH", ex.StatusCode, ex.Message);
            return fallback;
        }
        catch (VectraApiException ex) when (ex.StatusCode == 404)
        {
            // 404s are often expected — log at debug level in real code
            return fallback;
        }
        catch (VectraApiException ex)
        {
            LogError("API", ex.StatusCode, ex.Message);
            return fallback;
        }
        catch (VectraException ex)
        {
            LogError("SDK", 0, ex.Message);
            return fallback;
        }
    }

    private static void LogError(string category, int statusCode, string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(statusCode > 0
            ? $"  [ERROR/{category} {statusCode}] {message}"
            : $"  [ERROR/{category}] {message}");
        Console.ResetColor();
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
