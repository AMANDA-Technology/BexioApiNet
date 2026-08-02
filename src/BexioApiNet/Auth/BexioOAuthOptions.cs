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
/// Client registration and endpoint settings for the bexio OpenID Connect provider. Obtain
/// <see cref="ClientId" /> and <see cref="ClientSecret" /> by registering an app at
/// <see href="https://developer.bexio.com">developer.bexio.com</see>.
/// </summary>
public sealed record BexioOAuthOptions
{
    /// <summary>
    /// Client id of the registered bexio app.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Client secret of the registered bexio app. Required for confidential clients; may be null
    /// for a public client using PKCE.
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Issuer of the bexio realm. Defaults to <see cref="BexioAuthDefaults.Authority" />.
    /// </summary>
    public string Authority { get; init; } = BexioAuthDefaults.Authority;

    /// <summary>
    /// How the client authenticates at the token endpoint. Defaults to
    /// <see cref="BexioClientAuthenticationMethod.ClientSecretPost" />; switch to
    /// <see cref="BexioClientAuthenticationMethod.ClientSecretBasic" /> if the token endpoint
    /// answers <c>401</c> for an otherwise correct request.
    /// </summary>
    public BexioClientAuthenticationMethod ClientAuthenticationMethod { get; init; } = BexioClientAuthenticationMethod.ClientSecretPost;

    /// <summary>
    /// Scopes to request, as bexio spells them. For an unattended authorization code integration
    /// this must include <see cref="BexioAuthDefaults.OfflineAccessScope" /> so a refresh token is
    /// issued.
    /// </summary>
    /// <remarks>
    /// Free-form strings by design: the realm advertises far more scopes than the documented table
    /// lists, and read access is implied by the matching write scope (<c>contact_edit</c> grants
    /// <c>contact_show</c>). Scopes are fixed at consent time — they cannot be changed on refresh.
    /// </remarks>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Redirect URI registered with the bexio app. Used by the authorization code flow.
    /// </summary>
    public string? RedirectUri { get; init; }

    /// <summary>
    /// Safety margin subtracted from the token expiry when deciding whether a cached token is
    /// still usable. Defaults to <see cref="BexioAuthDefaults.ClockSkew" />.
    /// </summary>
    public TimeSpan ClockSkew { get; init; } = BexioAuthDefaults.ClockSkew;

    /// <summary>
    /// How long a token is cached when the token endpoint reports no <c>expires_in</c>. Defaults to
    /// <see cref="BexioAuthDefaults.FallbackTokenLifetime" />.
    /// </summary>
    /// <remarks>
    /// bexio documents no access token lifetime, so this is only a floor against the degenerate
    /// case: without it a missing <c>expires_in</c> would mean a token request per API call.
    /// </remarks>
    public TimeSpan FallbackTokenLifetime { get; init; } = BexioAuthDefaults.FallbackTokenLifetime;

    /// <summary>
    /// Static headers added to every request to the identity provider, on top of the headers the library sets
    /// itself. Intended for caller identification and diagnostics — an outbound tag naming the calling
    /// application, environment and build, for example. Defaults to <c>null</c>, which adds nothing.
    /// </summary>
    /// <remarks>
    /// The token client is configured from these options rather than from
    /// <see cref="Interfaces.IBexioConfiguration" />, so it needs its own property — the API client's
    /// <c>IBexioConfiguration.DefaultRequestHeaders</c> cannot reach it. Applied once per
    /// <see cref="HttpClient" />, so only static values belong here; reserved names (<c>Authorization</c>,
    /// <c>Accept</c>, <c>Host</c>) and malformed entries are silently skipped — see
    /// <see cref="Configuration.HttpClientExtension.ApplyDefaultRequestHeaders" /> for the full contract.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? DefaultRequestHeaders { get; init; }

    /// <summary>
    /// Absolute URI of the token endpoint, derived from <see cref="Authority" />.
    /// </summary>
    public Uri TokenEndpoint => BuildEndpoint(BexioAuthDefaults.TokenEndpointPath);

    /// <summary>
    /// Absolute URI of the authorization endpoint, derived from <see cref="Authority" />.
    /// </summary>
    public Uri AuthorizationEndpoint => BuildEndpoint(BexioAuthDefaults.AuthorizationEndpointPath);

    /// <summary>
    /// Absolute URI of the token revocation endpoint, derived from <see cref="Authority" />.
    /// </summary>
    public Uri RevocationEndpoint => BuildEndpoint(BexioAuthDefaults.RevocationEndpointPath);

    /// <summary>
    /// Combines the authority with an endpoint path, tolerating a missing trailing slash.
    /// </summary>
    /// <param name="path">Endpoint path relative to the authority.</param>
    private Uri BuildEndpoint(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Authority);
        return new Uri(new Uri(Authority.EndsWith('/') ? Authority : Authority + '/'), path);
    }
}
