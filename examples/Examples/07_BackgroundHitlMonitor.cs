using Synentra.Client.Abstractions;
using Synentra.Client.Models.Hitl;

namespace Synentra.Client.Examples;

/// <summary>
/// Example 07 — Background HITL Monitor
///
/// Demonstrates how to build a long-running polling loop that watches
/// the HITL review queue and alerts operators when new requests arrive.
///
/// Real-world patterns covered:
///   • Polling loop with configurable interval
///   • Change detection (only notify on new arrivals)
///   • Graceful shutdown via CancellationToken
///   • Exponential back-off on repeated API errors
///   • Pluggable alert handler (console, email, Slack, etc.)
///
/// In production, run this as a hosted BackgroundService:
///   services.AddHostedService&lt;HitlMonitorService&gt;();
///
/// Prerequisites:
///   • Synentra gateway running and authenticated
/// </summary>
public sealed class BackgroundHitlMonitorExample(ISynentraClient synentra)
{
    public async Task RunAsync(CancellationToken externalCt = default)
    {
        Out("  Starting HITL monitor — polls every 5 seconds.");
        Out("  Press ENTER to stop.\n");

        // Allow the user to stop by pressing Enter, independently of the
        // outer cancellation token passed by the menu runner.
        using var keyCts    = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt, keyCts.Token);

        // Listen for Enter in a background thread
        _ = Task.Run(() =>
        {
            Console.ReadLine();
            keyCts.Cancel();
        });

        var monitor = new HitlMonitor(
            synentra,
            pollInterval:  TimeSpan.FromSeconds(5),
            onNewRequest:  OnNewRequest,
            onError:       OnMonitorError);

        await monitor.RunAsync(linkedCts.Token);

        Out("\n  Monitor stopped.");
    }

    private static void OnNewRequest(PendingHitlRequest req)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\n  🔔 NEW HITL REQUEST ARRIVED");
        Console.ResetColor();
        Console.WriteLine($"     ID      : {req.Id}");
        Console.WriteLine($"     Agent   : {req.AgentId}");
        Console.WriteLine($"     Method  : {req.Method} {req.Url}");
        Console.WriteLine($"     Reason  : {req.Reason}");
        Console.WriteLine($"     Expires : {req.ExpiresAt:u}");
    }

    private static void OnMonitorError(Exception ex, int consecutiveFailures)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n  ✗ Monitor error (failure #{consecutiveFailures}): {ex.Message}");
        Console.ResetColor();
    }

    private static void Out(string msg) => Console.WriteLine(msg);
}

/// <summary>
/// Reusable polling monitor for the Synentra HITL queue.
/// Designed to be embedded in a BackgroundService for production use.
/// </summary>
public sealed class HitlMonitor(
    ISynentraClient synentra,
    TimeSpan pollInterval,
    Action<PendingHitlRequest> onNewRequest,
    Action<Exception, int>? onError = null)
{
    private static readonly TimeSpan MaxBackOff = TimeSpan.FromMinutes(2);

    public async Task RunAsync(CancellationToken ct)
    {
        var knownIds          = new HashSet<string>(StringComparer.Ordinal);
        var consecutiveFails  = 0;
        var currentInterval   = pollInterval;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var pending = await synentra.Hitl.GetAllPendingAsync(page: 1, pageSize: 10, cancellationToken: ct);

                // Detect and emit new arrivals
                foreach (var req in pending)
                {
                    if (knownIds.Add(req.Id))
                        onNewRequest(req);
                }

                // Remove IDs that are no longer pending (approved / denied / expired)
                var currentIds = pending.Select(r => r.Id).ToHashSet(StringComparer.Ordinal);
                knownIds.IntersectWith(currentIds);

                consecutiveFails = 0;
                currentInterval  = pollInterval; // reset back-off

                Console.Write($"\r  [{DateTime.Now:HH:mm:ss}] Pending: {pending.Count,3}   (press ENTER to stop)");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                consecutiveFails++;

                onError?.Invoke(ex, consecutiveFails);

                // Exponential back-off capped at MaxBackOff
                currentInterval = TimeSpan.FromMilliseconds(
                    Math.Min(pollInterval.TotalMilliseconds * Math.Pow(2, consecutiveFails - 1),
                             MaxBackOff.TotalMilliseconds));
            }

            try
            {
                await Task.Delay(currentInterval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
