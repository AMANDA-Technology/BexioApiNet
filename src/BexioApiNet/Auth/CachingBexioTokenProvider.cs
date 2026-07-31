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

    /// <summary>
    /// The cached token, or null when none has been acquired or it was invalidated. Replaced as a
    /// whole so readers never observe a token paired with the wrong expiry.
    /// </summary>
    private CachedToken? _cached;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingBexioTokenProvider" /> class.
    /// </summary>
    /// <param name="clockSkew">Margin subtracted from the token expiry before it is considered stale.</param>
    /// <param name="timeProvider">Time source. Defaults to <see cref="TimeProvider.System" />.</param>
    protected CachingBexioTokenProvider(TimeSpan clockSkew, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(clockSkew, TimeSpan.Zero);

        _clockSkew = clockSkew;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

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
        ObjectDisposedException.ThrowIf(_disposed, this);

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

            Volatile.Write(ref _cached, new CachedToken(
                response.AccessToken,
                _timeProvider.GetUtcNow().AddSeconds(response.ExpiresIn)));

            return response.AccessToken;
        }
        finally
        {
            _renewalGate.Release();
        }
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        Volatile.Write(ref _cached, null);
    }

    /// <summary>
    /// Reads the cached token when it is still valid for at least the configured clock skew.
    /// </summary>
    /// <param name="accessToken">The cached access token when the method returns true.</param>
    /// <returns>True when a usable token was cached.</returns>
    private bool TryGetCachedToken(out string accessToken)
    {
        var cached = Volatile.Read(ref _cached);

        if (cached is not null && _timeProvider.GetUtcNow() < cached.ExpiresAt - _clockSkew)
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
        if (_disposed)
            return;

        _disposed = true;
        Volatile.Write(ref _cached, null);
        _renewalGate.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// An access token together with the instant it stops being valid.
    /// </summary>
    /// <param name="AccessToken">The access token.</param>
    /// <param name="ExpiresAt">Absolute expiry, without the clock skew margin applied.</param>
    private sealed record CachedToken(string AccessToken, DateTimeOffset ExpiresAt);
}
