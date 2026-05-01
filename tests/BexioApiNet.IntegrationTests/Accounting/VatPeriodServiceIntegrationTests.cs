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

using BexioApiNet.Abstractions.Models.Accounting.VatPeriods.Enums;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="VatPeriodService" /> entry points against WireMock
/// stubs. Verifies the path composed from <see cref="VatPeriodConfiguration" />
/// (<c>3.0/accounting/vat_periods</c>) reaches the handler correctly, that the expected
/// HTTP verbs are used, and that fully populated JSON responses (matching the Bexio v3
/// OpenAPI <c>v3VatPeriod</c> schema) deserialize into every field of the
/// <see cref="BexioApiNet.Abstractions.Models.Accounting.VatPeriods.VatPeriod"/> model.
/// </summary>
public sealed class VatPeriodServiceIntegrationTests : IntegrationTestBase
{
    private const string VatPeriodsPath = "/3.0/accounting/vat_periods";

    private const string VatPeriodResponse = """
                                             {
                                                 "id": 1,
                                                 "start": "2024-01-01",
                                                 "end": "2024-03-31",
                                                 "type": "quarter",
                                                 "status": "closed",
                                                 "closed_at": "2024-04-30"
                                             }
                                             """;

    private const string VatPeriodListResponse = """
                                                 [
                                                     {
                                                         "id": 1,
                                                         "start": "2024-01-01",
                                                         "end": "2024-03-31",
                                                         "type": "quarter",
                                                         "status": "closed",
                                                         "closed_at": "2024-04-30"
                                                     },
                                                     {
                                                         "id": 2,
                                                         "start": "2024-04-01",
                                                         "end": "2024-06-30",
                                                         "type": "quarter",
                                                         "status": "open",
                                                         "closed_at": null
                                                     },
                                                     {
                                                         "id": 3,
                                                         "start": "2023-01-01",
                                                         "end": "2023-12-31",
                                                         "type": "annual",
                                                         "status": "closed_with_message",
                                                         "closed_at": "2024-01-31"
                                                     }
                                                 ]
                                                 """;

    /// <summary>
    /// <c>VatPeriodService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/vat_periods</c> and deserialize each <c>v3VatPeriod</c> entry —
    /// including the <c>type</c> and <c>status</c> enum values and the optional
    /// <c>closed_at</c> date.
    /// </summary>
    [Test]
    public async Task VatPeriodService_Get_DeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(VatPeriodsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(VatPeriodListResponse));

        var service = new VatPeriodService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(VatPeriodsPath));
            Assert.That(result.Data, Has.Count.EqualTo(3));

            var quarter = result.Data![0];
            Assert.That(quarter.Id, Is.EqualTo(1));
            Assert.That(quarter.Start, Is.EqualTo(new DateOnly(2024, 1, 1)));
            Assert.That(quarter.End, Is.EqualTo(new DateOnly(2024, 3, 31)));
            Assert.That(quarter.Type, Is.EqualTo(VatPeriodType.quarter));
            Assert.That(quarter.Status, Is.EqualTo(VatPeriodStatus.closed));
            Assert.That(quarter.ClosedAt, Is.EqualTo(new DateOnly(2024, 4, 30)));

            var openQuarter = result.Data![1];
            Assert.That(openQuarter.Status, Is.EqualTo(VatPeriodStatus.open));
            Assert.That(openQuarter.ClosedAt, Is.Null);

            var annual = result.Data![2];
            Assert.That(annual.Type, Is.EqualTo(VatPeriodType.annual));
            Assert.That(annual.Status, Is.EqualTo(VatPeriodStatus.closed_with_message));
        });
    }

    /// <summary>
    /// <c>VatPeriodService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and surface every field of the returned <c>v3VatPeriod</c>
    /// on success.
    /// </summary>
    [Test]
    public async Task VatPeriodService_GetById_DeserializesAllFields()
    {
        const int id = 1;
        var expectedPath = $"{VatPeriodsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(VatPeriodResponse));

        var service = new VatPeriodService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Start, Is.EqualTo(new DateOnly(2024, 1, 1)));
            Assert.That(result.Data!.End, Is.EqualTo(new DateOnly(2024, 3, 31)));
            Assert.That(result.Data!.Type, Is.EqualTo(VatPeriodType.quarter));
            Assert.That(result.Data!.Status, Is.EqualTo(VatPeriodStatus.closed));
            Assert.That(result.Data!.ClosedAt, Is.EqualTo(new DateOnly(2024, 4, 30)));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>VatPeriodService.Get</c> with a <see cref="QueryParameterVatPeriod"/> must forward
    /// optional <c>limit</c> and <c>offset</c> onto the URL. Per the v3 OpenAPI spec these
    /// are the only two supported query parameters for vat periods.
    /// </summary>
    [Test]
    public async Task VatPeriodService_Get_WithQuery_AppendsLimitAndOffset()
    {
        Server
            .Given(Request.Create().WithPath(VatPeriodsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new VatPeriodService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterVatPeriod(Limit: 25, Offset: 50),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(VatPeriodsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=50"));
        });
    }
}
