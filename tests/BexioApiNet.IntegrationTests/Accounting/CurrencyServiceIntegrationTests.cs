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
/// (<c>3.0/currencies</c>) reaches the handler correctly, that the expected HTTP
/// verbs and query parameters are used, and that fully populated JSON responses
/// (matching the Bexio v3 OpenAPI <c>v3CurrencyResponse</c> and <c>v3ExchangeRate</c>
/// schemas) deserialize into every field of the C# model.
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

    private const string ExchangeRateResponse = """
                                                [
                                                  {
                                                    "factor_nr": 1.087,
                                                    "exchange_currency": {
                                                      "id": 2,
                                                      "name": "EUR",
                                                      "round_factor": 0.01
                                                    },
                                                    "ratio": 1.0,
                                                    "exchange_rate_to_ratio": 1.087,
                                                    "source": "monthly_average",
                                                    "source_reason": "monthly_average_provided",
                                                    "exchange_rate_date": "2024-05-01"
                                                  }
                                                ]
                                                """;

    /// <summary>
    /// <c>CurrencyService.Get</c> returns a fully populated <c>v3CurrencyResponse</c> array. Each
    /// field of the resulting <see cref="BexioApiNet.Abstractions.Models.Accounting.Currencies.Currency"/>
    /// must deserialize correctly: <c>id</c>, <c>name</c>, and <c>round_factor</c>.
    /// </summary>
    [Test]
    public async Task CurrencyService_Get_DeserializesAllFields()
    {
        const string body = """
                            [
                              {
                                "id": 1,
                                "name": "CHF",
                                "round_factor": 0.05
                              },
                              {
                                "id": 2,
                                "name": "EUR",
                                "round_factor": 0.01
                              }
                            ]
                            """;

        Server
            .Given(Request.Create().WithPath(CurrenciesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(body));

        var service = new CurrencyService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("CHF"));
            Assert.That(result.Data![0].RoundFactor, Is.EqualTo(0.05));
            Assert.That(result.Data![1].Id, Is.EqualTo(2));
            Assert.That(result.Data![1].Name, Is.EqualTo("EUR"));
            Assert.That(result.Data![1].RoundFactor, Is.EqualTo(0.01));
        });
    }

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
    /// currency id in the URL path and deserialize the full <c>v3CurrencyResponse</c>
    /// payload returned by the API.
    /// </summary>
    [Test]
    public async Task CurrencyService_GetById_DeserializesAllFields()
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
            Assert.That(result.Data!.Name, Is.EqualTo("CHF"));
            Assert.That(result.Data!.RoundFactor, Is.EqualTo(0.05));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>CurrencyService.GetExchangeRates</c> must issue a <c>GET</c> request to the nested
    /// <c>/3.0/currencies/{id}/exchange_rates</c> path, forward the <c>date</c> query
    /// parameter when supplied, and deserialize all fields of the <c>v3ExchangeRate</c>
    /// schema (factor_nr, exchange_currency, ratio, exchange_rate_to_ratio, source,
    /// source_reason, exchange_rate_date).
    /// </summary>
    [Test]
    public async Task CurrencyService_GetExchangeRates_WithDate_DeserializesAllFields()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}/exchange_rates";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ExchangeRateResponse));

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
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].FactorNr, Is.EqualTo(1.087m));
            Assert.That(result.Data![0].ExchangeCurrency, Is.Not.Null);
            Assert.That(result.Data![0].ExchangeCurrency.Id, Is.EqualTo(2));
            Assert.That(result.Data![0].ExchangeCurrency.Name, Is.EqualTo("EUR"));
            Assert.That(result.Data![0].ExchangeCurrency.RoundFactor, Is.EqualTo(0.01));
            Assert.That(result.Data![0].Ratio, Is.EqualTo(1.0m));
            Assert.That(result.Data![0].ExchangeRateToRatio, Is.EqualTo(1.087m));
            Assert.That(result.Data![0].Source, Is.EqualTo("monthly_average"));
            Assert.That(result.Data![0].SourceReason, Is.EqualTo("monthly_average_provided"));
            Assert.That(result.Data![0].ExchangeRateDate, Is.EqualTo("2024-05-01"));
        });
    }

    /// <summary>
    /// <c>CurrencyService.Create</c> must issue a <c>POST</c> request whose body is the
    /// serialized <see cref="CurrencyCreate" /> payload — both <c>name</c> and
    /// <c>round_factor</c> must reach the wire because the spec marks them as required.
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
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(result.Data!.Name, Is.EqualTo("CHF"));
            Assert.That(result.Data!.RoundFactor, Is.EqualTo(0.05));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CurrenciesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"CHF\""));
            Assert.That(request.Body, Does.Contain("\"round_factor\":0.05"));
        });
    }

    /// <summary>
    /// <c>CurrencyService.Patch</c> must issue a <c>PATCH</c> request that includes the id in
    /// the URL path and serializes the payload as the body. Per the spec only
    /// <c>round_factor</c> is patchable. The endpoint is documented to return 201 Created.
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
    /// <c>CurrencyService.Patch</c> with a <see langword="null"/> <c>round_factor</c> must
    /// emit an empty JSON object — the <c>JsonIgnoreCondition.WhenWritingNull</c> attribute
    /// prevents the property from leaking onto the wire.
    /// </summary>
    [Test]
    public async Task CurrencyService_Patch_OmitsNullRoundFactorFromBody()
    {
        const int id = 1;
        var expectedPath = $"{CurrenciesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CurrencyResponse));

        var service = new CurrencyService(ConnectionHandler);

        await service.Patch(id, new CurrencyPatch(null), TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.Body, Does.Not.Contain("round_factor"));
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

    /// <summary>
    /// <c>CurrencyService.Get</c> with a <see cref="QueryParameterCurrency"/> must forward
    /// optional <c>limit</c>, <c>offset</c>, <c>embed</c> and <c>date</c> query parameters
    /// onto the URL.
    /// </summary>
    [Test]
    public async Task CurrencyService_Get_WithQueryParameter_AppendsAllOptionalQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(CurrenciesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CurrencyService(ConnectionHandler);

        await service.Get(
            new QueryParameterCurrency(Limit: 10, Offset: 5, Embed: "exchange_rate", Date: new DateOnly(2024, 5, 1)),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CurrenciesPath));
            Assert.That(request.RawQuery, Does.Contain("limit=10"));
            Assert.That(request.RawQuery, Does.Contain("offset=5"));
            Assert.That(request.RawQuery, Does.Contain("embed=exchange_rate"));
            Assert.That(request.RawQuery, Does.Contain("date=2024-05-01"));
        });
    }
}
