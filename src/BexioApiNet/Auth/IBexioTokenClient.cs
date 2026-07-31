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
    /// Redeems a refresh token for a new access token.
    /// </summary>
    /// <remarks>
    /// The response <em>may</em> carry a rotated <see cref="BexioTokenResponse.RefreshToken" />, in
    /// which case it replaces the one passed in. bexio documents rotation only for the migration
    /// off <c>idp.bexio.com</c>, so neither presence nor absence can be relied on — handle both.
    /// <para>
    /// Scopes cannot be changed here: a refresh keeps the scopes granted at consent time, and
    /// acquiring new ones requires running the authorization code flow again.
    /// </para>
    /// </remarks>
    /// <param name="refreshToken">The current refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    /// <exception cref="BexioAuthenticationException">The token endpoint rejected the request.</exception>
    Task<BexioTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a token with the <c>client_credentials</c> grant. The bexio documentation never
    /// mentions this grant and its permission model has no obvious place for a token without a
    /// user behind it, so treat it as unproven — see <c>doc/analysis/api-doc-discrepancies.md</c>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    /// <exception cref="BexioAuthenticationException">The token endpoint rejected the request.</exception>
    Task<BexioTokenResponse> ClientCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an access or refresh token, for clean teardown when an integration is disconnected.
    /// </summary>
    /// <remarks>
    /// Per RFC 7009 the endpoint answers <c>200</c> for an unknown token just as it does for a
    /// valid one, so a successful call is not evidence that the token existed.
    /// </remarks>
    /// <param name="token">The token to revoke.</param>
    /// <param name="tokenTypeHint">Optional <c>token_type_hint</c>, e.g. <c>refresh_token</c> or <c>access_token</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="BexioAuthenticationException">The revocation endpoint rejected the request.</exception>
    Task RevokeTokenAsync(string token, string? tokenTypeHint = null, CancellationToken cancellationToken = default);
}
