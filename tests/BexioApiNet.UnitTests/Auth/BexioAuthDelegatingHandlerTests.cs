/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Net;
using System.Text;

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// Unit tests for <see cref="BexioAuthDelegatingHandler" />: the bearer token is resolved for
/// every single request instead of being pinned to the client, and a <c>401</c> triggers exactly
/// one invalidate-and-replay cycle.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BexioAuthDelegatingHandlerTests
{
    private static readonly Uri RequestUri = new("https://api.example.local/2.0/accounts");

    /// <summary>
    /// Wires a client whose only handler is the auth handler under test.
    /// </summary>
    private static HttpClient CreateClient(IBexioTokenProvider tokenProvider, QueuedResponseHandler inner)
        => new(new BexioAuthDelegatingHandler(tokenProvider, inner));

    /// <summary>
    /// The token from the provider is sent as a bearer token.
    /// </summary>
    [Test]
    public async Task SendAsync_SetsBearerTokenFromProvider()
    {
        var inner = new QueuedResponseHandler().Enqueue(HttpStatusCode.OK, "[]");
        using var client = CreateClient(new StaticBexioTokenProvider("token-1"), inner);

        await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);

        inner.Requests.Single().Authorization.ShouldBe("token-1");
    }

    /// <summary>
    /// The token is read per request, not captured once. This is the whole point of the handler:
    /// a renewed OIDC access token takes effect without recreating the client.
    /// </summary>
    [Test]
    public async Task SendAsync_ResolvesTokenForEveryRequest()
    {
        var tokenProvider = new SequencedTokenProvider("token-1", "token-2");
        var inner = new QueuedResponseHandler().Enqueue(HttpStatusCode.OK, "[]").Enqueue(HttpStatusCode.OK, "[]");
        using var client = CreateClient(tokenProvider, inner);

        await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);
        tokenProvider.Invalidate("token-1");
        await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(inner.Requests[0].Authorization, Is.EqualTo("token-1"));
            Assert.That(inner.Requests[1].Authorization, Is.EqualTo("token-2"));
        });
    }

    /// <summary>
    /// A <c>401</c> means the token was rejected — possibly revoked before its advertised expiry.
    /// The handler drops it and replays the request once with a fresh one.
    /// </summary>
    [Test]
    public async Task SendAsync_WhenUnauthorized_InvalidatesAndRetriesOnceWithNewToken()
    {
        var tokenProvider = new SequencedTokenProvider("token-1", "token-2");
        var inner = new QueuedResponseHandler()
            .Enqueue(HttpStatusCode.Unauthorized, """{"message":"expired"}""")
            .Enqueue(HttpStatusCode.OK, "[]");
        using var client = CreateClient(tokenProvider, inner);

        var response = await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(inner.Requests, Has.Count.EqualTo(2));
            Assert.That(inner.Requests[0].Authorization, Is.EqualTo("token-1"));
            Assert.That(inner.Requests[1].Authorization, Is.EqualTo("token-2"));
            Assert.That(tokenProvider.InvalidateCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// The retry happens at most once. A second <c>401</c> is returned to the caller rather than
    /// turning into a renewal loop against the token endpoint.
    /// </summary>
    [Test]
    public async Task SendAsync_WhenUnauthorizedTwice_RetriesOnlyOnce()
    {
        var tokenProvider = new SequencedTokenProvider("token-1", "token-2", "token-3");
        var inner = new QueuedResponseHandler()
            .Enqueue(HttpStatusCode.Unauthorized)
            .Enqueue(HttpStatusCode.Unauthorized);
        using var client = CreateClient(tokenProvider, inner);

        var response = await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(inner.Requests, Has.Count.EqualTo(2));
            Assert.That(tokenProvider.InvalidateCount, Is.EqualTo(1));
        });
    }

    /// <summary>
    /// A static Personal Access Token cannot be re-minted, so replaying the request with the very
    /// same token would only produce a second <c>401</c>. The retry — and the request buffering it
    /// would have needed — is skipped.
    /// </summary>
    [Test]
    public async Task SendAsync_WithNonRenewableProvider_DoesNotRetry()
    {
        var inner = new QueuedResponseHandler().Enqueue(HttpStatusCode.Unauthorized);
        using var client = CreateClient(new StaticBexioTokenProvider("token-1"), inner);

        var response = await client.GetAsync(RequestUri, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(inner.Requests, Has.Count.EqualTo(1));
        });
    }

    /// <summary>
    /// The replayed request must carry the original method, URI, headers and body — a retry that
    /// drops the payload would silently turn a create into a no-op.
    /// </summary>
    [Test]
    public async Task SendAsync_WhenUnauthorized_ReplaysMethodUriHeadersAndBody()
    {
        const string body = """{"name":"test"}""";

        var inner = new QueuedResponseHandler()
            .Enqueue(HttpStatusCode.Unauthorized)
            .Enqueue(HttpStatusCode.OK, "{}");
        using var client = CreateClient(new SequencedTokenProvider("token-1", "token-2"), inner);

        using var request = new HttpRequestMessage(HttpMethod.Post, RequestUri)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Correlation-Id", "abc-123");

        await client.SendAsync(request, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(inner.Requests, Has.Count.EqualTo(2));
            Assert.That(inner.Requests[1].Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(inner.Requests[1].RequestUri, Is.EqualTo(RequestUri));
            Assert.That(inner.Requests[1].Body, Is.EqualTo(body));
            Assert.That(inner.Requests[1].Body, Is.EqualTo(inner.Requests[0].Body));
            Assert.That(inner.Requests[1].Headers["X-Correlation-Id"], Is.EqualTo(new[] { "abc-123" }));
        });
    }
}
