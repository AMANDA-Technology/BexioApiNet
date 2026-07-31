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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// Unit tests for the DI registration: the pre-existing static token overloads keep working
/// unchanged, the <c>Authorization</c> header is no longer pinned to the typed client, and the
/// OIDC overloads register a caching provider whose refresh token store is resolved per scope.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BexioServiceCollectionAuthTests
{
    private const string BaseUri = "https://api.example.local/";
    private const string TypedClientName = nameof(IBexioConnectionHandler);

    private static readonly BexioOAuthOptions OAuthOptions = new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Scopes = ["accounting", BexioAuthDefaults.OfflineAccessScope]
    };

    /// <summary>
    /// Builds a configuration for the static token path.
    /// </summary>
    private static BexioConfiguration CreateConfiguration(string jwtToken = "static-token") => new()
    {
        BaseUri = BaseUri,
        JwtToken = jwtToken,
        AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
    };

    /// <summary>
    /// The original two-argument overload still registers a fully usable client.
    /// </summary>
    [Test]
    public void AddBexioServices_WithBaseUriAndToken_ResolvesApiClient()
    {
        var services = new ServiceCollection();
        services.AddBexioServices(BaseUri, "static-token");

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.Multiple(() =>
        {
            Assert.That(scope.ServiceProvider.GetRequiredService<IBexioApiClient>(), Is.Not.Null);
            Assert.That(scope.ServiceProvider.GetRequiredService<IBexioConnectionHandler>(), Is.Not.Null);
        });
    }

    /// <summary>
    /// The static token path is implemented on top of the token provider abstraction, which is
    /// what keeps a Personal Access Token working unchanged.
    /// </summary>
    [Test]
    public async Task AddBexioServices_WithStaticToken_RegistersStaticTokenProvider()
    {
        var services = new ServiceCollection();
        services.AddBexioServices(CreateConfiguration());

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var tokenProvider = provider.GetRequiredService<IBexioTokenProvider>();
        var token = await tokenProvider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(tokenProvider, Is.InstanceOf<StaticBexioTokenProvider>());
            Assert.That(token, Is.EqualTo("static-token"));
        });
    }

    /// <summary>
    /// The typed client must not carry a pinned <c>Authorization</c> header — that is exactly what
    /// prevented a token from being swapped without recreating the client.
    /// </summary>
    [Test]
    public void AddBexioServices_DoesNotPinAuthorizationHeaderOnTypedClient()
    {
        var services = new ServiceCollection();
        services.AddBexioServices(CreateConfiguration());

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient(TypedClientName);

        Assert.Multiple(() =>
        {
            Assert.That(client.DefaultRequestHeaders.Authorization, Is.Null);
            Assert.That(client.BaseAddress, Is.EqualTo(new Uri(BaseUri)));
            Assert.That(client.DefaultRequestHeaders.Accept.ToString(), Is.EqualTo(ApiAcceptHeaders.JsonFormatted));
        });
    }

    /// <summary>
    /// A missing static token is a configuration error and is reported at registration time.
    /// </summary>
    [Test]
    public void AddBexioServices_WithoutStaticToken_Throws()
    {
        var services = new ServiceCollection();

        Assert.That(() => services.AddBexioServices(CreateConfiguration(jwtToken: null!)),
            Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// The client credentials overload registers a caching provider and the token client it needs.
    /// </summary>
    [Test]
    public void AddBexioServicesWithClientCredentials_RegistersClientCredentialsProvider()
    {
        var services = new ServiceCollection();
        services.AddBexioServicesWithClientCredentials(CreateConfiguration(), OAuthOptions);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IBexioTokenProvider>(),
                Is.InstanceOf<ClientCredentialsBexioTokenProvider>());
            Assert.That(provider.GetRequiredService<IBexioTokenClient>(), Is.InstanceOf<BexioTokenClient>());
        });
    }

    /// <summary>
    /// The refresh token provider is a singleton, but the host's store is typically scoped (a
    /// database context, for example). Resolving it must therefore go through a fresh scope —
    /// under <c>validateScopes</c> a captive dependency would throw here.
    /// </summary>
    [Test]
    public async Task AddBexioServicesWithRefreshToken_ResolvesScopedStorePerRenewal()
    {
        var store = new RecordingRefreshTokenStore();
        var tokenClient = Substitute.For<IBexioTokenClient>();
        tokenClient.RefreshTokenAsync("refresh-1", Arg.Any<CancellationToken>())
            .Returns(new BexioTokenResponse { AccessToken = "access-1", ExpiresIn = 3600, RefreshToken = "refresh-2" });

        var services = new ServiceCollection();
        services.AddSingleton(tokenClient);
        services.AddScoped<IBexioRefreshTokenStore>(_ => store);
        services.AddBexioServicesWithRefreshToken(CreateConfiguration(), OAuthOptions);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var tokenProvider = provider.GetRequiredService<IBexioTokenProvider>();

        var token = await tokenProvider.GetAccessTokenAsync(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(tokenProvider, Is.InstanceOf<RefreshTokenBexioTokenProvider>());
            Assert.That(token, Is.EqualTo("access-1"));
            Assert.That(store.StoredRefreshToken, Is.EqualTo("refresh-2"));
        });
    }

    /// <summary>
    /// The token request carries the client secret. Following a redirect would replay it to the
    /// host named by the redirect, so the token client must refuse redirects just like the API
    /// client does.
    /// </summary>
    [Test]
    public void AddBexioTokenClient_DisablesAutoRedirect()
    {
        var services = new ServiceCollection();
        services.AddBexioTokenClient(OAuthOptions);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        var handler = provider
            .GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
            .Get(BexioAuthDefaults.TokenHttpClientName)
            .HttpMessageHandlerBuilderActions
            .Aggregate(new TestHttpMessageHandlerBuilder(), (builder, configure) =>
            {
                configure(builder);
                return builder;
            })
            .PrimaryHandler;

        Assert.That(handler, Is.InstanceOf<HttpClientHandler>()
            .And.Property(nameof(HttpClientHandler.AllowAutoRedirect)).False);
    }

    /// <summary>
    /// Minimal <see cref="HttpMessageHandlerBuilder" /> used to run the registered handler
    /// configuration and inspect the primary handler it produces.
    /// </summary>
    private sealed class TestHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        /// <inheritdoc />
        public override string? Name { get; set; }

        /// <inheritdoc />
        public override HttpMessageHandler PrimaryHandler { get; set; } = new HttpClientHandler();

        /// <inheritdoc />
        public override IList<DelegatingHandler> AdditionalHandlers { get; } = [];

        /// <inheritdoc />
        public override HttpMessageHandler Build() => PrimaryHandler;
    }

    /// <summary>
    /// The token client can be registered on its own, for a consent endpoint that only needs to
    /// exchange an authorization code.
    /// </summary>
    [Test]
    public void AddBexioTokenClient_RegistersTokenClientOnly()
    {
        var services = new ServiceCollection();
        services.AddBexioTokenClient(OAuthOptions);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.Multiple(() =>
        {
            Assert.That(provider.GetRequiredService<IBexioTokenClient>(), Is.InstanceOf<BexioTokenClient>());
            Assert.That(provider.GetService<IBexioTokenProvider>(), Is.Null);
        });
    }

    /// <summary>
    /// In-memory refresh token store used to observe what the provider persisted.
    /// </summary>
    private sealed class RecordingRefreshTokenStore : IBexioRefreshTokenStore
    {
        /// <summary>
        /// The last token handed to <see cref="StoreRefreshTokenAsync" />.
        /// </summary>
        public string? StoredRefreshToken { get; private set; }

        /// <inheritdoc />
        public Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(StoredRefreshToken ?? "refresh-1");

        /// <inheritdoc />
        public Task StoreRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            StoredRefreshToken = refreshToken;
            return Task.CompletedTask;
        }
    }
}
