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
    /// Scopes to request. For an unattended authorization code integration this must include
    /// <see cref="BexioAuthDefaults.OfflineAccessScope" /> so a refresh token is issued.
    /// </summary>
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
    /// Absolute URI of the token endpoint, derived from <see cref="Authority" />.
    /// </summary>
    public Uri TokenEndpoint => BuildEndpoint(BexioAuthDefaults.TokenEndpointPath);

    /// <summary>
    /// Absolute URI of the authorization endpoint, derived from <see cref="Authority" />.
    /// </summary>
    public Uri AuthorizationEndpoint => BuildEndpoint(BexioAuthDefaults.AuthorizationEndpointPath);

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
