using Vectra.Client.Abstractions;
using Vectra.Client.Exceptions;
using Vectra.Client.Models.Hitl;

namespace Vectra.Client.Examples;

/// <summary>
/// Example 05 — HITL Review Workflow
///
/// Demonstrates the Human-in-the-Loop (HITL) review API — the mechanism
/// Vectra uses to intercept high-risk agent requests and hold them for
/// manual operator approval before replaying them upstream.
///
/// Patterns covered:
///   • List all pending HITL requests
///   • Inspect a specific request (status + details)
///   • Approve a request (gateway replays it to upstream)
///   • Deny a request (gateway permanently blocks it)
///   • Handling 404 for non-existent / expired requests
///   • Rendering pending requests in a review dashboard format
///
/// Prerequisites:
///   • Vectra gateway running and authenticated
///   • At least one pending HITL request in the queue
///     (trigger one by sending a request the gateway classifies as high-risk)
/// </summary>
public sealed class HitlWorkflowExample(IVectraClient vectra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        // ── 1. Get all pending requests ───────────────────────────────────────
        Section("1. List all pending HITL requests");

        var pending = await vectra.Hitl.GetAllPendingAsync(page: 1, pageSize: 10, ct);

        if (pending.Count == 0)
        {
            Out("  No pending HITL requests.");
            Out("  (Trigger a high-risk request through the gateway to generate one.)");
            Out();
            Out("  Continuing with status / error-handling demonstrations...");
        }
        else
        {
            Out($"  Found {pending.Count} pending request(s):\n");
            RenderDashboard(pending);
        }

        // ── 2. Inspect a specific request ────────────────────────────────────
        Section("2. Inspect a specific HITL request");

        if (pending.Count > 0)
        {
            var first = pending[0];
            Out($"  Fetching status for request: {first.Id}");

            var status = await vectra.Hitl.GetStatusAsync(first.Id, ct);
            PrintStatus(status);
        }
        else
        {
            Out("  (No pending requests to inspect — skipping.)");
        }

        // ── 3. Approve the oldest pending request ─────────────────────────────
        Section("3. Approve a pending request");

        if (pending.Count > 0)
        {
            var target = pending[0];

            Out($"  Approving request: {target.Id}");
            Out($"    Method : {target.Method}");
            Out($"    URL    : {target.Url}");
            Out($"    Reason : {target.Reason}");
            Out();

            Console.Write("  Confirm approval? [y/N]: ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (confirm is "y" or "yes")
            {
                await vectra.Hitl.ApproveAsync(target.Id, new ReviewDecisionRequest
                {
                    Comment = "Approved via SDK examples — verified safe."
                }, ct);

                Out($"  ✓ Request {target.Id} approved. Gateway will replay it to upstream.");
            }
            else
            {
                Out("  Skipped approval (no confirmation).");
            }
        }
        else
        {
            Out("  (No pending requests — skipping approval demo.)");
        }

        // ── 4. Deny a request ────────────────────────────────────────────────
        Section("4. Deny a pending request");

        // Re-fetch in case the approve consumed the first one
        var remaining = await vectra.Hitl.GetAllPendingAsync(page: 1, pageSize: 10, ct);

        if (remaining.Count > 0)
        {
            var target = remaining[0];

            Out($"  Denying request: {target.Id}");
            Out($"    Reason: {target.Reason}");
            Out();

            Console.Write("  Confirm denial? [y/N]: ");
            var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (confirm is "y" or "yes")
            {
                await vectra.Hitl.DenyAsync(target.Id, new ReviewDecisionRequest
                {
                    Comment = "Denied via SDK examples — suspicious pattern detected."
                }, ct);

                Out($"  ✓ Request {target.Id} denied.");
            }
            else
            {
                Out("  Skipped denial (no confirmation).");
            }
        }
        else
        {
            Out("  (No pending requests remaining — skipping denial demo.)");
        }

        // ── 5. 404 error handling ─────────────────────────────────────────────
        Section("5. Handling a non-existent HITL request (404)");

        try
        {
            await vectra.Hitl.GetStatusAsync("non-existent-hitl-id-00000000", ct);
        }
        catch (VectraApiException ex) when (ex.StatusCode == 404)
        {
            Out($"  ✓ Caught 404 as expected for unknown HITL ID.");
            Out($"      Message: {ex.Message}");
        }

        try
        {
            await vectra.Hitl.ApproveAsync("non-existent-hitl-id-00000000",
                new ReviewDecisionRequest { Comment = "test" }, ct);
        }
        catch (VectraApiException ex) when (ex.StatusCode == 404)
        {
            Out($"  ✓ Caught 404 on approve of unknown HITL ID.");
        }
    }

    private static void RenderDashboard(IReadOnlyList<PendingHitlRequest> requests)
    {
        Out($"  {"ID",-38} {"METHOD",-8} {"AGENT",-38} {"EXPIRES",-22} REASON");
        Out($"  {new string('─', 120)}");

        foreach (var r in requests)
        {
            var expiry = r.ExpiresAt < DateTime.UtcNow
                ? "EXPIRED"
                : r.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss");

            Out($"  {r.Id,-38} {r.Method,-8} {r.AgentId,-38} {expiry,-22} {r.Reason}");
        }
    }

    private static void PrintStatus(HitlStatusResponse s)
    {
        Out($"  ID     : {s.Id}");
        Out($"  Status : {s.Status}");

        if (s.Request is { } req)
        {
            Out($"  Method : {req.Method}");
            Out($"  URL    : {req.Url}");
            Out($"  Agent  : {req.AgentId}");
            Out($"  Reason : {req.Reason}");
            Out($"  Body   : {req.Body ?? "(empty)"}");
            Out($"  Expires: {req.ExpiresAt:u}");
        }
    }

    private static void Out(string msg = "") => Console.WriteLine(msg);
    private static void Section(string title)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  — {title} —");
        Console.ResetColor();
    }
}
