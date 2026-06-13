using Vectra.Client.Exceptions;
using Vectra.Client.Http;
using Vectra.Client.Models.Tokens;
using Vectra.Client.UnitTests.Helpers;
using System.Net;

namespace Vectra.Client.UnitTests.Http;

public sealed class TokenClientTests
{
    private static HttpClient CreateClient(MockHttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("http://localhost/") };

    [Fact]
    public async Task GenerateAsync_ReturnsAccessToken_OnSuccess()
    {
        var payload = new GenerateTokenResult { AccessToken = "jwt-abc" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new TokenClient(CreateClient(handler));

        var result = await sut.GenerateAsync(new GenerateTokenRequest
        {
            AgentId = Guid.NewGuid(),
            ClientSecret = "secret"
        });

        result.AccessToken.Should().Be("jwt-abc");
    }

    [Fact]
    public async Task GenerateAsync_PostsToTokensEndpoint()
    {
        var payload = new GenerateTokenResult { AccessToken = "jwt" };
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, payload);
        var sut = new TokenClient(CreateClient(handler));

        await sut.GenerateAsync(new GenerateTokenRequest { AgentId = Guid.NewGuid(), ClientSecret = "s" });

        handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        handler.Requests[0].RequestUri!.AbsolutePath.Should().EndWith("Tokens");
    }

    [Fact]
    public async Task GenerateAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK);
        var sut = new TokenClient(CreateClient(handler));

        var act = () => sut.GenerateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GenerateAsync_ThrowsVectraAuthenticationException_On401()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.Unauthorized);
        var sut = new TokenClient(CreateClient(handler));

        var act = () => sut.GenerateAsync(new GenerateTokenRequest { AgentId = Guid.NewGuid(), ClientSecret = "bad" });

        await act.Should().ThrowAsync<VectraAuthenticationException>()
            .Where(e => e.StatusCode == 401);
    }

    [Fact]
    public async Task GenerateAsync_ThrowsVectraApiException_OnStructuredError()
    {
        var errorBody = new { message = "Invalid secret", code = "INVALID_SECRET", statusCode = 400 };
        var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, errorBody);
        var sut = new TokenClient(CreateClient(handler));

        var act = () => sut.GenerateAsync(new GenerateTokenRequest { AgentId = Guid.NewGuid(), ClientSecret = "wrong" });

        var ex = await act.Should().ThrowAsync<VectraApiException>();
        ex.Which.StatusCode.Should().Be(400);
        ex.Which.ApiError!.Code.Should().Be("INVALID_SECRET");
    }
}
