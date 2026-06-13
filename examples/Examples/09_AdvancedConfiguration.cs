using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Synentra.Client.Abstractions;
using Synentra.Client.Configuration;
using Synentra.Client.Extensions;

namespace Synentra.Client.Examples;

/// <summary>
/// Example 09 — Advanced Configuration
///
/// Demonstrates less-common but important configuration patterns:
///
///   1. Custom timeout per-client
///   2. Reading and modifying options at runtime via IOptionsMonitor
///   3. Constructing the SDK client manually (without DI host)
///   4. Configuring a short timeout to test resilience
///   5. Using multiple named gateway instances (multi-tenant)
///
/// Prerequisites:
///   • Synentra gateway running
/// </summary>
public sealed class AdvancedConfigurationExample(
    ISynentraClient synentra,
    IOptionsMonitor<SynentraClientOptions> optionsMonitor)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. Reading current options ────────────────────────────────────────
        Section("1. Reading the current SynentraClientOptions at runtime");

        var current = optionsMonitor.CurrentValue;
        Out($"  BaseUrl    : {current.BaseUrl}");
        Out($"  BearerToken: {(current.BearerToken is null ? "(not set)" : $"set ({current.BearerToken.Length} chars)")}");
        Out($"  Timeout    : {current.Timeout.TotalSeconds}s");
        Out($"  ThrowOnErr : {current.ThrowOnError}");

        // ── 2. Changing timeout at runtime ────────────────────────────────────
        Section("2. Modifying Timeout at runtime");

        var previousTimeout = current.Timeout;
        optionsMonitor.CurrentValue.Timeout = TimeSpan.FromSeconds(5);
        Out($"  Timeout changed from {previousTimeout.TotalSeconds}s → 5s");
        Out("  (Note: HttpClient.Timeout is set during DI registration and does not");
        Out("   change after build. For per-request timeouts use CancellationToken.)");

        // Restore
        optionsMonitor.CurrentValue.Timeout = previousTimeout;

        // ── 3. Per-request timeout via CancellationToken ──────────────────────
        Section("3. Per-request timeout via CancellationToken");

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        try
        {
            var agents = await synentra.Agents.ListAsync(cancellationToken: timeoutCts.Token);
            Out($"  ✓ Completed within 3s. Agents returned: {agents.Count}");
        }
        catch (OperationCanceledException)
        {
            Out("  ✗ Request timed out after 3s — gateway too slow or unreachable.");
        }

        // ── 4. Manually constructing the client (no DI host) ──────────────────
        Section("4. Manually constructing SynentraClient without a DI host");
        Out("  Useful for console tools, scripts, or integration tests.\n");

        var manualServices = new ServiceCollection();
        manualServices.AddSynentraClient(opts =>
        {
            opts.BaseUrl     = optionsMonitor.CurrentValue.BaseUrl;
            opts.BearerToken = optionsMonitor.CurrentValue.BearerToken;
            opts.Timeout     = TimeSpan.FromSeconds(10);
        });

        await using var sp = manualServices.BuildServiceProvider();
        var manualClient = sp.GetRequiredService<ISynentraClient>();

        var manualAgents = await manualClient.Agents.ListAsync(cancellationToken: ct);
        Out($"  ✓ Manual client returned {manualAgents.Count} agent(s).");

        // ── 5. Options-change listener ────────────────────────────────────────
        Section("5. Reacting to option changes with IOptionsMonitor");

        Out("  Register a change listener to react when BearerToken is rotated:");
        Out("""
          optionsMonitor.OnChange(opts =>
          {
              logger.LogInformation("SynentraClientOptions changed — new token applied.");
          });
        """);

        using var _ = optionsMonitor.OnChange(opts =>
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\n  [OnChange] BearerToken updated at {DateTime.Now:HH:mm:ss}");
            Console.ResetColor();
        });

        // Simulate a token rotation
        Out("  Simulating a token rotation...");
        optionsMonitor.CurrentValue.BearerToken = "rotated-token-example";
        await Task.Delay(50, ct); // let the callback fire
        optionsMonitor.CurrentValue.BearerToken = current.BearerToken; // restore

        // ── 6. Gateway URL swap (multi-environment) ───────────────────────────
        Section("6. Swapping gateway URL (multi-environment pattern)");

        Out("  Swap the gateway target at runtime — useful for blue/green or staging:");
        Out("""
          // Target staging
          optionsMonitor.CurrentValue.BaseUrl = "http://staging.internal:7080";

          // Back to production
          optionsMonitor.CurrentValue.BaseUrl = "http://prod.internal:7080";
        """);
        Out("  ⚠  HttpClient.BaseAddress is set once at startup via IHttpClientFactory.");
        Out("     For true runtime URL switching, inject separate named clients.");
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
