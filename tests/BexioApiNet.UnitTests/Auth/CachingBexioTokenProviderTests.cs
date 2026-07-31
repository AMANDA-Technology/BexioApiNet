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

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// Unit tests for the caching behaviour shared by all OIDC token providers, exercised through
/// <see cref="ClientCredentialsBexioTokenProvider" />: expiry with a clock skew margin,
/// single-flight renewal under concurrency, and explicit invalidation.
/// </summary>
[TestFixture]
[Category("Unit")]
public class CachingBexioTokenProviderTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly BexioOAuthOptions Options = new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        ClockSkew = TimeSpan.FromSeconds(60)
    };

    /// <summary>
    /// Builds a token endpoint response with the given token and lifetime.
    /// </summary>
    private static BexioTokenResponse Token(string accessToken, int expiresIn = 3600)
        => new() { AccessToken = accessToken, ExpiresIn = expiresIn, TokenType = "Bearer" };

    /// <summary>
    /// The first call has nothing cached and must hit the token endpoint.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_OnFirstCall_RequestsToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"));

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        var token = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        token.ShouldBe("token-1");
        await tokenClient.Received(1).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// While the cached token is comfortably inside its lifetime, no further token request is made.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WithinLifetime_ReusesCachedToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"));

        var time = new ManualTimeProvider(Start);
        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, time);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(3500));
        var second = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        second.ShouldBe("token-1");
        await tokenClient.Received(1).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The clock skew margin makes a token stale before it actually expires, so a request is never
    /// sent with a token that dies in flight. With a 3600s lifetime and a 60s skew, the token is
    /// renewed from second 3540 onwards.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WithinClockSkewOfExpiry_RequestsNewToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"), Token("token-2"));

        var time = new ManualTimeProvider(Start);
        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, time);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(3570));
        var second = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        second.ShouldBe("token-2");
        await tokenClient.Received(2).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Once the lifetime is over, the provider mints a new token.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_AfterExpiry_RequestsNewToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"), Token("token-2"));

        var time = new ManualTimeProvider(Start);
        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, time);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(4000));
        var second = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        second.ShouldBe("token-2");
        await tokenClient.Received(2).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Concurrent callers must not stampede the token endpoint: the first one performs the request
    /// and everybody else waits for and reuses its result.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WithConcurrentCallers_IssuesSingleTokenRequest()
    {
        const int callerCount = 16;

        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<BexioTokenResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            Interlocked.Increment(ref requestCount);
            entered.TrySetResult();
            return release.Task;
        });

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        var callers = Enumerable
            .Range(0, callerCount)
            // ReSharper disable once AccessToDisposedClosure
            .Select(_ => Task.Run(() => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken)))
            .ToArray();

        await entered.Task;
        release.SetResult(Token("token-1"));
        var tokens = await Task.WhenAll(callers);

        Assert.Multiple(() =>
        {
            Assert.That(requestCount, Is.EqualTo(1), "concurrent callers must share a single token request");
            Assert.That(tokens, Has.Length.EqualTo(callerCount));
            Assert.That(tokens, Is.All.EqualTo("token-1"));
        });
    }

    /// <summary>
    /// Invalidation drops the cached token so the next caller re-mints, which is what the
    /// <c>401</c> retry path relies on.
    /// </summary>
    [Test]
    public async Task Invalidate_DiscardsCachedToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"), Token("token-2"));

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        provider.Invalidate("token-1");
        var second = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        second.ShouldBe("token-2");
        await tokenClient.Received(2).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Invalidation names the token that was rejected, and is ignored when the cache has already
    /// moved on. This is what stops a burst of concurrent <c>401</c>s — every one of them holding
    /// the same stale token — from each discarding the token its predecessor just minted and
    /// requesting one of its own.
    /// </summary>
    [Test]
    public async Task Invalidate_WithAlreadyReplacedToken_KeepsCachedToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"), Token("token-2"));

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        // First rejection renews, the rest still name the stale token and must be no-ops.
        provider.Invalidate("token-1");
        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        provider.Invalidate("token-1");
        provider.Invalidate("token-1");

        var current = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        current.ShouldBe("token-2");
        await tokenClient.Received(2).ClientCredentialsAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A response without <c>expires_in</c> deserializes to a zero lifetime. Treating that as
    /// "already expired" would make the cache permanently unusable and turn every API call into a
    /// token request, so it falls back to the configured lifetime instead.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenResponseHasNoExpiry_UsesFallbackLifetime()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(Token("token-1", expiresIn: 0), Token("token-2"));

        var time = new ManualTimeProvider(Start);
        var options = Options with { FallbackTokenLifetime = TimeSpan.FromMinutes(5), ClockSkew = TimeSpan.FromSeconds(60) };
        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, options, time);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        time.Advance(TimeSpan.FromMinutes(3));
        var cached = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        time.Advance(TimeSpan.FromMinutes(3));
        var renewed = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(cached, Is.EqualTo("token-1"), "the fallback lifetime must make the token cacheable");
            Assert.That(renewed, Is.EqualTo("token-2"), "the fallback lifetime must still expire");
        });
    }

    /// <summary>
    /// A token whose lifetime is shorter than the clock skew would be stale the instant it was
    /// issued. The skew is clamped so the token stays usable for part of its life instead.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenLifetimeIsShorterThanClockSkew_StillCachesTheToken()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(Token("token-1", expiresIn: 30), Token("token-2"));

        var time = new ManualTimeProvider(Start);
        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, time);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(10));
        var cached = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        time.Advance(TimeSpan.FromSeconds(10));
        var renewed = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(cached, Is.EqualTo("token-1"), "half the 30s lifetime must remain usable");
            Assert.That(renewed, Is.EqualTo("token-2"), "the clamped window must still close before the real expiry");
        });
    }

    /// <summary>
    /// A failing token request must not poison the cache — the next call retries instead of
    /// serving a stale or empty token.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenTokenRequestFails_PropagatesAndCachesNothing()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<BexioTokenResponse>(new BexioAuthenticationException("boom")),
                _ => Task.FromResult(Token("token-1")));

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        await Should.ThrowAsync<BexioAuthenticationException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));

        var recovered = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        recovered.ShouldBe("token-1");
    }

    /// <summary>
    /// A token response without an access token is a protocol violation and must be rejected
    /// rather than cached as an empty bearer value.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenAccessTokenIsEmpty_Throws()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>())
            .Returns(new BexioTokenResponse { AccessToken = "  ", ExpiresIn = 3600 });

        using var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));

        await Should.ThrowAsync<BexioAuthenticationException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));
    }

    /// <summary>
    /// Using a disposed provider is a programming error and surfaces as
    /// <see cref="ObjectDisposedException" /> rather than a semaphore failure.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_AfterDispose_Throws()
    {
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.ClientCredentialsAsync(Arg.Any<CancellationToken>()).Returns(Token("token-1"));

        var provider = new ClientCredentialsBexioTokenProvider(tokenClient, Options, new ManualTimeProvider(Start));
        provider.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));
    }
}
