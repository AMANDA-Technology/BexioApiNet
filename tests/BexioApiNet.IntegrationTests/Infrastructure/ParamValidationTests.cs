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
using BexioApiNet.Models;

namespace BexioApiNet.IntegrationTests.Infrastructure;

/// <summary>
/// Integration tests covering argument validation in the <see cref="BexioConnectionHandler"/>
/// constructors and query-string encoding performed by <see cref="BexioConnectionHandler.GetAsync{TResult}"/>.
/// </summary>
public sealed class ParamValidationTests : IntegrationTestBase
{
    private const string AccountsPath = "/2.0/accounts";
    private const string AccountsRequestPath = "2.0/accounts";

    /// <summary>
    /// A null <c>BaseUri</c> must be rejected by the owning constructor — it delegates to
    /// <see cref="ArgumentException.ThrowIfNullOrWhiteSpace(string?, string?)"/> which throws
    /// an <see cref="ArgumentNullException"/> (a subtype of <see cref="ArgumentException"/>).
    /// </summary>
    [Test]
    public void Constructor_WithNullBaseUri_ThrowsArgumentException()
    {
        var configuration = new BexioConfiguration
        {
            BaseUri = null!,
            JwtToken = "token",
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        Assert.That(() => new BexioConnectionHandler(configuration), Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// An empty <c>BaseUri</c> fails the same null-or-whitespace guard and surfaces as an
    /// <see cref="ArgumentException"/>.
    /// </summary>
    [Test]
    public void Constructor_WithEmptyBaseUri_ThrowsArgumentException()
    {
        var configuration = new BexioConfiguration
        {
            BaseUri = string.Empty,
            JwtToken = "token",
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        Assert.That(() => new BexioConnectionHandler(configuration), Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// A null <c>JwtToken</c> is rejected the same way as a null <c>BaseUri</c>, since the
    /// bearer header cannot be omitted from Bexio requests.
    /// </summary>
    [Test]
    public void Constructor_WithNullJwtToken_ThrowsArgumentException()
    {
        var configuration = new BexioConfiguration
        {
            BaseUri = "https://api.bexio.com/",
            JwtToken = null!,
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        Assert.That(() => new BexioConnectionHandler(configuration), Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// The DI-friendly overload validates the injected <see cref="HttpClient"/> up-front so
    /// callers see a clear <see cref="ArgumentNullException"/> rather than a null-reference
    /// failure deeper in the pipeline.
    /// </summary>
    [Test]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        var configuration = new BexioConfiguration
        {
            BaseUri = "https://api.bexio.com/",
            JwtToken = "token",
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        Assert.That(() => new BexioConnectionHandler(null!, configuration), Throws.ArgumentNullException);
    }

    /// <summary>
    /// The owning constructor normalizes a <c>BaseUri</c> missing a trailing slash so that
    /// combining it with a request path does not drop the last URI segment.
    /// </summary>
    [Test]
    public async Task Constructor_WithBaseUriMissingTrailingSlash_AppendsSlash()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var baseUriWithoutSlash = Server.Url!;
        Assert.That(baseUriWithoutSlash, Does.Not.EndWith("/"), "pre-condition: WireMock URL has no trailing slash");

        var configuration = new BexioConfiguration
        {
            BaseUri = baseUriWithoutSlash,
            JwtToken = FakeJwtToken,
            AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
        };

        using var handler = new BexioConnectionHandler(configuration);

        var result = await handler.GetAsync<List<object>>(
            AccountsRequestPath,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(configuration.BaseUri, Does.EndWith("/"));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountsPath));
        });
    }

    /// <summary>
    /// Query parameter values containing whitespace or reserved URI characters must be
    /// percent-encoded in the outgoing request so the server receives them intact.
    /// </summary>
    [Test]
    public async Task GetAsync_WithQueryParameter_CorrectlyEncodesSpecialChars()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var queryParam = new QueryParameter(new Dictionary<string, object>
        {
            ["search"] = "hello world & friends"
        });

        var result = await ConnectionHandler.GetAsync<List<object>>(
            AccountsRequestPath,
            queryParam,
            TestContext.CurrentContext.CancellationToken);

        var rawQuery = Server.LogEntries.Last().RequestMessage!.RawQuery ?? string.Empty;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(rawQuery, Does.Not.Contain(" "), "raw query must not contain unencoded spaces");
            Assert.That(rawQuery, Does.Contain("search="), "search key must be present in the query string");
            Assert.That(rawQuery, Does.Contain("%26").Or.Contain("%26+"), "ampersand character must be percent-encoded");
        });
    }
}
