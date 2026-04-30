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

using BexioApiNet.Abstractions.Models.Accounting.Taxes.Enums;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="TaxService" /> entry points against WireMock stubs.
/// Verifies the path composed from <see cref="TaxConfiguration" /> (<c>3.0/taxes</c>)
/// reaches the handler correctly, that the expected HTTP verbs and query parameters are
/// used, and that fully populated JSON responses (matching the Bexio v3 OpenAPI <c>v3Tax</c>
/// schema) deserialize into every field of the <see cref="BexioApiNet.Abstractions.Models.Accounting.Taxes.Tax"/>
/// model.
/// </summary>
public sealed class TaxServiceIntegrationTests : IntegrationTestBase
{
    private const string TaxesPath = "/3.0/taxes";

    private const string TaxResponse = """
                                       {
                                           "id": 1,
                                           "uuid": "11111111-1111-4111-9111-111111111111",
                                           "name": "lib.model.tax.ch.sales_8_1.name",
                                           "code": "UN81",
                                           "digit": "312",
                                           "type": "sales_tax",
                                           "account_id": 2200,
                                           "tax_settlement_type": "none",
                                           "value": 8.1,
                                           "net_tax_value": null,
                                           "start_year": 2024,
                                           "end_year": null,
                                           "is_active": true,
                                           "display_name": "MwSt 8.1% (312)",
                                           "start_month": 1,
                                           "end_month": null
                                       }
                                       """;

    private const string TaxListResponse = """
                                           [
                                               {
                                                   "id": 1,
                                                   "uuid": "11111111-1111-4111-9111-111111111111",
                                                   "name": "lib.model.tax.ch.sales_8_1.name",
                                                   "code": "UN81",
                                                   "digit": "312",
                                                   "type": "sales_tax",
                                                   "account_id": 2200,
                                                   "tax_settlement_type": "none",
                                                   "value": 8.1,
                                                   "net_tax_value": null,
                                                   "start_year": 2024,
                                                   "end_year": null,
                                                   "is_active": true,
                                                   "display_name": "MwSt 8.1% (312)",
                                                   "start_month": 1,
                                                   "end_month": null
                                               },
                                               {
                                                   "id": 2,
                                                   "uuid": "22222222-2222-4222-9222-222222222222",
                                                   "name": "lib.model.tax.ch.net_5_1.name",
                                                   "code": "NU51",
                                                   "digit": "302",
                                                   "type": "net_tax",
                                                   "account_id": 2201,
                                                   "tax_settlement_type": "none",
                                                   "value": 8.1,
                                                   "net_tax_value": "5.1",
                                                   "start_year": 2024,
                                                   "end_year": 2030,
                                                   "is_active": true,
                                                   "display_name": "Net 5.1% (302)",
                                                   "start_month": 1,
                                                   "end_month": 12
                                               }
                                           ]
                                           """;

    /// <summary>
    /// <c>TaxService.Get()</c> must issue a <c>GET</c> request against <c>/3.0/taxes</c> and
    /// deserialize each <c>v3Tax</c> entry — including string-typed nullable
    /// <c>net_tax_value</c> and the optional year/month range fields.
    /// </summary>
    [Test]
    public async Task TaxService_Get_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(TaxesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaxListResponse));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TaxesPath));
            Assert.That(result.Data, Has.Count.EqualTo(2));

            var first = result.Data![0];
            Assert.That(first.Id, Is.EqualTo(1));
            Assert.That(first.Uuid, Is.EqualTo("11111111-1111-4111-9111-111111111111"));
            Assert.That(first.Name, Is.EqualTo("lib.model.tax.ch.sales_8_1.name"));
            Assert.That(first.Code, Is.EqualTo("UN81"));
            Assert.That(first.Digit, Is.EqualTo("312"));
            Assert.That(first.Type, Is.EqualTo("sales_tax"));
            Assert.That(first.AccountId, Is.EqualTo(2200));
            Assert.That(first.TaxSettlementType, Is.EqualTo("none"));
            Assert.That(first.Value, Is.EqualTo(8.1m));
            Assert.That(first.NetTaxValue, Is.Null);
            Assert.That(first.StartYear, Is.EqualTo(2024));
            Assert.That(first.EndYear, Is.Null);
            Assert.That(first.IsActive, Is.True);
            Assert.That(first.DisplayName, Is.EqualTo("MwSt 8.1% (312)"));
            Assert.That(first.StartMonth, Is.EqualTo(1));
            Assert.That(first.EndMonth, Is.Null);

            var second = result.Data![1];
            Assert.That(second.Type, Is.EqualTo("net_tax"));
            Assert.That(second.NetTaxValue, Is.EqualTo("5.1"));
            Assert.That(second.EndYear, Is.EqualTo(2030));
            Assert.That(second.EndMonth, Is.EqualTo(12));
        });
    }

    /// <summary>
    /// <c>TaxService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and surface every field of the returned <c>v3Tax</c> on success.
    /// </summary>
    [Test]
    public async Task TaxService_GetById_DeserializesAllFields()
    {
        const int id = 1;
        var expectedPath = $"{TaxesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaxResponse));

        var service = new TaxService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Uuid, Is.EqualTo("11111111-1111-4111-9111-111111111111"));
            Assert.That(result.Data!.Name, Is.EqualTo("lib.model.tax.ch.sales_8_1.name"));
            Assert.That(result.Data!.Code, Is.EqualTo("UN81"));
            Assert.That(result.Data!.Digit, Is.EqualTo("312"));
            Assert.That(result.Data!.Type, Is.EqualTo("sales_tax"));
            Assert.That(result.Data!.AccountId, Is.EqualTo(2200));
            Assert.That(result.Data!.TaxSettlementType, Is.EqualTo("none"));
            Assert.That(result.Data!.Value, Is.EqualTo(8.1m));
            Assert.That(result.Data!.NetTaxValue, Is.Null);
            Assert.That(result.Data!.StartYear, Is.EqualTo(2024));
            Assert.That(result.Data!.EndYear, Is.Null);
            Assert.That(result.Data!.IsActive, Is.True);
            Assert.That(result.Data!.DisplayName, Is.EqualTo("MwSt 8.1% (312)"));
            Assert.That(result.Data!.StartMonth, Is.EqualTo(1));
            Assert.That(result.Data!.EndMonth, Is.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>TaxService.Get</c> with a <see cref="QueryParameterTax"/> must forward optional
    /// <c>scope</c>, <c>date</c>, <c>types</c>, <c>limit</c> and <c>offset</c> onto the URL
    /// — matching the v3 OpenAPI spec query parameter list.
    /// </summary>
    [Test]
    public async Task TaxService_Get_WithFullQuery_AppendsAllQueryParameters()
    {
        Server
            .Given(Request.Create().WithPath(TaxesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterTax(
                Scope: TaxScope.active,
                Date: new DateOnly(2024, 1, 1),
                Type: TaxType.sales_tax,
                Limit: 50,
                Offset: 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TaxesPath));
            Assert.That(request.RawQuery, Does.Contain("scope=active"));
            Assert.That(request.RawQuery, Does.Contain("date=2024-01-01"));
            Assert.That(request.RawQuery, Does.Contain("types=sales_tax"));
            Assert.That(request.RawQuery, Does.Contain("limit=50"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    /// <c>TaxService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path. The 200 response body is <c>EntryDeleted</c> per spec.
    /// </summary>
    [Test]
    public async Task TaxService_Delete_SendsDeleteRequest()
    {
        const int id = 1;
        var expectedPath = $"{TaxesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new TaxService(ConnectionHandler);

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
    /// <c>TaxService.Delete</c> against a tax that is referenced or assigned to digit 000
    /// returns 409 Conflict per the OpenAPI spec. The error must be surfaced via
    /// <see cref="BexioApiNet.Abstractions.Models.Api.ApiError"/> and <c>IsSuccess</c>
    /// must be <see langword="false"/>.
    /// </summary>
    [Test]
    public async Task TaxService_Delete_OnConflict_ReturnsApiError()
    {
        const int id = 1;
        var expectedPath = $"{TaxesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithBody("{\"error_code\":409,\"message\":\"tax cannot be deleted\"}"));

        var service = new TaxService(ConnectionHandler);

        var result = await service.Delete(id, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ApiError, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
        });
    }
}
