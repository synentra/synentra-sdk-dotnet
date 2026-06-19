using System.Text.Json.Nodes;
using Synentra.Client.Abstractions;

namespace Synentra.Client.Examples;

/// <summary>
/// Example 11 — Proxy Command
///
/// Demonstrates how to execute a custom proxy command on the Synentra gateway
/// using the SDK's <see cref="ISynentraProxyClient"/>.
///
/// Patterns covered:
///   • Building a JSON payload for a proxied endpoint
///   • Executing the command via <c>synentra.Proxy.ExecuteAsync</c>
///   • Handling proxy request failures gracefully
///
/// Prerequisites:
///   • Synentra gateway running
///   • Gateway has a matching command/route registered
/// </summary>
public sealed class ProxyCommandExample(ISynentraClient synentra)
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        Section("1. Execute proxy command");

        var path = "http://httpbingo.org/anything/user";
        var payload = new JsonObject
        {
            ["name"] = "world"
        };
        
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Synentra-Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZTdkYzNhYy1hZDRkLTQyNzQtODEyOC03OTM4Nzc5YmYxNzYiLCJhZ2VudF9uYW1lIjoiVGVzdEFnZW50IiwidHJ1c3Rfc2NvcmUiOiIwLjUiLCJqdGkiOiI1YjU5MDc5NC04MTA1LTQ5ODItYWM5NC1iMmYyZjhiMjgwYjEiLCJleHAiOjE3ODE4MTcwMDMsImlzcyI6InN5bmVudHJhIiwiYXVkIjoic3luZW50cmEtYWdlbnRzIn0.MWUxRS24wBz2cUfenhFq7sAKOR86gqT3fAKL2TSxM9U" }
        };

        Out($"  Path   : {path}");
        Out($"  Payload: {payload}");

        try
        {
            var result = await synentra.Proxy.ExecuteAsync(path, "POST", payload, headers, ct);
            Out($"  ✓ Result: {result}");
        }
        catch (HttpRequestException ex)
        {
            Out($"  ✗ Proxy request failed: {ex.Message}");
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
