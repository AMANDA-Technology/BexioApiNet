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
/// Unit tests for <see cref="RefreshTokenBexioTokenProvider" />, focused on bexio's refresh token
/// rotation contract: the replacement token must reach the store before the refresh counts as
/// successful, because bexio has already invalidated the previous one.
/// </summary>
[TestFixture]
[Category("Unit")]
public class RefreshTokenBexioTokenProviderTests
{
    private static readonly BexioOAuthOptions Options = new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Scopes = ["accounting", BexioAuthDefaults.OfflineAccessScope]
    };

    /// <summary>
    /// Builds a refresh response carrying a rotated refresh token.
    /// </summary>
    private static BexioTokenResponse Token(string accessToken, string? refreshToken)
        => new() { AccessToken = accessToken, ExpiresIn = 3600, RefreshToken = refreshToken };

    /// <summary>
    /// Creates a provider over the supplied doubles.
    /// </summary>
    private static RefreshTokenBexioTokenProvider CreateProvider(IBexioTokenClient tokenClient, IBexioRefreshTokenStore store)
        => new(tokenClient, store, Options, new ManualTimeProvider(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero)));

    /// <summary>
    /// The refresh token from the store is the one redeemed, and the rotated replacement returned
    /// by bexio is written back.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_RedeemsStoredTokenAndPersistsRotatedOne()
    {
        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns("refresh-1");

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>()).Returns(Token("access-1", "refresh-2"));

        using var provider = CreateProvider(tokenClient, store);

        var token = await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        token.ShouldBe("access-1");
        await store.Received(1).StoreRefreshTokenAsync("refresh-2", Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The store write must complete before the access token is handed out. If it did not, a host
    /// could act on a token whose refresh credential was never persisted — and both tokens would
    /// be lost on the next restart.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_DoesNotReturnBeforeRotatedTokenIsPersisted()
    {
        var writeCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns("refresh-1");
        store.StoreRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_ => writeCompleted.Task);

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>()).Returns(Token("access-1", "refresh-2"));

        using var provider = CreateProvider(tokenClient, store);

        var pending = provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        pending.IsCompleted.ShouldBeFalse("the refresh must block on the store write");

        writeCompleted.SetResult();

        (await pending).ShouldBe("access-1");
    }

    /// <summary>
    /// A failed store write fails the whole refresh. Nothing is cached, so the next call retries
    /// the acquisition instead of silently running on an unpersisted credential.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenStoreWriteFails_PropagatesAndCachesNothing()
    {
        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns("refresh-1");
        store.StoreRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException(new InvalidOperationException("database unavailable")));

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>()).Returns(Token("access-1", "refresh-2"));

        using var provider = CreateProvider(tokenClient, store);

        await Should.ThrowAsync<InvalidOperationException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));

        await Should.ThrowAsync<InvalidOperationException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));

        await tokenClient.Received(2).RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When bexio returns the same refresh token, there is nothing to rotate and the store is left
    /// untouched.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenRefreshTokenUnchanged_DoesNotWriteToStore()
    {
        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns("refresh-1");

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>()).Returns(Token("access-1", "refresh-1"));

        using var provider = CreateProvider(tokenClient, store);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        await store.DidNotReceive().StoreRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A response without a refresh token leaves the stored one in place — overwriting it with
    /// nothing would break the next renewal.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WhenResponseHasNoRefreshToken_DoesNotWriteToStore()
    {
        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns("refresh-1");

        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>()).Returns(Token("access-1", null));

        using var provider = CreateProvider(tokenClient, store);

        await provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        await store.DidNotReceive().StoreRefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Without a stored refresh token the application has never been consented to, which must be
    /// reported as an authentication failure rather than a null reference.
    /// </summary>
    [Test]
    public async Task GetAccessTokenAsync_WithoutStoredRefreshToken_ThrowsBexioAuthenticationException()
    {
        var store = Substitute.For<IBexioRefreshTokenStore>();
        store.GetRefreshTokenAsync(Arg.Any<CancellationToken>()).Returns((string?)null);

        var tokenClient = Substitute.For<IBexioTokenClient>();

        using var provider = CreateProvider(tokenClient, store);

        var exception = await Should.ThrowAsync<BexioAuthenticationException>(
            () => provider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken));

        exception.Message.ShouldContain(BexioAuthDefaults.OfflineAccessScope);
        await tokenClient.DidNotReceive().RefreshTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
