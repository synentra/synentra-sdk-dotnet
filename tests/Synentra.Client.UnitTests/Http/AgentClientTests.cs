using Synentra.Client.Configuration;
using Synentra.Client.Exceptions;
using Synentra.Client.Http;
using Synentra.Client.Models.Agents;
using Synentra.Client.Models.Common;
using Synentra.Client.UnitTests.Helpers;
using System.Net;
using System.Text.Json;

namespace Synentra.Client.UnitTests.Http;

public sealed class AgentClientTests
{
    private static HttpClient CreateClient(MockHttpMessageHandler handler, string baseUrl = "http://localhost/")
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        return client;
    }

    [Fact]
    public async Task ListAsync_ReturnsItems_WhenResponseIsSuccess()
    {
        var expected = new PagedResult<AgentSummary>
        {
            Items = [new AgentSummary { AgentId = Guid.NewGuid(), Name = "agent1", Status = AgentStatus.Active }],
            Page = 1,
            PageSize = 25,
            TotalCount = 1
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, expected);
        var sut = new AgentClient(CreateClient(handler));

        var result = await sut.ListAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("agent1");
    }

    [Fact]
    public async Task ListAsync_SendsCorrectQueryString()
    {
        var payload = new PagedResult<AgentSummary> { Items = [], TotalCount = 0, Page = 2, PageSize = 10 };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new AgentClient(CreateClient(handler));

        await sut.ListAsync(page: 2, pageSize: 10);

        handler.Requests[0].RequestUri!.Query.Should().Contain("page=2").And.Contain("pageSize=10");
    }

    [Fact]
    public async Task ListAsync_ThrowsSynentraApiException_OnServerError()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "oops");
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.ListAsync();

        await act.Should().ThrowAsync<SynentraApiException>();
    }

    [Fact]
    public async Task RegisterAsync_ReturnsAgentId_OnSuccess()
    {
        var agentId = Guid.NewGuid();
        var payload = new RegisterAgentResult { AgentId = agentId };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new AgentClient(CreateClient(handler));

        var result = await sut.RegisterAsync(new RegisterAgentRequest
        {
            Name = "MyAgent",
            OwnerId = "owner1",
            ClientSecret = "secret"
        });

        result.AgentId.Should().Be(agentId);
    }

    [Fact]
    public async Task RegisterAsync_PostsToAgentsEndpoint()
    {
        var payload = new RegisterAgentResult { AgentId = Guid.NewGuid() };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new AgentClient(CreateClient(handler));

        await sut.RegisterAsync(new RegisterAgentRequest
        {
            Name = "A",
            OwnerId = "O",
            ClientSecret = "S"
        });

        handler.Requests[0].RequestUri!.AbsolutePath.Should().EndWith("Agents");
        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.RegisterAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterAsync_ThrowsSynentraAuthenticationException_On401()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.RegisterAsync(new RegisterAgentRequest { Name = "A", OwnerId = "O", ClientSecret = "S" });

        await act.Should().ThrowAsync<SynentraAuthenticationException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task AssignPolicyAsync_SendsPutRequest()
    {
        var agentId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var sut = new AgentClient(CreateClient(handler));

        await sut.AssignPolicyAsync(agentId, new AssignPolicyRequest { PolicyName = "policy1" });

        handler.Requests[0].Method.Should().Be(HttpMethod.Put);
        handler.Requests[0].RequestUri!.ToString().Should().Contain(agentId.ToString());
    }

    [Fact]
    public async Task AssignPolicyAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.AssignPolicyAsync(Guid.NewGuid(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AssignPolicyAsync_ThrowsSynentraApiException_On500()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "err");
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.AssignPolicyAsync(Guid.NewGuid(), new AssignPolicyRequest { PolicyName = "p" });

        await act.Should().ThrowAsync<SynentraApiException>();
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest()
    {
        var agentId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var sut = new AgentClient(CreateClient(handler));

        await sut.DeleteAsync(agentId);

        handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
        handler.Requests[0].RequestUri!.ToString().Should().Contain(agentId.ToString());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsSynentraAuthenticationException_On403()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.Forbidden);
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<SynentraAuthenticationException>()
            .Where(e => e.StatusCode == 403);
    }

    [Fact]
    public async Task LiftQuarantineAsync_SendsPostRequest()
    {
        var agentId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var sut = new AgentClient(CreateClient(handler));

        await sut.LiftQuarantineAsync(agentId);

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.ToString().Should().Contain(agentId.ToString());
        handler.Requests[0].RequestUri!.ToString().Should().Contain("lift-quarantine");
    }

    [Fact]
    public async Task LiftQuarantineAsync_ThrowsSynentraApiException_On500()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "err");
        var sut = new AgentClient(CreateClient(handler));

        var act = () => sut.LiftQuarantineAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<SynentraApiException>();
    }
}
