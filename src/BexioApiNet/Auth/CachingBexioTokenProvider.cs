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

namespace BexioApiNet.Auth;

/// <summary>
/// Base class for token providers that cache an access token until it expires. Adds two things
/// on top of <see cref="AcquireTokenAsync" />:
/// <list type="bullet">
///     <item>expiry awareness with a clock skew margin, so a token is renewed slightly early;</item>
///     <item>single-flight renewal — concurrent callers issue one token request and share its result.</item>
/// </list>
/// </summary>
public abstract class CachingBexioTokenProvider : IBexioTokenProvider, IDisposable
{
    /// <summary>
    /// Serializes token renewal so N concurrent callers trigger exactly one token request.
    /// </summary>
    private readonly SemaphoreSlim _renewalGate = new(1, 1);

    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _clockSkew;
    private readonly TimeSpan _fallbackTokenLifetime;

    /// <summary>
    /// The cached token, or null when none has been acquired or it was invalidated. Replaced as a
    /// whole so readers never observe a token paired with the wrong expiry.
    /// </summary>
    private CachedToken? _cached;

    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingBexioTokenProvider" /> class.
    /// </summary>
    /// <param name="options">Client registration and endpoint settings, read for the caching margins.</param>
    /// <param name="timeProvider">Time source. Defaults to <see cref="TimeProvider.System" />.</param>
    protected CachingBexioTokenProvider(BexioOAuthOptions options, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.ClockSkew, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.FallbackTokenLifetime, TimeSpan.Zero);

        _clockSkew = options.ClockSkew;
        _fallbackTokenLifetime = options.FallbackTokenLifetime;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public bool CanRenew => true;

    /// <summary>
    /// Obtains a new token from the identity provider. Implementations must complete every side
    /// effect that the caller depends on — notably persisting a rotated refresh token — before
    /// returning, because the returned token is only cached once this task completes successfully.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The freshly issued tokens.</returns>
    protected abstract Task<BexioTokenResponse> AcquireTokenAsync(CancellationToken cancellationToken);

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

        if (TryGetCachedToken(out var cached))
            return cached;

        await _renewalGate.WaitAsync(cancellationToken);

        try
        {
            // A caller that queued behind the renewal takes the token the first one just acquired.
            if (TryGetCachedToken(out cached))
                return cached;

            var response = await AcquireTokenAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(response.AccessToken))
                throw new BexioAuthenticationException("The bexio token endpoint returned an empty access token.");

            Volatile.Write(ref _cached, new CachedToken(response.AccessToken, CalculateUsableUntil(response.ExpiresIn)));

            return response.AccessToken;
        }
        finally
        {
            _renewalGate.Release();
        }
    }

    /// <inheritdoc />
    public void Invalidate(string accessToken)
    {
        var cached = Volatile.Read(ref _cached);

        // Drop the cache only if it still holds the rejected token. Another caller may already have
        // renewed it, and discarding that fresh token would send every concurrent 401 to the token
        // endpoint for a token of its own.
        if (cached is not null && string.Equals(cached.AccessToken, accessToken, StringComparison.Ordinal))
            Interlocked.CompareExchange(ref _cached, null, cached);
    }

    /// <summary>
    /// Works out how long a freshly issued token may be served from the cache.
    /// </summary>
    /// <remarks>
    /// Two degenerate cases would otherwise make the cache permanently unusable and turn every API
    /// call into a token request: a response without <c>expires_in</c>, and a lifetime shorter than
    /// the clock skew. The first falls back to a configured lifetime, the second halves the skew.
    /// Both are safe to guess at because a token that dies early surfaces as a <c>401</c>, which
    /// <see cref="BexioAuthDelegatingHandler" /> resolves by renewing and replaying.
    /// </remarks>
    /// <param name="expiresIn">Lifetime in seconds as reported by the token endpoint.</param>
    /// <returns>The instant after which the cached token must not be served.</returns>
    private DateTimeOffset CalculateUsableUntil(int expiresIn)
    {
        var lifetime = expiresIn > 0 ? TimeSpan.FromSeconds(expiresIn) : _fallbackTokenLifetime;
        var skew = _clockSkew < lifetime ? _clockSkew : lifetime / 2;

        return _timeProvider.GetUtcNow() + lifetime - skew;
    }

    /// <summary>
    /// Reads the cached token while it is still inside its usable window.
    /// </summary>
    /// <param name="accessToken">The cached access token when the method returns true.</param>
    /// <returns>True when a usable token was cached.</returns>
    private bool TryGetCachedToken(out string accessToken)
    {
        var cached = Volatile.Read(ref _cached);

        if (cached is not null && _timeProvider.GetUtcNow() < cached.UsableUntil)
        {
            accessToken = cached.AccessToken;
            return true;
        }

        accessToken = string.Empty;
        return false;
    }

    /// <summary>
    /// Releases the renewal gate and drops the cached token.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        Volatile.Write(ref _cached, null);
        _renewalGate.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// An access token together with the instant it stops being served from the cache.
    /// </summary>
    /// <param name="AccessToken">The access token.</param>
    /// <param name="UsableUntil">Expiry with the clock skew margin already subtracted.</param>
    private sealed record CachedToken(string AccessToken, DateTimeOffset UsableUntil);
}
