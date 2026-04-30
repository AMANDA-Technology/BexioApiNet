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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests for <see cref="LanguageService" /> against WireMock stubs. The Bexio
///     v2 languages endpoint is read-only (list + search) under <c>/2.0/language</c>. Each test
///     verifies HTTP verb, request URL, query string composition, and response deserialization
///     against a JSON payload that matches the OpenAPI <c>Language</c> schema exactly.
/// </summary>
public sealed class LanguageServiceIntegrationTests : IntegrationTestBase
{
    private const string BasePath = "/2.0/language";
    private const string SearchPath = $"{BasePath}/search";

    private const string LanguageResponse = """
                                            {
                                                "id": 1,
                                                "name": "German",
                                                "decimal_point": ".",
                                                "thousands_separator": "'",
                                                "date_format_id": 1,
                                                "date_format": "d.m.Y",
                                                "iso_639_1": "de"
                                            }
                                            """;

    /// <summary>
    ///     <c>Get()</c> issues a <c>GET</c> at <c>/2.0/language</c> and deserializes the
    ///     full language entry — including optional formatting metadata — into the
    ///     <see cref="BexioApiNet.Abstractions.Models.MasterData.Languages.Language" /> record.
    /// </summary>
    [Test]
    public async Task LanguageService_Get_SendsGetRequest_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{LanguageResponse}]"));

        var service = new LanguageService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            var language = result.Data![0];
            Assert.That(language.Id, Is.EqualTo(1));
            Assert.That(language.Name, Is.EqualTo("German"));
            Assert.That(language.DecimalPoint, Is.EqualTo("."));
            Assert.That(language.ThousandsSeparator, Is.EqualTo("'"));
            Assert.That(language.DateFormatId, Is.EqualTo(1));
            Assert.That(language.DateFormat, Is.EqualTo("d.m.Y"));
            Assert.That(language.Iso6391, Is.EqualTo("de"));
        });
    }

    /// <summary>
    ///     <c>Get</c> with a populated <see cref="QueryParameterLanguage" /> renders the
    ///     <c>limit</c> and <c>offset</c> values onto the request URI as expected by the
    ///     Bexio OpenAPI spec.
    /// </summary>
    [Test]
    public async Task LanguageService_Get_WithQueryParameter_RendersLimitAndOffsetOnUri()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new LanguageService(ConnectionHandler);

        await service.Get(new QueryParameterLanguage(50, 25),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.RawQuery, Does.Contain("limit=50"));
            Assert.That(request.RawQuery, Does.Contain("offset=25"));
        });
    }

    /// <summary>
    ///     <c>Search</c> issues a <c>POST</c> at <c>/2.0/language/search</c> with the supplied
    ///     <see cref="SearchCriteria" /> list serialized as the JSON body, and deserializes the
    ///     response array into <see cref="BexioApiNet.Abstractions.Models.MasterData.Languages.Language" />.
    /// </summary>
    [Test]
    public async Task LanguageService_Search_SendsPostRequest_ToSearchPath()
    {
        Server
            .Given(Request.Create().WithPath(SearchPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{LanguageResponse}]"));

        var service = new LanguageService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "German", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SearchPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"German\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Iso6391, Is.EqualTo("de"));
        });
    }

    /// <summary>
    ///     <c>Search</c> with a populated <see cref="QueryParameterLanguage" /> renders the
    ///     pagination parameters onto the request URI.
    /// </summary>
    [Test]
    public async Task LanguageService_Search_WithQueryParameter_RendersLimitAndOffsetOnUri()
    {
        Server
            .Given(Request.Create().WithPath(SearchPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new LanguageService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "iso_639_1", Value = "de", Criteria = "=" }
        };

        await service.Search(criteria, new QueryParameterLanguage(10, 0),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SearchPath));
            Assert.That(request.RawQuery, Does.Contain("limit=10"));
            Assert.That(request.RawQuery, Does.Contain("offset=0"));
        });
    }
}
