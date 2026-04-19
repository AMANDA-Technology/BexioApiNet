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

using BexioApiNet.Abstractions.Models.Accounting.Currencies.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="CurrencyService" /> entry points against
/// WireMock stubs. Verifies the path composed from <see cref="CurrencyConfiguration" />
/// (<c>3.0/currencies</c>) reaches the handler correctly and that the expected HTTP
/// verbs and query parameters are used.
/// </summary>
public sealed class CurrencyServiceIntegrationTests : IntegrationTestBase
{
    private const string CurrenciesPath = "/3.0/currencies";

    private const string CurrencyResponse = """
                                            {
                                                "id": 1,
                                                "name": "CHF",
                                                "round_factor": 0.05
                                            }
                                            """;

    /// <summary>
    /// <c>CurrencyService.GetCodes</c> must issue a <c>GET</c> request to
    /// <c>/3.0/currencies/codes</c> and surface the array of currency codes.
    /// </summary>
    [Test]
    public async Task CurrencyService_GetCodes_SendsGetRequestToCodesPath()
    {
        const string codesPath = $"{CurrenciesPath}/codes";
        Server
            .Given(Request.Create().WithPath(codesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[\"EUR\",\"GBP\",\"PLN\"]"));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.GetCodes(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.EqualTo(new[] { "EUR", "GBP", "PLN" }));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(codesPath));
        });
    }

    /// <summary>
    /// <c>CurrencyService.GetById</c> must issue a <c>GET</c> request that includes the
    /// currency id in the URL path.
    /// </summary>
    [Test]
    public async Task CurrencyService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CurrencyResponse));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>CurrencyService.GetExchangeRates</c> must issue a <c>GET</c> request to the nested
    /// <c>/3.0/currencies/{id}/exchange_rates</c> path and forward an optional <c>date</c>
    /// query parameter when supplied.
    /// </summary>
    [Test]
    public async Task CurrencyService_GetExchangeRates_WithDate_AppendsDateQuery()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}/exchange_rates";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.GetExchangeRates(
            id,
            new QueryParameterExchangeRate(new DateOnly(2024, 5, 1)),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.RawQuery, Does.Contain("date=2024-05-01"));
        });
    }

    /// <summary>
    /// <c>CurrencyService.Create</c> must issue a <c>POST</c> request whose body is the
    /// serialized <see cref="CurrencyCreate" /> payload.
    /// </summary>
    [Test]
    public async Task CurrencyService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(CurrenciesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CurrencyResponse));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Create(
            new CurrencyCreate("CHF", 0.05),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CurrenciesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"round_factor\":0.05"));
        });
    }

    /// <summary>
    /// <c>CurrencyService.Patch</c> must issue a <c>PATCH</c> request that includes the id in
    /// the URL path and serializes the payload as the body.
    /// </summary>
    [Test]
    public async Task CurrencyService_Patch_SendsPatchRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CurrencyResponse));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Patch(
            id,
            new CurrencyPatch(0.10),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"round_factor\":0.1"));
        });
    }

    /// <summary>
    /// <c>CurrencyService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path and surface a successful <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task CurrencyService_Delete_SendsDeleteRequest()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Delete(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
