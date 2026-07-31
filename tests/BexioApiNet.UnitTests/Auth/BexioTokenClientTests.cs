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
using System.Web;

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// Unit tests for <see cref="BexioTokenClient" />. The current bexio identity provider only reads
/// token request parameters from the request body, so these tests assert the wire format as much
/// as the response mapping.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BexioTokenClientTests
{
    private const string TokenBody =
        """{"access_token":"access-1","token_type":"Bearer","expires_in":3600,"refresh_token":"refresh-2"}""";

    private static readonly BexioOAuthOptions Options = new()
    {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        Authority = "https://auth.example.local/realms/bexio",
        RedirectUri = "https://app.example.local/callback",
        Scopes = ["accounting", "file"]
    };

    private readonly List<IDisposable> _disposables = [];

    /// <summary>
    /// Disposes the clients and handlers created during a test.
    /// </summary>
    [TearDown]
    public void DisposeClients()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();

        _disposables.Clear();
    }

    /// <summary>
    /// Creates a token client over a recording handler.
    /// </summary>
    private (BexioTokenClient client, QueuedResponseHandler inner) CreateClient(
        HttpStatusCode statusCode = HttpStatusCode.OK, string body = TokenBody, BexioOAuthOptions? options = null)
    {
        var inner = new QueuedResponseHandler().Enqueue(statusCode, body);
        var httpClient = new HttpClient(inner);
        _disposables.Add(httpClient);

        return (new BexioTokenClient(httpClient, options ?? Options), inner);
    }

    /// <summary>
    /// Parses a captured <c>application/x-www-form-urlencoded</c> body.
    /// </summary>
    private static Dictionary<string, string?> ParseForm(string? body)
    {
        var parsed = HttpUtility.ParseQueryString(body ?? string.Empty);
        return parsed.AllKeys.Where(key => key is not null).ToDictionary(key => key!, key => parsed[key]);
    }

    /// <summary>
    /// A refresh must POST to the realm's token endpoint with every parameter in the body — the
    /// new identity provider no longer accepts them as query parameters.
    /// </summary>
    [Test]
    public async Task RefreshTokenAsync_PostsAllParametersInBody()
    {
        var (client, inner) = CreateClient();

        var response = await client.RefreshTokenAsync("refresh-1", TestContext.CurrentContext.CancellationToken);

        var request = inner.Requests.Single();
        var form = ParseForm(request.Body);

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.RequestUri, Is.EqualTo(new Uri("https://auth.example.local/realms/bexio/protocol/openid-connect/token")));
            Assert.That(request.RequestUri!.Query, Is.Empty, "credentials must never appear in the query string");
            Assert.That(form["grant_type"], Is.EqualTo("refresh_token"));
            Assert.That(form["refresh_token"], Is.EqualTo("refresh-1"));
            Assert.That(form["client_id"], Is.EqualTo("client-id"));
            Assert.That(form["client_secret"], Is.EqualTo("client-secret"));
            Assert.That(response.AccessToken, Is.EqualTo("access-1"));
            Assert.That(response.RefreshToken, Is.EqualTo("refresh-2"));
            Assert.That(response.ExpiresIn, Is.EqualTo(3600));
        });
    }

    /// <summary>
    /// The client credentials grant carries the configured scopes as a space separated list.
    /// </summary>
    [Test]
    public async Task ClientCredentialsAsync_SendsGrantAndScopes()
    {
        var (client, inner) = CreateClient();

        await client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken);

        var form = ParseForm(inner.Requests.Single().Body);

        Assert.Multiple(() =>
        {
            Assert.That(form["grant_type"], Is.EqualTo("client_credentials"));
            Assert.That(form["scope"], Is.EqualTo("accounting file"));
        });
    }

    /// <summary>
    /// A public client has no secret, and an empty <c>client_secret</c> parameter would be
    /// rejected — it must be omitted entirely.
    /// </summary>
    [Test]
    public async Task ClientCredentialsAsync_WithoutClientSecret_OmitsParameter()
    {
        var (client, inner) = CreateClient(options: Options with { ClientSecret = null });

        await client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken);

        ParseForm(inner.Requests.Single().Body).ShouldNotContainKey("client_secret");
    }

    /// <summary>
    /// The authorization code exchange sends the redirect URI it was consented with, plus the PKCE
    /// verifier when one was used.
    /// </summary>
    [Test]
    public async Task ExchangeAuthorizationCodeAsync_SendsCodeRedirectUriAndVerifier()
    {
        var (client, inner) = CreateClient();

        await client.ExchangeAuthorizationCodeAsync("auth-code", codeVerifier: "verifier",
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var form = ParseForm(inner.Requests.Single().Body);

        Assert.Multiple(() =>
        {
            Assert.That(form["grant_type"], Is.EqualTo("authorization_code"));
            Assert.That(form["code"], Is.EqualTo("auth-code"));
            Assert.That(form["redirect_uri"], Is.EqualTo("https://app.example.local/callback"));
            Assert.That(form["code_verifier"], Is.EqualTo("verifier"));
        });
    }

    /// <summary>
    /// An OAuth error response is surfaced as a <see cref="BexioAuthenticationException" /> that
    /// keeps the error code, so callers can distinguish a revoked grant from a misconfigured app.
    /// </summary>
    [Test]
    public async Task RefreshTokenAsync_WhenTokenEndpointRejects_ThrowsWithErrorDetails()
    {
        var (client, _) = CreateClient(HttpStatusCode.BadRequest,
            """{"error":"invalid_grant","error_description":"Token is not active"}""");

        var exception = await Should.ThrowAsync<BexioAuthenticationException>(
            () => client.RefreshTokenAsync("refresh-1", TestContext.CurrentContext.CancellationToken));

        Assert.Multiple(() =>
        {
            Assert.That(exception.Error, Is.EqualTo("invalid_grant"));
            Assert.That(exception.ErrorDescription, Is.EqualTo("Token is not active"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(exception.Message, Does.Contain("invalid_grant"));
        });
    }

    /// <summary>
    /// A 2xx response that carries no usable access token is a protocol violation, not a success.
    /// </summary>
    [Test]
    public async Task ClientCredentialsAsync_WhenResponseHasNoAccessToken_Throws()
    {
        var (client, _) = CreateClient(body: """{"token_type":"Bearer"}""");

        await Should.ThrowAsync<BexioAuthenticationException>(
            () => client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken));
    }

    /// <summary>
    /// A non-JSON error body — an HTML page from a proxy, for example — must not surface as a
    /// <c>JsonException</c>.
    /// </summary>
    [Test]
    public async Task ClientCredentialsAsync_WhenErrorBodyIsNotJson_ThrowsAuthenticationException()
    {
        var (client, _) = CreateClient(HttpStatusCode.BadGateway, "<html>bad gateway</html>", Options);

        var exception = await Should.ThrowAsync<BexioAuthenticationException>(
            () => client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken));

        exception.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
    }

    /// <summary>
    /// The DI constructor takes a client per request from the factory, so a long-lived token client
    /// never pins a single handler.
    /// </summary>
    [Test]
    public async Task Constructor_WithClientFactory_UsesAFreshClientPerRequest()
    {
        var inner = new QueuedResponseHandler().Enqueue(HttpStatusCode.OK, TokenBody).Enqueue(HttpStatusCode.OK, TokenBody);
        _disposables.Add(inner);

        var createdClients = 0;
        var client = new BexioTokenClient(() =>
        {
            createdClients++;
            return new HttpClient(inner, disposeHandler: false);
        }, Options);

        await client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken);
        await client.ClientCredentialsAsync(TestContext.CurrentContext.CancellationToken);

        createdClients.ShouldBe(2);
    }
}
