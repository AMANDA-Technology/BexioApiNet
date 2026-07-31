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
/// Constants for the bexio OpenID Connect identity provider.
/// <see href="https://docs.bexio.com/#section/Authentication">Authentication</see>
/// </summary>
public static class BexioAuthDefaults
{
    /// <summary>
    /// Issuer of the bexio Keycloak realm. Discovery document:
    /// <c>{Authority}/.well-known/openid-configuration</c>.
    /// </summary>
    public const string Authority = "https://auth.bexio.com/realms/bexio";

    /// <summary>
    /// Path of the token endpoint, relative to the authority.
    /// </summary>
    public const string TokenEndpointPath = "protocol/openid-connect/token";

    /// <summary>
    /// Path of the authorization endpoint, relative to the authority.
    /// </summary>
    public const string AuthorizationEndpointPath = "protocol/openid-connect/auth";

    /// <summary>
    /// Path of the token revocation endpoint, relative to the authority.
    /// </summary>
    public const string RevocationEndpointPath = "protocol/openid-connect/revoke";

    /// <summary>
    /// Authorization header scheme used for bexio API requests.
    /// </summary>
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// Scope requesting a refresh token alongside the access token. Required for unattended
    /// integrations built on the authorization code flow.
    /// </summary>
    public const string OfflineAccessScope = "offline_access";

    /// <summary>
    /// Name of the <see cref="HttpClient" /> used for token endpoint calls when the token client
    /// is resolved through <c>IHttpClientFactory</c>.
    /// </summary>
    public const string TokenHttpClientName = "BexioApiNet.Auth";

    /// <summary>
    /// Default safety margin subtracted from the token expiry, so a token is renewed before it
    /// actually expires. Covers clock drift between this host and the identity provider.
    /// </summary>
    public static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Conservative lifetime assumed for a token whose response carries no <c>expires_in</c>. Short
    /// enough that guessing wrong costs little — an early expiry surfaces as a <c>401</c> and is
    /// renewed and replayed.
    /// </summary>
    public static readonly TimeSpan FallbackTokenLifetime = TimeSpan.FromMinutes(5);
}
