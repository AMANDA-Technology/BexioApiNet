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

using BexioApiNet.Auth;
using BexioApiNet.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BexioApiNet.AspNetCore;

/// <summary>
///     Registration of the bexio OpenID Connect authentication flows.
/// </summary>
public static class BexioOAuthServiceCollection
{
    /// <summary>
    ///     Adds all Bexio services, authenticating with an OIDC access token obtained from a stored refresh
    ///     token. This is the unattended path for an integration that completed the authorization code flow
    ///     with the <see cref="BexioAuthDefaults.OfflineAccessScope" /> scope.
    /// </summary>
    /// <remarks>
    ///     The host must register its own <see cref="IBexioRefreshTokenStore" />; any lifetime works, it is
    ///     resolved from a fresh scope for each token renewal.
    /// </remarks>
    /// <param name="services">Service collection to register Bexio services into.</param>
    /// <param name="bexioConfiguration">Bexio configuration (base URI, accept header format). The static token is ignored.</param>
    /// <param name="oauthOptions">Client registration and endpoint settings.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddBexioServicesWithRefreshToken(this IServiceCollection services,
        IBexioConfiguration bexioConfiguration, BexioOAuthOptions oauthOptions)
    {
        services.AddBexioTokenClient(oauthOptions);

        return services.AddBexioServices(bexioConfiguration, provider => new RefreshTokenBexioTokenProvider(
            provider.GetRequiredService<IBexioTokenClient>(),
            new ScopedBexioRefreshTokenStore(provider.GetRequiredService<IServiceScopeFactory>()),
            oauthOptions));
    }

    /// <summary>
    ///     Adds all Bexio services, authenticating with an OIDC access token obtained through the
    ///     <c>client_credentials</c> grant. Needs no consent and no refresh token storage, but only works if
    ///     the bexio app registration permits the grant.
    /// </summary>
    /// <param name="services">Service collection to register Bexio services into.</param>
    /// <param name="bexioConfiguration">Bexio configuration (base URI, accept header format). The static token is ignored.</param>
    /// <param name="oauthOptions">Client registration and endpoint settings.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddBexioServicesWithClientCredentials(this IServiceCollection services,
        IBexioConfiguration bexioConfiguration, BexioOAuthOptions oauthOptions)
    {
        services.AddBexioTokenClient(oauthOptions);

        return services.AddBexioServices(bexioConfiguration, provider => new ClientCredentialsBexioTokenProvider(
            provider.GetRequiredService<IBexioTokenClient>(),
            oauthOptions));
    }

    /// <summary>
    ///     Registers <see cref="IBexioTokenClient" /> on its own named <see cref="HttpClient" />. Called by the
    ///     flow-specific overloads; register it directly when the consent redirect handler needs to exchange an
    ///     authorization code without the rest of the client being OIDC-backed.
    /// </summary>
    /// <param name="services">Service collection to register the token client into.</param>
    /// <param name="oauthOptions">Client registration and endpoint settings.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddBexioTokenClient(this IServiceCollection services,
        BexioOAuthOptions oauthOptions)
    {
        ArgumentNullException.ThrowIfNull(oauthOptions);

        services.AddHttpClient(BexioAuthDefaults.TokenHttpClientName);

        services.TryAddSingleton<IBexioTokenClient>(provider =>
        {
            // A client per token request keeps this singleton from pinning one handler for the
            // process lifetime, which is what IHttpClientFactory rotation exists to prevent.
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            return new BexioTokenClient(
                () => httpClientFactory.CreateClient(BexioAuthDefaults.TokenHttpClientName),
                oauthOptions);
        });

        return services;
    }
}
