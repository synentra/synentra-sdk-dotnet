using Vectra.Client.Exceptions;
using Vectra.Client.Http;
using Vectra.Client.Models.Common;
using Vectra.Client.Models.Policies;
using Vectra.Client.UnitTests.Helpers;
using System.Net;

namespace Vectra.Client.UnitTests.Http;

public sealed class PolicyClientTests
{
    private static HttpClient CreateClient(MockHttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    [Fact]
    public async Task ListAsync_ReturnsItems_WhenResponseIsSuccess()
    {
        var payload = new PagedResult<PolicySummary>
        {
            Items = [new PolicySummary { PolicyName = "default-policy" }],
            Page = 1,
            PageSize = 25,
            TotalCount = 1
        };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new PolicyClient(CreateClient(handler));

        var result = await sut.ListAsync();

        result.Should().HaveCount(1);
        result[0].PolicyName.Should().Be("default-policy");
    }

    [Fact]
    public async Task ListAsync_SendsCorrectQueryString()
    {
        var payload = new PagedResult<PolicySummary> { Items = [], TotalCount = 0, Page = 3, PageSize = 5 };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new PolicyClient(CreateClient(handler));

        await sut.ListAsync(page: 3, pageSize: 5);

        handler.Requests[0].RequestUri!.Query.Should().Contain("page=3").And.Contain("pageSize=5");
    }

    [Fact]
    public async Task ListAsync_ThrowsVectraApiException_OnServerError()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.ServiceUnavailable);
        var sut = new PolicyClient(CreateClient(handler));

        var act = () => sut.ListAsync();

        await act.Should().ThrowAsync<VectraApiException>();
    }

    [Fact]
    public async Task GetAsync_ReturnsPolicyDetails_WhenFound()
    {
        var payload = new PolicyDetails { Name = "my-policy", Owner = "team-a", Default = PolicyType.Allow };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new PolicyClient(CreateClient(handler));

        var result = await sut.GetAsync("my-policy");

        result.Name.Should().Be("my-policy");
        result.Owner.Should().Be("team-a");
    }

    [Fact]
    public async Task GetAsync_EscapesPolicyNameInUrl()
    {
        var payload = new PolicyDetails { Name = "my policy", Owner = "o" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new PolicyClient(CreateClient(handler));

        await sut.GetAsync("my policy");

        handler.Requests[0].RequestUri!.AbsoluteUri.Should().Contain("my%20policy");
    }

    [Fact]
    public async Task GetAsync_ThrowsArgumentException_WhenPolicyNameIsWhitespace()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new PolicyClient(CreateClient(handler));

        var act = () => sut.GetAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetAsync_ThrowsVectraAuthenticationException_On401()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = new PolicyClient(CreateClient(handler));

        var act = () => sut.GetAsync("some-policy");

        await act.Should().ThrowAsync<VectraAuthenticationException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task GetAsync_ThrowsVectraApiException_WithStructuredError()
    {
        var errorBody = new { message = "Not found", code = "POLICY_NOT_FOUND", statusCode = 404 };
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, errorBody);
        var sut = new PolicyClient(CreateClient(handler));

        var act = () => sut.GetAsync("missing");

        var ex = await act.Should().ThrowAsync<VectraApiException>();
        ex.Which.StatusCode.Should().Be(404);
        ex.Which.ApiError.Should().NotBeNull();
        ex.Which.ApiError!.Code.Should().Be("POLICY_NOT_FOUND");
    }
}
