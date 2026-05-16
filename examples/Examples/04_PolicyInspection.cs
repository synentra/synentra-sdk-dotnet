using Vectra.Client.Abstractions;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Policies;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 04 — Policy Inspection
///
/// Shows how to read, display, and analyse governance policies.
///
/// Patterns covered:
///   • Listing all policies
///   • Fetching full policy details including rules and conditions
///   • Rendering a human-readable policy summary
///   • Searching policies by owner
///   • Analysing rule priorities and effects
///   • Handling a 404 for a non-existent policy
///
/// Prerequisites:
///   • Vectra gateway running and authenticated
///   • At least one policy configured in the gateway
/// </summary>
public sealed class PolicyInspectionExample(IVectraClient vectra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. List all policies ──────────────────────────────────────────────
        Section("1. List all policies");

        var policies = await vectra.Policies.ListAsync(cancellationToken: ct);

        if (policies.Count == 0)
        {
            Out("  No policies found. Add policy definitions to your gateway config.");
            return;
        }

        foreach (var p in policies)
            Out($"  {p.PolicyName,-35} owner={p.Owner,-20} {p.Description}");

        Out($"\n  Total: {policies.Count}");

        // ── 2. Full details of each policy ───────────────────────────────────
        Section("2. Full details for each policy");

        foreach (var summary in policies)
        {
            try
            {
                var details = await vectra.Policies.GetAsync(summary.PolicyName, ct);
                PrintPolicyDetails(details);
            }
            catch (VectraApiException ex) when (ex.StatusCode == 404)
            {
                Out($"  Policy '{summary.PolicyName}' not found (404).");
            }
        }

        // ── 3. Search by owner ────────────────────────────────────────────────
        Section("3. Filter policies by owner");

        if (policies.Count > 0)
        {
            var owner = policies[0].Owner;
            var byOwner = policies.Where(p => p.Owner == owner).ToList();
            Out($"  Policies owned by '{owner}': {byOwner.Count}");
            foreach (var p in byOwner)
                Out($"    • {p.PolicyName}");
        }

        // ── 4. Analyse rule priorities ────────────────────────────────────────
        Section("4. Rule priority analysis across all policies");

        foreach (var summary in policies)
        {
            PolicyDetails details;
            try { details = await vectra.Policies.GetAsync(summary.PolicyName, ct); }
            catch { continue; }

            if (details.Rules.Count == 0) continue;

            var maxPriority = details.Rules.Max(r => r.Priority);
            var topRule     = details.Rules.First(r => r.Priority == maxPriority);

            Out($"  [{summary.PolicyName}] highest-priority rule: '{topRule.Name}' (priority={maxPriority}, effect={topRule.Effect})");
        }

        // ── 5. Count rules by effect ──────────────────────────────────────────
        Section("5. Rule effect breakdown");

        var allRules = new List<PolicyRule>();
        foreach (var summary in policies)
        {
            try
            {
                var d = await vectra.Policies.GetAsync(summary.PolicyName, ct);
                allRules.AddRange(d.Rules);
            }
            catch { /* skip */ }
        }

        var byEffect = allRules
            .GroupBy(r => r.Effect)
            .OrderByDescending(g => g.Count());

        foreach (var group in byEffect)
            Out($"  {group.Key,-10} : {group.Count()} rule(s)");

        // ── 6. 404 handling ───────────────────────────────────────────────────
        Section("6. Handling a non-existent policy (404)");

        try
        {
            await vectra.Policies.GetAsync("this-policy-does-not-exist", ct);
        }
        catch (VectraApiException ex) when (ex.StatusCode == 404)
        {
            Out($"  ✓ Caught 404 as expected: {ex.Message}");
        }
    }

    private static void PrintPolicyDetails(PolicyDetails p)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Out($"\n  ┌── {p.Name}");
        Console.ResetColor();
        Out($"  │  Description : {p.Description ?? "(none)"}");
        Out($"  │  Owner       : {p.Owner}");
        Out($"  │  Created     : {p.CreatedOn?.ToString("yyyy-MM-dd") ?? "(unknown)"}");
        Out($"  │  Default     : {p.Default}");
        Out($"  │  Rules       : {p.Rules.Count}");

        foreach (var rule in p.Rules.OrderByDescending(r => r.Priority))
        {
            Out($"  │");
            Out($"  │  [{rule.Priority,3}] {rule.Name}  →  {rule.Effect}");
            if (rule.Reason is not null)
                Out($"  │       Reason: {rule.Reason}");

            foreach (var cond in rule.Conditions)
                Out($"  │       If {cond.Field} {cond.Operator} {cond.Value}");
        }

        Out($"  └{'─', 40}");
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
