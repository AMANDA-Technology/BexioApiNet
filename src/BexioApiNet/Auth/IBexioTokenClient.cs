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
/// Client for the bexio OpenID Connect token endpoint. All parameters are sent in the request
/// body — the current identity provider does not accept them as query parameters.
/// </summary>
public interface IBexioTokenClient
{
    /// <summary>
    /// Exchanges an authorization code for tokens. Call this once from the consent redirect
    /// handler, then persist <see cref="BexioTokenResponse.RefreshToken" /> in an
    /// <see cref="IBexioRefreshTokenStore" />.
    /// </summary>
    /// <param name="code">Authorization code received on the redirect URI.</param>
    /// <param name="redirectUri">Redirect URI used for the authorization request. Falls back to <see cref="BexioOAuthOptions.RedirectUri" /> when null.</param>
    /// <param name="codeVerifier">PKCE code verifier, when the authorization request used PKCE.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    /// <exception cref="BexioAuthenticationException">The token endpoint rejected the request.</exception>
    Task<BexioTokenResponse> ExchangeAuthorizationCodeAsync(string code, string? redirectUri = null,
        string? codeVerifier = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Redeems a refresh token for a new access token. bexio rotates refresh tokens, so the
    /// returned <see cref="BexioTokenResponse.RefreshToken" /> replaces the one passed in.
    /// </summary>
    /// <param name="refreshToken">The current refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    /// <exception cref="BexioAuthenticationException">The token endpoint rejected the request.</exception>
    Task<BexioTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a token with the <c>client_credentials</c> grant. Whether a given bexio app may
    /// use this grant depends on its registration; see <c>doc/analysis/api-doc-discrepancies.md</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    /// <exception cref="BexioAuthenticationException">The token endpoint rejected the request.</exception>
    Task<BexioTokenResponse> ClientCredentialsAsync(CancellationToken cancellationToken = default);
}
