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

using System.Net;
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.AspNetCore;
using BexioApiNet.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BexioApiNet.IntegrationTests.Auth;

/// <summary>
/// Offline integration tests for the OIDC pipeline: a real service collection, a real
/// <see cref="BexioConnectionHandler" /> and a real token client, all pointed at a local
/// <see cref="WireMockServer" /> that plays both the bexio identity provider and the API.
/// </summary>
[TestFixture]
[Category("Integration")]
public sealed class OAuthPipelineTests
{
    private const string TokenPath = "/realms/bexio/protocol/openid-connect/token";
    private const string AccountsPath = "/2.0/accounts";

    private WireMockServer _server = null!;

    /// <summary>
    /// Starts the stub server standing in for both auth.bexio.com and api.bexio.com.
    /// </summary>
    [SetUp]
    public void StartServer() => _server = WireMockServer.Start();

    /// <summary>
    /// Stops the stub server.
    /// </summary>
    [TearDown]
    public void StopServer()
    {
        _server.Stop();
        _server.Dispose();
    }

    /// <summary>
    /// Builds a provider wired to the stub server with the client credentials flow.
    /// </summary>
    private ServiceProvider CreateProvider() => new ServiceCollection()
        .AddBexioServicesWithClientCredentials(
            new BexioConfiguration
            {
                BaseUri = _server.Url! + "/",
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            },
            new BexioOAuthOptions
            {
                ClientId = "client-id",
                ClientSecret = "client-secret",
                Authority = _server.Url! + "/realms/bexio",
                Scopes = ["accounting"]
            })
        .BuildServiceProvider(validateScopes: true);

    /// <summary>
    /// Stubs the token endpoint to hand out the given access tokens in order, repeating the last
    /// one once the list is exhausted.
    /// </summary>
    private void StubTokenEndpoint(params string[] accessTokens)
    {
        var issued = 0;

        _server
            .Given(Request.Create().WithPath(TokenPath).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(_ =>
                {
                    var accessToken = accessTokens[Math.Min(Interlocked.Increment(ref issued) - 1, accessTokens.Length - 1)];
                    return $$"""{"access_token":"{{accessToken}}","token_type":"Bearer","expires_in":3600}""";
                }));
    }

    /// <summary>
    /// Counts the requests the stub server received for the given path.
    /// </summary>
    private int CountRequests(string path)
        => _server.LogEntries.Count(entry => entry.RequestMessage?.AbsolutePath == path);

    /// <summary>
    /// Reads the <c>Authorization</c> header of the last request to the given path.
    /// </summary>
    private string? LastAuthorizationHeader(string path)
    {
        var headers = _server.LogEntries
            .Last(entry => entry.RequestMessage?.AbsolutePath == path)
            .RequestMessage!.Headers;

        return headers is not null && headers.TryGetValue("Authorization", out var values)
            ? values.ToString()
            : null;
    }

    /// <summary>
    /// The full DI path must mint a token and present it as a bearer token on the API call,
    /// without any <c>Authorization</c> header being pinned to the typed client.
    /// </summary>
    [Test]
    public async Task ClientCredentialsRegistration_AuthenticatesApiCallWithMintedToken()
    {
        StubTokenEndpoint("access-1");
        _server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IBexioApiClient>();

        var result = await client.Accounts.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(LastAuthorizationHeader(AccountsPath), Is.EqualTo("Bearer access-1"));
        });
    }

    /// <summary>
    /// Two API calls share one minted token: the provider caches it, so the token endpoint is hit
    /// once regardless of API traffic.
    /// </summary>
    [Test]
    public async Task ClientCredentialsRegistration_ReusesTokenAcrossRequests()
    {
        StubTokenEndpoint("access-1");
        _server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IBexioApiClient>();

        await client.Accounts.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);
        await client.Accounts.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(CountRequests(TokenPath), Is.EqualTo(1), "the cached token is reused across API calls");
    }

    /// <summary>
    /// A revoked token surfaces as <c>401</c>. The pipeline must re-mint and replay the request
    /// once, so the caller still receives the successful result.
    /// </summary>
    [Test]
    public async Task ClientCredentialsRegistration_WhenApiReturnsUnauthorized_RemintsAndRetries()
    {
        StubTokenEndpoint("access-1", "access-2");

        _server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet()
                .WithHeader("Authorization", "Bearer access-1"))
            .RespondWith(Response.Create().WithStatusCode((int)HttpStatusCode.Unauthorized));

        _server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet()
                .WithHeader("Authorization", "Bearer access-2"))
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IBexioApiClient>();

        var result = await client.Accounts.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var apiRequests = CountRequests(AccountsPath);
        var tokenRequests = CountRequests(TokenPath);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True, "the replayed request must succeed");
            Assert.That(apiRequests, Is.EqualTo(2), "the request is replayed exactly once");
            Assert.That(tokenRequests, Is.EqualTo(2), "the rejected token is re-minted once");
        });
    }

    /// <summary>
    /// A token endpoint failure cannot be expressed as an <c>ApiResult</c>, because no API request
    /// was ever made. It surfaces as a <see cref="BexioAuthenticationException" /> carrying the
    /// OAuth error code.
    /// </summary>
    [Test]
    public async Task ClientCredentialsRegistration_WhenTokenEndpointRejects_ThrowsAuthenticationException()
    {
        _server
            .Given(Request.Create().WithPath(TokenPath).UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"error":"unauthorized_client","error_description":"Client not allowed"}"""));

        await using var provider = CreateProvider();
        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IBexioApiClient>();

        var exception = Assert.ThrowsAsync<BexioAuthenticationException>(
            () => client.Accounts.Get(cancellationToken: TestContext.CurrentContext.CancellationToken));

        Assert.That(exception!.Error, Is.EqualTo("unauthorized_client"));
        await Task.CompletedTask;
    }
}
