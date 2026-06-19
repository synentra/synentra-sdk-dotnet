using Synentra.Client.Abstractions;
using Synentra.Client.Models.Agents;
using Synentra.Client.Models.Common;
using Synentra.Client.Models.Hitl;
using Synentra.Client.Models.Policies;
using Synentra.Client.Models.Tokens;

namespace Synentra.Client.UnitTests;

public sealed class SynentraClientTests
{
    [Fact]
    public void Constructor_AssignsSubClients()
    {
        var agents = new StubAgentClient();
        var policies = new StubPolicyClient();
        var hitl = new StubHitlClient();
        var tokens = new StubTokenClient();
        var proxy = new StubProxyClient();

        var sut = new SynentraClient(agents, policies, hitl, tokens, proxy);

        sut.Agents.Should().BeSameAs(agents);
        sut.Policies.Should().BeSameAs(policies);
        sut.Hitl.Should().BeSameAs(hitl);
        sut.Tokens.Should().BeSameAs(tokens);
        sut.Proxy.Should().BeSameAs(proxy);
    }

    [Fact]
    public void ImplementsISynentraClient()
    {
        var sut = new SynentraClient(
            new StubAgentClient(),
            new StubPolicyClient(),
            new StubHitlClient(),
            new StubTokenClient(),
            new StubProxyClient());

        sut.Should().BeAssignableTo<ISynentraClient>();
    }

    private sealed class StubAgentClient : ISynentraAgentClient
    {
        public Task<IReadOnlyList<AgentSummary>> ListAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AgentSummary>>([]);
        public Task<RegisterAgentResult> RegisterAsync(RegisterAgentRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new RegisterAgentResult());
        public Task AssignPolicyAsync(Guid agentId, AssignPolicyRequest request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task DeleteAsync(Guid agentId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task LiftQuarantineAsync(Guid agentId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubPolicyClient : ISynentraPolicyClient
    {
        public Task<IReadOnlyList<PolicySummary>> ListAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PolicySummary>>([]);
        public Task<PolicyDetails> GetAsync(string policyName, CancellationToken cancellationToken = default)
            => Task.FromResult(new PolicyDetails());
    }

    private sealed class StubHitlClient : ISynentraHitlClient
    {
        public Task<IReadOnlyList<PendingHitlRequest>> GetAllPendingAsync(int page = 1, int pageSize = 25, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PendingHitlRequest>>([]);
        public Task<HitlStatusResponse> GetStatusAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(new HitlStatusResponse());
        public Task ApproveAsync(string id, ReviewDecisionRequest? decision = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task DenyAsync(string id, ReviewDecisionRequest? decision = null, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class StubTokenClient : ISynentraTokenClient
    {
        public Task<GenerateTokenResult> GenerateAsync(GenerateTokenRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new GenerateTokenResult());
    }

    private sealed class StubProxyClient : ISynentraProxyClient
    {
        public Task<System.Text.Json.Nodes.JsonNode?> ExecuteAsync(
            string path, 
            string method, 
            System.Text.Json.Nodes.JsonNode payload, 
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<System.Text.Json.Nodes.JsonNode?>(new System.Text.Json.Nodes.JsonObject());
    }
}
