using Vectra.Client.Abstractions;
using Vectra.Client.Models.Agents;
using Vectra.Client.Models.Policies;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 10 — Pagination and Bulk Operations
///
/// Shows how to work with paginated list endpoints and perform bulk
/// operations across all results.
///
/// Patterns covered:
///   • Manual page-by-page iteration
///   • A reusable FetchAllAsync extension helper
///   • Bulk policy assignment across all agents
///   • Parallel bulk operations with concurrency control
///   • Aggregating data across pages
///
/// Prerequisites:
///   • Vectra gateway running and authenticated
///   • Multiple agents registered (register a few via example 03 first)
/// </summary>
public sealed class PaginationAndBulkExample(IVectraClient vectra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. Manual page-by-page iteration ──────────────────────────────────
        Section("1. Manual page-by-page iteration");

        const int PageSize = 5;
        var       page     = 1;
        var       totalFetched = 0;

        Out($"  Iterating agents with page size = {PageSize}:");

        while (true)
        {
            var batch = await vectra.Agents.ListAsync(page, PageSize, ct);

            if (batch.Count == 0)
                break;

            Out($"    Page {page,2}: {batch.Count} agent(s)");
            totalFetched += batch.Count;
            page++;

            if (batch.Count < PageSize)
                break; // last page
        }

        Out($"  Total agents fetched: {totalFetched}");

        // ── 2. FetchAllAsync helper ────────────────────────────────────────────
        Section("2. Using the FetchAllAsync helper (fetches every page automatically)");

        var allAgents   = await FetchAllAgentsAsync(vectra, pageSize: 25, ct);
        var allPolicies = await FetchAllPoliciesAsync(vectra, pageSize: 25, ct);

        Out($"  All agents   : {allAgents.Count}");
        Out($"  All policies : {allPolicies.Count}");

        // ── 3. Aggregate stats across all agents ──────────────────────────────
        Section("3. Aggregate stats across all agents");

        var activeCount  = allAgents.Count(a => a.Status == AgentStatus.Active);
        var revokedCount = allAgents.Count(a => a.Status == AgentStatus.Revoked);
        var withPolicy   = allAgents.Count(a => a.PolicyName is not null);
        var noPolicy     = allAgents.Count(a => a.PolicyName is null);

        Out($"  Status breakdown:");
        Out($"    Active  : {activeCount}");
        Out($"    Revoked : {revokedCount}");
        Out($"  Policy coverage:");
        Out($"    Assigned : {withPolicy}");
        Out($"    Unassigned: {noPolicy}");

        if (allAgents.Count > 0)
        {
            var byOwner = allAgents
                .GroupBy(a => a.OwnerId ?? "(unknown)")
                .OrderByDescending(g => g.Count());

            Out($"  By owner:");
            foreach (var g in byOwner)
                Out($"    {g.Key,-30} : {g.Count()}");
        }

        // ── 4. Bulk policy assignment ─────────────────────────────────────────
        Section("4. Bulk policy assignment — assign a policy to all unassigned agents");

        if (allPolicies.Count == 0)
        {
            Out("  No policies available — skipping bulk assignment.");
        }
        else
        {
            var defaultPolicy  = allPolicies[0].PolicyName;
            var unassigned     = allAgents.Where(a => a.PolicyName is null && a.Status == AgentStatus.Active).ToList();

            if (unassigned.Count == 0)
            {
                Out("  All active agents already have a policy assigned.");
            }
            else
            {
                Out($"  Assigning '{defaultPolicy}' to {unassigned.Count} unassigned agent(s)...");

                Console.Write("  Confirm bulk assignment? [y/N]: ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (confirm is "y" or "yes")
                {
                    var results = await BulkAssignPolicyAsync(
                        vectra,
                        unassigned,
                        defaultPolicy,
                        maxConcurrency: 3,
                        ct);

                    Out($"  ✓ Succeeded: {results.Count(r => r.Success)}");
                    Out($"  ✗ Failed   : {results.Count(r => !r.Success)}");

                    foreach (var r in results.Where(r => !r.Success))
                        Out($"    Agent {r.AgentId}: {r.Error}");
                }
                else
                {
                    Out("  Skipped.");
                }
            }
        }

        // ── 5. Policy rule aggregation across all policies ────────────────────
        Section("5. Cross-policy rule aggregation");

        var totalRules = 0;
        var effectCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var policySummary in allPolicies)
        {
            try
            {
                var details = await vectra.Policies.GetAsync(policySummary.PolicyName, ct);
                totalRules += details.Rules.Count;

                foreach (var rule in details.Rules)
                {
                    var key = rule.Effect.ToString();
                    effectCounts[key] = effectCounts.GetValueOrDefault(key) + 1;
                }
            }
            catch { /* skip inaccessible policies */ }
        }

        Out($"  Total rules across {allPolicies.Count} policy/policies: {totalRules}");
        foreach (var (effect, count) in effectCounts.OrderByDescending(kv => kv.Value))
            Out($"    {effect,-10} : {count}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches every page of agents and returns them as a single flat list.
    /// </summary>
    private static async Task<IReadOnlyList<AgentSummary>> FetchAllAgentsAsync(
        IVectraClient vectra,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var all  = new List<AgentSummary>();
        var page = 1;

        while (true)
        {
            var batch = await vectra.Agents.ListAsync(page, pageSize, ct);
            all.AddRange(batch);

            if (batch.Count < pageSize) break;
            page++;
        }

        return all;
    }

    /// <summary>
    /// Fetches every page of policies and returns them as a single flat list.
    /// </summary>
    private static async Task<IReadOnlyList<PolicySummary>> FetchAllPoliciesAsync(
        IVectraClient vectra,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var all  = new List<PolicySummary>();
        var page = 1;

        while (true)
        {
            var batch = await vectra.Policies.ListAsync(page, pageSize, ct);
            all.AddRange(batch);

            if (batch.Count < pageSize) break;
            page++;
        }

        return all;
    }

    /// <summary>
    /// Assigns a policy to a list of agents with bounded concurrency.
    /// Returns a result per agent indicating success or failure.
    /// </summary>
    private static async Task<IReadOnlyList<BulkAssignResult>> BulkAssignPolicyAsync(
        IVectraClient vectra,
        IEnumerable<AgentSummary> agents,
        string policyName,
        int maxConcurrency = 5,
        CancellationToken ct = default)
    {
        var results   = new List<BulkAssignResult>();
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks     = agents.Select(async agent =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                await vectra.Agents.AssignPolicyAsync(
                    agent.AgentId,
                    new AssignPolicyRequest { PolicyName = policyName },
                    ct);

                lock (results) results.Add(new BulkAssignResult(agent.AgentId, true, null));
            }
            catch (Exception ex)
            {
                lock (results) results.Add(new BulkAssignResult(agent.AgentId, false, ex.Message));
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    private record BulkAssignResult(Guid AgentId, bool Success, string? Error);

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
