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

using System.Web;

namespace BexioApiNet.Auth;

/// <summary>
/// Builds the bexio consent URL for the authorization code flow. Where the user is redirected to
/// it, and how <c>state</c> is generated and validated, stays with the host application.
/// </summary>
public static class BexioAuthorizeUrlBuilder
{
    /// <summary>
    /// Builds the URL the user is sent to in order to grant the app access to their bexio company.
    /// </summary>
    /// <param name="options">Client registration and endpoint settings. <see cref="BexioOAuthOptions.Scopes" /> must contain <see cref="BexioAuthDefaults.OfflineAccessScope" /> for an unattended integration.</param>
    /// <param name="state">Opaque anti-forgery value echoed back on the redirect. Verify it there.</param>
    /// <param name="redirectUri">Redirect URI to use. Falls back to <see cref="BexioOAuthOptions.RedirectUri" /> when null.</param>
    /// <param name="codeChallenge">PKCE code challenge derived from the code verifier.</param>
    /// <param name="codeChallengeMethod">PKCE challenge method. Only relevant together with <paramref name="codeChallenge" />.</param>
    /// <returns>The absolute authorization URL.</returns>
    public static Uri Build(BexioOAuthOptions options, string state, string? redirectUri = null,
        string? codeChallenge = null, string codeChallengeMethod = "S256")
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        var effectiveRedirectUri = redirectUri ?? options.RedirectUri;
        ArgumentException.ThrowIfNullOrWhiteSpace(effectiveRedirectUri);

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"] = options.ClientId;
        query["redirect_uri"] = effectiveRedirectUri;
        query["state"] = state;

        if (options.Scopes.Count > 0)
            query["scope"] = string.Join(' ', options.Scopes);

        if (!string.IsNullOrWhiteSpace(codeChallenge))
        {
            query["code_challenge"] = codeChallenge;
            query["code_challenge_method"] = codeChallengeMethod;
        }

        return new UriBuilder(options.AuthorizationEndpoint) { Query = query.ToString() }.Uri;
    }
}
