using Vectra.Client.Abstractions;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Agents;
using Vectra.Client.Models.Tokens;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 08 — Full Agent Lifecycle
///
/// Walks through the complete lifecycle of an AI agent from birth to deletion:
///
///   1. Register a new agent
///   2. Authenticate — obtain a JWT for the new agent
///   3. Verify the agent appears in the list as Active
///   4. Assign a governance policy
///   5. Upgrade to a stricter policy (re-assign)
///   6. Simulate the agent operating (inspect policy rules)
///   7. Decommission — delete the agent
///   8. Verify the agent is gone
///
/// This is a great reference for onboarding scripts or integration tests.
///
/// Prerequisites:
///   • Vectra gateway running and admin-authenticated (BearerToken set)
///   • At least one policy configured in the gateway
/// </summary>
public sealed class AgentLifecycleExample(IVectraClient vectra)
{
    private const string AgentName   = "lifecycle-demo-agent";
    private const string AgentOwner  = "sdk-examples";

    public async Task RunAsync(string agentSecret, CancellationToken ct = default)
    {
        Guid? agentId = null;

        try
        {
            // ── Step 1: Register ──────────────────────────────────────────────
            Step(1, "Register the agent");

            var reg = await vectra.Agents.RegisterAsync(new RegisterAgentRequest
            {
                Name         = AgentName,
                OwnerId      = AgentOwner,
                ClientSecret = agentSecret
            }, ct);

            agentId = reg.AgentId;
            OK($"Agent registered — ID: {agentId}");

            // ── Step 2: Authenticate ──────────────────────────────────────────
            Step(2, "Authenticate — obtain a JWT for the new agent");

            Models.Tokens.GenerateTokenResult? tokenResult = null;
            try
            {
                tokenResult = await vectra.Tokens.GenerateAsync(new GenerateTokenRequest
                {
                    AgentId      = agentId.Value,
                    ClientSecret = agentSecret
                }, ct);

                OK($"Token obtained (length: {tokenResult.AccessToken.Length} chars)");
            }
            catch (VectraAuthenticationException ex)
            {
                Warn($"Auth failed [{ex.StatusCode}]: {ex.Message}");
                Warn("Continuing lifecycle demo without token...");
            }

            // ── Step 3: Verify in list ────────────────────────────────────────
            Step(3, "Verify the agent appears as Active in the list");

            var agents = await vectra.Agents.ListAsync(cancellationToken: ct);
            var found  = agents.FirstOrDefault(a => a.AgentId == agentId);

            if (found is null)
            {
                Fail("Agent not found in list after registration!");
                return;
            }

            OK($"Found in list — Status: {found.Status}, Policy: {found.PolicyName ?? "(none)"}");

            if (found.Status != AgentStatus.Active)
                Warn($"Unexpected status: {found.Status}");

            // ── Step 4: Assign a policy ───────────────────────────────────────
            Step(4, "Assign a governance policy");

            var policies = await vectra.Policies.ListAsync(cancellationToken: ct);

            if (policies.Count == 0)
            {
                Warn("No policies available — skipping assignment steps.");
            }
            else
            {
                var policy = policies[0];
                await vectra.Agents.AssignPolicyAsync(
                    agentId.Value,
                    new AssignPolicyRequest { PolicyName = policy.PolicyName },
                    ct);

                OK($"Policy assigned: '{policy.PolicyName}'");

                // ── Step 5: Upgrade policy ────────────────────────────────────
                if (policies.Count > 1)
                {
                    Step(5, "Upgrade to a stricter policy");

                    var stricterPolicy = policies[1];
                    await vectra.Agents.AssignPolicyAsync(
                        agentId.Value,
                        new AssignPolicyRequest { PolicyName = stricterPolicy.PolicyName },
                        ct);

                    OK($"Policy updated to: '{stricterPolicy.PolicyName}'");
                }
                else
                {
                    Step(5, "Upgrade to a stricter policy — skipped (only one policy available)");
                }

                // ── Step 6: Inspect rules for the assigned policy ─────────────
                Step(6, "Inspect the assigned policy rules");

                var assignedPolicyName = (await vectra.Agents.ListAsync(cancellationToken: ct))
                    .FirstOrDefault(a => a.AgentId == agentId)?.PolicyName;

                if (assignedPolicyName is not null)
                {
                    var details = await vectra.Policies.GetAsync(assignedPolicyName, ct);
                    Out($"    Policy  : {details.Name}");
                    Out($"    Default : {details.Default}");
                    Out($"    Rules   : {details.Rules.Count}");

                    foreach (var rule in details.Rules.OrderByDescending(r => r.Priority))
                        Out($"      [{rule.Priority,3}] {rule.Name} → {rule.Effect}");
                }
            }

            // ── Step 7: Decommission ──────────────────────────────────────────
            Step(7, "Decommission — delete the agent");

            Console.Write("\n    Confirm deletion? [y/N]: ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (confirm is "y" or "yes")
            {
                await vectra.Agents.DeleteAsync(agentId.Value, ct);
                OK($"Agent {agentId} deleted.");
                agentId = null; // prevent double-delete in finally
            }
            else
            {
                Warn("Deletion skipped. Remember to clean up manually.");
            }

            // ── Step 8: Verify deletion ───────────────────────────────────────
            if (agentId is null)
            {
                Step(8, "Verify the agent is gone");

                var afterDelete = await vectra.Agents.ListAsync(cancellationToken: ct);
                var stillThere  = afterDelete.Any(a => a.AgentId == reg.AgentId);

                if (stillThere)
                    Fail("Agent still appears in list after deletion!");
                else
                    OK("Agent no longer appears in the list. ✓");
            }
        }
        finally
        {
            if (agentId.HasValue)
            {
                Out("\n    [cleanup] Removing example agent...");
                try
                {
                    await vectra.Agents.DeleteAsync(agentId.Value, ct);
                    Out("    [cleanup] Done.");
                }
                catch (Exception ex)
                {
                    Warn($"[cleanup] Failed: {ex.Message}. Delete agent {agentId} manually.");
                }
            }
        }
    }

    private static void Step(int n, string desc)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n  ── Step {n}: {desc}");
        Console.ResetColor();
    }

    private static void OK(string msg)   { Console.ForegroundColor = ConsoleColor.Green;    Console.WriteLine($"    ✓ {msg}"); Console.ResetColor(); }
    private static void Warn(string msg) { Console.ForegroundColor = ConsoleColor.DarkYellow; Console.WriteLine($"    ⚠ {msg}"); Console.ResetColor(); }
    private static void Fail(string msg) { Console.ForegroundColor = ConsoleColor.Red;       Console.WriteLine($"    ✗ {msg}"); Console.ResetColor(); }
    private static void Out(string msg)  => Console.WriteLine(msg);
}
