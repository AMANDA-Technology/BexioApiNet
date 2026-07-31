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
    /// Gets a currently valid access token, renewing it if the cached one is expired or missing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The access token to send in the <c>Authorization</c> header.</returns>
    /// <exception cref="BexioAuthenticationException">The token could not be obtained.</exception>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Discards the cached token so the next <see cref="GetAccessTokenAsync" /> call obtains a
    /// fresh one. Called when bexio rejects a request with <c>401</c>, which can happen before
    /// the advertised expiry if the token was revoked.
    /// </summary>
    void Invalidate();
}
