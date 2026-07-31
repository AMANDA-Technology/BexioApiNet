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
/// Supplies the bearer token for outgoing bexio API requests. Resolved per request by
/// <see cref="BexioAuthDelegatingHandler" />, so a short-lived OIDC access token can be swapped
/// without recreating the <see cref="HttpClient" />.
/// </summary>
public interface IBexioTokenProvider
{
    /// <summary>
    /// Whether this provider can produce a different token than the one it just handed out. False
    /// for a pre-issued token, which no amount of retrying will change.
    /// </summary>
    /// <remarks>
    /// <see cref="BexioAuthDelegatingHandler" /> uses this to skip both the <c>401</c> retry and
    /// the request buffering that retry would need.
    /// </remarks>
    bool CanRenew { get; }

    /// <summary>
    /// Gets a currently valid access token, renewing it if the cached one is expired or missing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The access token to send in the <c>Authorization</c> header.</returns>
    /// <exception cref="BexioAuthenticationException">The token could not be obtained.</exception>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards the cached token <b>only if it is still</b> <paramref name="accessToken" />, so the
    /// next <see cref="GetAccessTokenAsync" /> call obtains a fresh one. Called when bexio rejects
    /// a request with <c>401</c>, which can happen before the advertised expiry if the token was
    /// revoked.
    /// </summary>
    /// <remarks>
    /// The comparison is what keeps concurrent rejections from stampeding the token endpoint: when
    /// N in-flight requests all fail on the same stale token, the first invalidation renews and the
    /// rest find a token that is no longer the one they complained about, so they leave it alone.
    /// Unconditional invalidation would have each one discard the token its predecessor just
    /// minted, turning N rejections into N token requests.
    /// </remarks>
    /// <param name="accessToken">The token that was rejected.</param>
    void Invalidate(string accessToken);
}
