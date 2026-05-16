using Vectra.Client.Abstractions;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Agents;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 03 — Agent Management
///
/// Demonstrates the full set of agent CRUD operations:
///   • Register a new agent
///   • List all agents with filtering
///   • Assign a policy to an agent
///   • Delete an agent
///
/// Note: This example creates a real agent in your gateway. The agent is
/// cleaned up at the end of the example unless an error occurs mid-way.
///
/// Prerequisites:
///   • Vectra gateway running and authenticated (BearerToken set)
///   • At least one policy configured (or leave PolicyName blank)
/// </summary>
public sealed class AgentManagementExample(IVectraClient vectra)
{
    private const string ExampleAgentName   = "sdk-example-agent";
    private const string ExampleAgentOwner  = "sdk-examples";
    private const string ExampleAgentSecret = "example-secret-do-not-use-in-prod";

    public async Task RunAsync(CancellationToken ct = default)
    {
        Guid? createdAgentId = null;

        try
        {
            // ── 1. Register ───────────────────────────────────────────────────
            Section("1. Register a new agent");

            var registration = await vectra.Agents.RegisterAsync(new RegisterAgentRequest
            {
                Name         = ExampleAgentName,
                OwnerId      = ExampleAgentOwner,
                ClientSecret = ExampleAgentSecret
            }, ct);

            createdAgentId = registration.AgentId;
            Out($"  ✓ Agent registered");
            Out($"      Name  : {ExampleAgentName}");
            Out($"      ID    : {createdAgentId}");
            Out($"      Owner : {ExampleAgentOwner}");

            // ── 2. List & locate ──────────────────────────────────────────────
            Section("2. List agents and locate the new one");

            var agents = await vectra.Agents.ListAsync(cancellationToken: ct);
            Out($"  Total agents in gateway: {agents.Count}");

            var found = agents.FirstOrDefault(a => a.AgentId == createdAgentId);
            if (found is null)
            {
                Out("  ✗ Newly registered agent not found in list.");
                return;
            }

            Out($"  ✓ Found in list:");
            PrintAgent(found);

            // ── 3. Filter active agents ───────────────────────────────────────
            Section("3. Filter — active agents only");

            var activeAgents = agents.Where(a => a.Status == AgentStatus.Active).ToList();
            Out($"  Active: {activeAgents.Count} / {agents.Count}");

            // ── 4. Assign a policy ────────────────────────────────────────────
            Section("4. Assign a policy");

            var policies = await vectra.Policies.ListAsync(cancellationToken: ct);
            if (policies.Count == 0)
            {
                Out("  No policies available — skipping assignment.");
                Out("  (Add a policy to the gateway config and re-run this example.)");
            }
            else
            {
                var policyName = policies[0].PolicyName;

                await vectra.Agents.AssignPolicyAsync(
                    createdAgentId.Value,
                    new AssignPolicyRequest { PolicyName = policyName },
                    ct);

                Out($"  ✓ Policy '{policyName}' assigned to agent {createdAgentId}");

                // Verify by re-listing
                var updated = (await vectra.Agents.ListAsync(cancellationToken: ct))
                    .FirstOrDefault(a => a.AgentId == createdAgentId);

                if (updated is not null)
                    Out($"  ✓ Confirmed: agent now shows policy = '{updated.PolicyName}'");
            }

            // ── 5. Re-assign to a different policy ────────────────────────────
            if (policies.Count > 1)
            {
                Section("5. Re-assign to a different policy");

                var newPolicy = policies[1].PolicyName;
                await vectra.Agents.AssignPolicyAsync(
                    createdAgentId.Value,
                    new AssignPolicyRequest { PolicyName = newPolicy },
                    ct);

                Out($"  ✓ Policy updated to '{newPolicy}'");
            }
        }
        finally
        {
            // ── 6. Cleanup ────────────────────────────────────────────────────
            if (createdAgentId.HasValue)
            {
                Section("6. Cleanup — delete the example agent");
                try
                {
                    await vectra.Agents.DeleteAsync(createdAgentId.Value, ct);
                    Out($"  ✓ Agent {createdAgentId} deleted.");
                }
                catch (Exception ex)
                {
                    Out($"  ✗ Cleanup failed: {ex.Message}");
                    Out($"    Please manually delete agent {createdAgentId}.");
                }
            }
        }
    }

    private static void PrintAgent(Models.Agents.AgentSummary a)
    {
        Out($"      ID     : {a.AgentId}");
        Out($"      Name   : {a.Name}");
        Out($"      Owner  : {a.OwnerId ?? "(none)"}");
        Out($"      Status : {a.Status}");
        Out($"      Policy : {a.PolicyName ?? "(none)"}");
    }

    private static void Out(string msg) => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
