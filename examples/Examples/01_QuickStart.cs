using Vectra.Client.Abstractions;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 01 — Quick Start
///
/// The fastest way to verify your SDK is wired up correctly.
/// Lists agents and policies from the gateway with minimal boilerplate.
///
/// Prerequisites:
///   • Vectra gateway running at http://localhost:7080
///   • At least one agent and one policy already registered
///   • BearerToken set in Program.cs (or run example 02 first)
/// </summary>
public sealed class QuickStartExample(IVectraClient vectra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        Out("Connecting to Vectra gateway...");

        // ── 1. List agents ────────────────────────────────────────────────────
        var agents = await vectra.Agents.ListAsync(page: 1, pageSize: 10, ct);

        Section("Agents");
        if (agents.Count == 0)
        {
            Out("  No agents found. Register one via example 03.");
        }
        else
        {
            foreach (var a in agents)
                Out($"  [{a.Status,-8}] {a.Name,-30} id={a.AgentId}  policy={a.PolicyName ?? "(none)"}");
        }

        // ── 2. List policies ──────────────────────────────────────────────────
        var policies = await vectra.Policies.ListAsync(page: 1, pageSize: 10, ct);

        Section("Policies");
        if (policies.Count == 0)
        {
            Out("  No policies found. Add a policy YAML/JSON to the gateway config.");
        }
        else
        {
            foreach (var p in policies)
                Out($"  {p.PolicyName,-30} owner={p.Owner}");
        }

        // ── 3. HITL queue depth ───────────────────────────────────────────────
        var pending = await vectra.Hitl.GetAllPendingAsync(page: 1, pageSize: 10, ct);

        Section("HITL Queue");
        Out($"  Pending reviews: {pending.Count}");

        Out("\n  ✓ Quick start complete.");
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
