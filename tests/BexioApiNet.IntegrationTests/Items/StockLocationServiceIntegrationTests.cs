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
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.IntegrationTests.Items;

/// <summary>
/// Integration tests covering the read-only entry points of <see cref="StockLocationService" /> against
/// WireMock stubs. The Bexio API exposes stock locations under the path
/// <c>2.0/stock</c> (see <see cref="StockLocationConfiguration" />). This fixture verifies
/// that the URL is built correctly and that the expected HTTP verbs are used. The service only
/// supports <c>Get</c> and <c>Search</c> — there are no Create, Update, or Delete endpoints.
/// </summary>
public sealed class StockLocationServiceIntegrationTests : IntegrationTestBase
{
    private const string StockPath = "/2.0/stock";

    private const string StockLocationResponse = """
                                                 {
                                                     "id": 1,
                                                     "name": "Stock Berlin"
                                                 }
                                                 """;

    /// <summary>
    /// <c>StockLocationService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/stock</c> and deserialize each returned
    /// <see cref="Abstractions.Models.Items.StockLocations.StockLocation" /> from the
    /// OpenAPI-shaped JSON array returned by Bexio.
    /// </summary>
    [Test]
    public async Task StockLocationService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(StockPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{StockLocationResponse}]"));

        var service = new StockLocationService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(StockPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("Stock Berlin"));
        });
    }

    /// <summary>
    /// <c>StockLocationService.Get()</c> must forward typed query parameters (limit/offset/order_by)
    /// onto the request URI when a <see cref="QueryParameterStockLocation" /> is supplied.
    /// </summary>
    [Test]
    public async Task StockLocationService_Get_WithQueryParameter_ForwardsLimitOffsetOrderBy()
    {
        Server
            .Given(Request.Create().WithPath(StockPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new StockLocationService(ConnectionHandler);

        var queryParameter = new QueryParameterStockLocation(Limit: 25, Offset: 10, OrderBy: "name_asc");

        var result = await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Url, Does.Contain("limit=25"));
            Assert.That(request.Url, Does.Contain("offset=10"));
            Assert.That(request.Url, Does.Contain("order_by=name_asc"));
        });
    }

    /// <summary>
    /// <c>StockLocationService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/stock/search</c> with the <see cref="SearchCriteria" /> list as the JSON
    /// body and deserialize each returned stock location.
    /// </summary>
    [Test]
    public async Task StockLocationService_Search_SendsPostRequest_ToSearchPath_AndDeserializesAllFields()
    {
        var expectedPath = $"{StockPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{StockLocationResponse}]"));

        var service = new StockLocationService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Berlin", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("Stock Berlin"));
        });
    }
}
