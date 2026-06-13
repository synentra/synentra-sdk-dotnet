using Synentra.Client.Exceptions;
using Synentra.Client.Http;
using Synentra.Client.Models.Common;
using Synentra.Client.Models.Hitl;
using Synentra.Client.UnitTests.Helpers;
using System.Net;

namespace Synentra.Client.UnitTests.Http;

public sealed class HitlClientTests
{
    private static HttpClient CreateClient(MockHttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    [Fact]
    public async Task GetAllPendingAsync_ReturnsPendingItems()
    {
        var agentId = Guid.NewGuid();
        var payload = new PagedResult<PendingHitlRequest>
        {
            Items =
            [
                new PendingHitlRequest
                {
                    Id = "req-1",
                    Method = "POST",
                    Url = "http://api/action",
                    Reason = "Suspicious action",
                    AgentId = agentId,
                    Timestamp = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                }
            ],
            Page = 1,
            PageSize = 25,
            TotalCount = 1
        };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new HitlClient(CreateClient(handler));

        var result = await sut.GetAllPendingAsync();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("req-1");
    }

    [Fact]
    public async Task GetAllPendingAsync_SendsCorrectQueryString()
    {
        var payload = new PagedResult<PendingHitlRequest> { Items = [], TotalCount = 0, Page = 2, PageSize = 10 };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new HitlClient(CreateClient(handler));

        await sut.GetAllPendingAsync(page: 2, pageSize: 10);

        handler.Requests[0].RequestUri!.Query.Should().Contain("page=2").And.Contain("pageSize=10");
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsHitlStatusResponse()
    {
        var payload = new HitlStatusResponse { Id = "req-1", Status = HitlRequestStatus.Pending };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new HitlClient(CreateClient(handler));

        var result = await sut.GetStatusAsync("req-1");

        result.Id.Should().Be("req-1");
        result.Status.Should().Be(HitlRequestStatus.Pending);
    }

    [Fact]
    public async Task GetStatusAsync_EscapesIdInUrl()
    {
        var payload = new HitlStatusResponse { Id = "req 1", Status = HitlRequestStatus.Approved };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new HitlClient(CreateClient(handler));

        await sut.GetStatusAsync("req 1");

        handler.Requests[0].RequestUri!.AbsoluteUri.Should().Contain("req%201");
    }

    [Fact]
    public async Task GetStatusAsync_ThrowsArgumentException_WhenIdIsEmpty()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        var act = () => sut.GetStatusAsync(string.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ApproveAsync_SendsPostToApproveEndpoint()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        await sut.ApproveAsync("req-1");

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("approve");
    }

    [Fact]
    public async Task ApproveAsync_SendsDecisionComment_WhenProvided()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        await sut.ApproveAsync("req-1", new ReviewDecisionRequest { Comment = "LGTM" });

        var body = await handler.Requests[0].Content!.ReadAsStringAsync();
        body.Should().Contain("LGTM");
    }

    [Fact]
    public async Task ApproveAsync_ThrowsArgumentException_WhenIdIsWhitespace()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        var act = () => sut.ApproveAsync("  ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DenyAsync_SendsPostToDenyEndpoint()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        await sut.DenyAsync("req-1");

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.ToString().Should().Contain("deny");
    }

    [Fact]
    public async Task DenyAsync_ThrowsArgumentException_WhenIdIsEmpty()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new HitlClient(CreateClient(handler));

        var act = () => sut.DenyAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DenyAsync_ThrowsSynentraAuthenticationException_On403()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.Forbidden);
        var sut = new HitlClient(CreateClient(handler));

        var act = () => sut.DenyAsync("req-1");

        await act.Should().ThrowAsync<SynentraAuthenticationException>()
            .Where(e => e.StatusCode == 403);
    }
}
