# Synentra SDK for .NET

[![NuGet](https://img.shields.io/nuget/v/Synentra.Client.Net.svg)](https://www.nuget.org/packages/Synentra.Client.Net)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

Official .NET client library for **Synentra** — the Intent-Aware Governance Gateway for Autonomous AI Agents.

Synentra sits between your AI agents and the outside world, enforcing governance policies, intercepting high-risk actions, and routing requests through Human-in-the-Loop (HITL) review when needed. This SDK gives your .NET applications strongly-typed access to every capability Synentra exposes: agent registration, JWT authentication, policy inspection, and HITL review workflows.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [API Reference](#api-reference)
  - [Agent Management](#agent-management)
  - [Token Authentication](#token-authentication)
  - [Policy Inspection](#policy-inspection)
  - [Human-in-the-Loop (HITL)](#human-in-the-loop-hitl)
- [Error Handling](#error-handling)
- [Dependency Injection](#dependency-injection)
- [Advanced Usage](#advanced-usage)
- [Running the Examples](#running-the-examples)
- [Running the Tests](#running-the-tests)
- [Contributing](#contributing)
- [License](#license)

## Features

- **Agent Management** — Register, list, assign policies to, and delete AI agents.
- **JWT Authentication** — Exchange agent credentials for a scoped Bearer token with one call.
- **Policy Inspection** — List and inspect the governance policies configured in your gateway.
- **HITL Review Workflows** — Retrieve pending review requests, then approve or deny them programmatically.
- **First-class DI support** — Single `AddSynentraClient(...)` extension wires up all typed HTTP clients.
- **Automatic Bearer token injection** — Set `BearerToken` once; every outgoing request carries the header.
- **Structured exceptions** — `SynentraApiException` and `SynentraAuthenticationException` expose the HTTP status code and the server's error payload.
- **Full cancellation support** — Every async method accepts a `CancellationToken`.
- **Pagination** — All list operations accept `page` and `pageSize` parameters.

## Prerequisites

| Requirement | Version |
|---|---|
| .NET | 10.0+ |
| Synentra Gateway | Running and reachable (default: `http://localhost:7080`) |

## Installation

```bash
dotnet add package Synentra.Client.Net
```

Or via the NuGet Package Manager:

```
Install-Package Synentra.Client.Net
```

## Quick Start

```csharp
// Program.cs — ASP.NET Core / Generic Host
builder.Services.AddSynentraClient(options =>
{
    options.BaseUrl     = "http://localhost:7080";
    options.BearerToken = "your-jwt-token"; // optional; see Token Authentication
});
```

```csharp
// Any service that needs Synentra
public class MyService(ISynentraClient synentra)
{
    public async Task RunAsync(CancellationToken ct)
    {
        // List registered agents
        var agents = await synentra.Agents.ListAsync(page: 1, pageSize: 10, ct);
        foreach (var agent in agents)
            Console.WriteLine($"[{agent.Status,-8}] {agent.Name}  id={agent.AgentId}");

        // List governance policies
        var policies = await synentra.Policies.ListAsync(cancellationToken: ct);
        foreach (var policy in policies)
            Console.WriteLine($"{policy.PolicyName}  owner={policy.Owner}");

        // Check the HITL queue
        var pending = await synentra.Hitl.GetAllPendingAsync(cancellationToken: ct);
        Console.WriteLine($"Pending HITL reviews: {pending.Count}");
    }
}
```

## Configuration

Register the SDK with `AddSynentraClient` and supply a configuration delegate:

```csharp
builder.Services.AddSynentraClient(options =>
{
    options.BaseUrl     = "http://localhost:7080"; // required
    options.BearerToken = "<jwt>";                 // optional — injected on every request
    options.Timeout     = TimeSpan.FromSeconds(30);// default: 30 s
    options.ThrowOnError = true;                   // default: true
});
```

You can also pass a pre-built `SynentraClientOptions` instance (useful when binding from `appsettings.json`):

```csharp
var opts = builder.Configuration
    .GetSection("Synentra")
    .Get<SynentraClientOptions>()!;

builder.Services.AddSynentraClient(opts);
```

### `SynentraClientOptions` Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `string` | *(required)* | Base URL of the Synentra gateway. |
| `BearerToken` | `string?` | `null` | Static JWT injected as `Authorization: Bearer <token>`. |
| `Timeout` | `TimeSpan` | `00:00:30` | Per-request HTTP timeout. |
| `ThrowOnError` | `bool` | `true` | Throw `SynentraApiException` on non-2xx responses. |

## API Reference

All operations are accessed through the `ISynentraClient` facade:

```
├── .Agents    → ISynentraAgentClient
├── .Policies  → ISynentraPolicyClient
├── .Hitl      → ISynentraHitlClient
└── .Tokens    → ISynentraTokenClient
```

### Agent Management

```csharp
// List agents (paginated)
IReadOnlyList<AgentSummary> agents = await synentra.Agents.ListAsync(page: 1, pageSize: 25, ct);

// Register a new agent
RegisterAgentResult result = await synentra.Agents.RegisterAsync(new RegisterAgentRequest
{
    Name         = "my-research-agent",
    OwnerId      = "team-backend",
    ClientSecret = "super-secret-value"   // store securely — Synentra hashes it
}, ct);

Console.WriteLine($"New agent ID: {result.AgentId}");

// Assign a policy to an agent
await synentra.Agents.AssignPolicyAsync(result.AgentId, new AssignPolicyRequest
{
    PolicyName = "strict-outbound"
}, ct);

// Delete an agent
await synentra.Agents.DeleteAsync(result.AgentId, ct);
```

#### `AgentSummary` Properties

| Property | Type | Description |
|---|---|---|
| `AgentId` | `Guid` | Unique identifier. |
| `Name` | `string` | Display name. |
| `Status` | `AgentStatus` | Current lifecycle status (e.g. `Active`, `Inactive`). |
| `PolicyName` | `string?` | Name of the assigned policy, or `null`. |

### Token Authentication

Exchange an agent's credentials for a scoped JWT:

```csharp
GenerateTokenResult token = await synentra.Tokens.GenerateAsync(new GenerateTokenRequest
{
    AgentId      = agentId,
    ClientSecret = "super-secret-value"
}, ct);

// Apply the token to all subsequent SDK calls
optionsMonitor.CurrentValue.BearerToken = token.AccessToken;
```

#### Token Refresh Pattern

```csharp
public sealed class TokenRefresher(
    ISynentraClient synentra,
    IOptionsMonitor<SynentraClientOptions> options,
    Guid agentId,
    string clientSecret)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTime _expiresAt;

    public async Task EnsureValidTokenAsync(CancellationToken ct = default)
    {
        if (DateTime.UtcNow < _expiresAt) return;

        await _lock.WaitAsync(ct);
        try
        {
            if (DateTime.UtcNow >= _expiresAt)
            {
                var result = await synentra.Tokens.GenerateAsync(
                    new GenerateTokenRequest { AgentId = agentId, ClientSecret = clientSecret }, ct);

                options.CurrentValue.BearerToken = result.AccessToken;
                _expiresAt = DateTime.UtcNow.AddMinutes(55); // refresh before expiry
            }
        }
        finally { _lock.Release(); }
    }
}
```

### Policy Inspection

```csharp
// List all policies (paginated)
IReadOnlyList<PolicySummary> policies = await synentra.Policies.ListAsync(page: 1, pageSize: 25, ct);

// Get full policy details including rules
PolicyDetails details = await synentra.Policies.GetAsync("strict-outbound", ct);

Console.WriteLine($"Policy : {details.Name}");
Console.WriteLine($"Default: {details.Default}");
foreach (var rule in details.Rules)
    Console.WriteLine($"  [{rule.Effect}] {rule.Description}");
```

#### `PolicyDetails` Properties

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Policy identifier. |
| `Description` | `string?` | Human-readable description. |
| `Owner` | `string` | Team or user that owns this policy. |
| `Default` | `PolicyType` | Effect applied when no rule matches. |
| `Rules` | `IReadOnlyList<PolicyRule>` | Ordered list of evaluation rules. |
| `CreatedOn` | `DateTime?` | Creation timestamp. |

### Human-in-the-Loop (HITL)

When Synentra intercepts a high-risk agent request it raises a HITL item. Your operators (or automation) can then approve or deny the item, controlling whether the original request is replayed upstream.

```csharp
// 1. Get the queue of pending reviews
IReadOnlyList<PendingHitlRequest> queue =
    await synentra.Hitl.GetAllPendingAsync(page: 1, pageSize: 25, ct);

// 2. Inspect a specific item
HitlStatusResponse status = await synentra.Hitl.GetStatusAsync(queue[0].Id, ct);
Console.WriteLine($"Status : {status.Status}");
Console.WriteLine($"Method : {status.Method}  URL: {status.Url}");
Console.WriteLine($"Reason : {status.Reason}");

// 3. Approve — gateway replays the request to the upstream service
await synentra.Hitl.ApproveAsync(queue[0].Id, new ReviewDecisionRequest
{
    Comment = "Verified safe by on-call engineer."
}, ct);

// 4. Deny — gateway permanently blocks the request
await synentra.Hitl.DenyAsync(queue[0].Id, new ReviewDecisionRequest
{
    Comment = "Blocked: unexpected data-exfiltration attempt."
}, ct);
```

#### `PendingHitlRequest` Properties

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier of the HITL item. |
| `Method` | `string` | HTTP method of the intercepted request. |
| `Url` | `string` | Target URL of the intercepted request. |
| `Reason` | `string` | Gateway's classification reason for interception. |
| `CreatedAt` | `DateTime` | When the item entered the queue. |

## Error Handling

The SDK uses a structured exception hierarchy:

```
SynentraException                    (base — all SDK errors)
├── SynentraApiException             (non-2xx HTTP response)
└── SynentraAuthenticationException  (401 / 403 responses)
```

```csharp
try
{
    var policy = await synentra.Policies.GetAsync("unknown-policy", ct);
}
catch (SynentraAuthenticationException ex)
{
    // 401 or 403 — re-authenticate or surface to the user
    Console.WriteLine($"Auth error [{ex.StatusCode}]: {ex.Message}");
}
catch (SynentraApiException ex) when (ex.StatusCode == 404)
{
    Console.WriteLine("Policy not found.");
}
catch (SynentraApiException ex)
{
    // All other non-2xx responses
    Console.WriteLine($"API error [{ex.StatusCode}]: {ex.Message}");

    // Structured payload from the server (when available)
    if (ex.ApiError is { } err)
        Console.WriteLine($"Server detail: {err.Detail}");
}
catch (SynentraException ex)
{
    // SDK-level error (e.g. serialization, configuration)
    Console.WriteLine($"SDK error: {ex.Message}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Request timed out or was cancelled.");
}
```

Set `options.ThrowOnError = false` to suppress automatic exception throwing and handle HTTP errors manually via status codes.

## Dependency Injection

The SDK integrates with `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Http`.

### ASP.NET Core / Generic Host

```csharp
// Program.cs
builder.Services.AddSynentraClient(options =>
{
    options.BaseUrl     = builder.Configuration["Synentra:BaseUrl"]!;
    options.BearerToken = builder.Configuration["Synentra:BearerToken"];
});
```

```csharp
// Inject into any service
public class AgentOrchestrator(ISynentraClient synentra) { ... }
```

### Console Application (manual `ServiceCollection`)

```csharp
var services = new ServiceCollection();
services.AddSynentraClient(opts =>
{
    opts.BaseUrl     = "http://localhost:7080";
    opts.BearerToken = "<jwt>";
});

await using var provider = services.BuildServiceProvider();
var synentra = provider.GetRequiredService<ISynentraClient>();
```

### Registered Services

| Interface | Implementation | Lifetime |
|---|---|---|
| `ISynentraClient` | `SynentraClient` | Transient |
| `ISynentraAgentClient` | `AgentClient` | Typed `HttpClient` |
| `ISynentraPolicyClient` | `PolicyClient` | Typed `HttpClient` |
| `ISynentraHitlClient` | `HitlClient` | Typed `HttpClient` |
| `ISynentraTokenClient` | `TokenClient` | Typed `HttpClient` |

## Advanced Usage

### Per-Request Timeout via `CancellationToken`

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var agents = await synentra.Agents.ListAsync(cancellationToken: cts.Token);
```

### Reacting to Option Changes

```csharp
optionsMonitor.OnChange(opts =>
{
    logger.LogInformation("Synentra BearerToken rotated at {Time}", DateTimeOffset.UtcNow);
});
```

### Background HITL Monitor

Poll the HITL queue on a timer and auto-approve safe requests:

```csharp
public class HitlMonitorService(ISynentraClient synentra, ILogger<HitlMonitorService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var pending = await synentra.Hitl.GetAllPendingAsync(cancellationToken: stoppingToken);

            foreach (var request in pending)
            {
                // Apply your own approval logic here
                logger.LogInformation("Pending HITL: {Id} — {Method} {Url}", request.Id, request.Method, request.Url);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

## Running the Examples

The `examples/` project contains ten annotated runnable examples covering every SDK feature:

| # | Name | What it demonstrates |
|---|---|---|
| 01 | Quick Start | List agents, policies, and HITL queue in a few lines |
| 02 | Token Authentication | Credential exchange, token refresh, auth error handling |
| 03 | Agent Management | Register, list, assign policy, delete agents |
| 04 | Policy Inspection | List policies, drill into rules |
| 05 | HITL Workflow | Pending queue, status inspection, approve/deny |
| 06 | Error Handling | All exception types, `when`-clause patterns |
| 07 | Background HITL Monitor | Polling queue as a hosted background service |
| 08 | Agent Lifecycle | Full create → configure → decommission flow |
| 09 | Advanced Configuration | Runtime options, manual DI, per-request timeouts |
| 10 | Pagination & Bulk | Iterating large result sets efficiently |

```bash
# Make sure the Synentra gateway is running first
cd examples
dotnet run
```

Edit `examples/Program.cs` to configure the gateway URL, Bearer token, and which example to execute.

## Running the Tests

```bash
dotnet test tests/Synentra.Client.UnitTests
```

The test suite uses **xUnit** and covers all HTTP clients, the DI extensions, exception types, model validation, and internal helpers via mock HTTP message handlers — no live gateway required.

## Contributing

1. Fork the repository and create a feature branch.
2. Add or update tests for any changed behaviour.
3. Ensure `dotnet build` and `dotnet test` both pass.
4. Open a pull request against `main`.

Please follow the existing code style (nullable reference types enabled, `required` properties, XML doc comments on public API).

## License

© Synentra. Licensed under the [Apache License, Version 2.0](LICENSE).
