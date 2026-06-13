# Synentra SDK for .NET

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://github.com/synentra/synentra-sdk-dotnet/blob/main/LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

Official .NET client library for **Synentra** — the Intent-Aware Governance Gateway for Autonomous AI Agents.

Synentra sits between your AI agents and the outside world, enforcing governance policies, intercepting high-risk actions, and routing requests through Human-in-the-Loop (HITL) review when needed.

---

## Installation

```bash
dotnet add package Synentra.Client.Net
```

---

## Quick Start

```csharp
// Register with the DI container (ASP.NET Core / Generic Host)
builder.Services.AddSynentraClient(options =>
{
	options.BaseUrl     = "http://localhost:7080";
	options.BearerToken = "your-jwt-token"; // optional — see Token Authentication
});
```

```csharp
// Inject and use
public class MyService(ISynentraClient synentra)
{
	public async Task RunAsync(CancellationToken ct)
	{
		var agents   = await synentra.Agents.ListAsync(page: 1, pageSize: 10, ct);
		var policies = await synentra.Policies.ListAsync(cancellationToken: ct);
		var pending  = await synentra.Hitl.GetAllPendingAsync(cancellationToken: ct);
	}
}
```

---

## Features

- **Agent Management** — Register, list, assign policies to, and delete AI agents
- **JWT Authentication** — Exchange agent credentials for a scoped Bearer token with a single call
- **Policy Inspection** — List and inspect the governance policies configured in your gateway
- **HITL Review Workflows** — Retrieve pending review requests and approve or deny them programmatically
- **First-class DI support** — `AddSynentraClient(...)` wires up all typed HTTP clients in one line
- **Automatic Bearer token injection** — Set `BearerToken` once; every outgoing request carries the header
- **Structured exceptions** — `SynentraApiException` and `SynentraAuthenticationException` expose the HTTP status code and the server error payload
- **Full cancellation support** — Every async method accepts a `CancellationToken`
- **Pagination** — All list operations accept `page` and `pageSize` parameters

---

## Configuration

| Property | Type | Default | Description |
|---|---|---|---|
| `BaseUrl` | `string` | *(required)* | Base URL of the Synentra gateway |
| `BearerToken` | `string?` | `null` | Static JWT injected as `Authorization: Bearer <token>` |
| `Timeout` | `TimeSpan` | `00:00:30` | Per-request HTTP timeout |
| `ThrowOnError` | `bool` | `true` | Throw `SynentraApiException` on non-2xx responses |

---

## API Reference

```
ISynentraClient
├── .Agents    → ISynentraAgentClient
├── .Policies  → ISynentraPolicyClient
├── .Hitl      → ISynentraHitlClient
└── .Tokens    → ISynentraTokenClient
```

### Agent Management

```csharp
var agents = await synentra.Agents.ListAsync(page: 1, pageSize: 25, ct);

var result = await synentra.Agents.RegisterAsync(new RegisterAgentRequest
{
	Name         = "my-research-agent",
	OwnerId      = "team-backend",
	ClientSecret = "super-secret-value"
}, ct);

await synentra.Agents.AssignPolicyAsync(result.AgentId, new AssignPolicyRequest
{
	PolicyName = "strict-outbound"
}, ct);

await synentra.Agents.DeleteAsync(result.AgentId, ct);
```

### Token Authentication

```csharp
GenerateTokenResult token = await synentra.Tokens.GenerateAsync(new GenerateTokenRequest
{
	AgentId      = agentId,
	ClientSecret = "super-secret-value"
}, ct);
```

### Policy Inspection

```csharp
var policies = await synentra.Policies.ListAsync(page: 1, pageSize: 25, ct);
var details  = await synentra.Policies.GetAsync("strict-outbound", ct);
```

### HITL Workflows

```csharp
var queue = await synentra.Hitl.GetAllPendingAsync(page: 1, pageSize: 25, ct);

// Approve
await synentra.Hitl.ApproveAsync(queue[0].Id, new ReviewDecisionRequest
{
	Comment = "Verified safe by on-call engineer."
}, ct);

// Deny
await synentra.Hitl.DenyAsync(queue[0].Id, new ReviewDecisionRequest
{
	Comment = "Blocked: unexpected data-exfiltration attempt."
}, ct);
```

---

## Error Handling

```csharp
try
{
	var policy = await synentra.Policies.GetAsync("unknown-policy", ct);
}
catch (SynentraAuthenticationException ex)
{
	// 401 or 403
}
catch (SynentraApiException ex) when (ex.StatusCode == 404)
{
	// Not found
}
catch (SynentraApiException ex)
{
	// All other non-2xx responses
}
```

---

## Links

- [Full Documentation & Examples](https://github.com/synentra/synentra-sdk-dotnet)
- [NuGet Package](https://www.nuget.org/packages/Synentra.Client.Net)
- [Product Page](https://synentra.io)
- [Report an Issue](https://github.com/synentra/synentra-sdk-dotnet/issues)
- [Changelog](https://github.com/synentra/synentra-sdk-dotnet/releases)

---

© Synentra. All rights reserved.
