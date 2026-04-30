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

using BexioApiNet.Abstractions.Models.Accounting.BusinessYears.Enums;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="BusinessYearService" /> entry points against
/// WireMock stubs. Verifies the path composed from <see cref="BusinessYearConfiguration" />
/// (<c>3.0/accounting/business_years</c>) reaches the handler correctly and that the
/// expected HTTP verbs are used.
/// </summary>
public sealed class BusinessYearServiceIntegrationTests : IntegrationTestBase
{
    private const string BusinessYearsPath = "/3.0/accounting/business_years";

    /// <summary>
    /// Single business-year payload matching the <c>BusinessYear</c> schema in
    /// <c>doc/openapi/bexio-v3.json</c> for the <c>ListBusinessYears</c> /
    /// <c>ShowBusinessYear</c> operations.
    /// </summary>
    private const string BusinessYearResponse = """
                                                {
                                                    "id": 1,
                                                    "start": "2018-01-01",
                                                    "end": "2018-12-31",
                                                    "status": "closed",
                                                    "closed_at": "2019-04-28"
                                                }
                                                """;

    /// <summary>
    /// <c>BusinessYearService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/business_years</c> and return a successful <c>ApiResult</c>
    /// when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task BusinessYearService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(BusinessYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BusinessYearsPath));
        });
    }

    /// <summary>
    /// When the server returns a fully-populated business-year array, the deserialized
    /// <c>BusinessYear</c> record must round-trip every field defined by the
    /// <c>BusinessYear</c> schema (id, start, end, status, closed_at).
    /// </summary>
    [Test]
    public async Task BusinessYearService_Get_DeserializesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(BusinessYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{BusinessYearResponse}]"));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var year = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(year.Id, Is.EqualTo(1));
            Assert.That(year.Start, Is.EqualTo(new DateOnly(2018, 1, 1)));
            Assert.That(year.End, Is.EqualTo(new DateOnly(2018, 12, 31)));
            Assert.That(year.Status, Is.EqualTo(BusinessYearStatus.closed));
            Assert.That(year.ClosedAt, Is.EqualTo(new DateOnly(2019, 4, 28)));
        });
    }

    /// <summary>
    /// When a <see cref="QueryParameterBusinessYear" /> is supplied, the service must
    /// translate its <c>limit</c> and <c>offset</c> values into query-string parameters
    /// on the outgoing request.
    /// </summary>
    [Test]
    public async Task BusinessYearService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(BusinessYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterBusinessYear(Limit: 20, Offset: 40),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BusinessYearsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=20"));
            Assert.That(request.RawQuery, Does.Contain("offset=40"));
        });
    }

    /// <summary>
    /// <c>BusinessYearService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and surface the returned business year on success, with
    /// every schema field correctly deserialized.
    /// </summary>
    [Test]
    public async Task BusinessYearService_GetById_DeserializesAllSchemaFields()
    {
        const int id = 1;
        var expectedPath = $"{BusinessYearsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(BusinessYearResponse));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Start, Is.EqualTo(new DateOnly(2018, 1, 1)));
            Assert.That(result.Data!.End, Is.EqualTo(new DateOnly(2018, 12, 31)));
            Assert.That(result.Data!.Status, Is.EqualTo(BusinessYearStatus.closed));
            Assert.That(result.Data!.ClosedAt, Is.EqualTo(new DateOnly(2019, 4, 28)));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// An open business year has no closing date — <c>closed_at</c> is nullable per spec
    /// and must round-trip as <see langword="null" />, with the status string deserialized
    /// to <see cref="BusinessYearStatus.open" />.
    /// </summary>
    [Test]
    public async Task BusinessYearService_GetById_DeserializesNullClosedAtForOpenYear()
    {
        const string openYear = """
                                {
                                    "id": 2,
                                    "start": "2024-01-01",
                                    "end": "2024-12-31",
                                    "status": "open",
                                    "closed_at": null
                                }
                                """;
        const int id = 2;
        var expectedPath = $"{BusinessYearsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(openYear));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Status, Is.EqualTo(BusinessYearStatus.open));
            Assert.That(result.Data!.ClosedAt, Is.Null);
        });
    }

    /// <summary>
    /// The <c>locked</c> status enum value (business year closed for new bookings, but
    /// without an ordinary year-end closing) must deserialize to
    /// <see cref="BusinessYearStatus.locked" />.
    /// </summary>
    [Test]
    public async Task BusinessYearService_GetById_DeserializesLockedStatus()
    {
        const string lockedYear = """
                                  {
                                      "id": 3,
                                      "start": "2023-01-01",
                                      "end": "2023-12-31",
                                      "status": "locked",
                                      "closed_at": null
                                  }
                                  """;
        const int id = 3;
        var expectedPath = $"{BusinessYearsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(lockedYear));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Status, Is.EqualTo(BusinessYearStatus.locked));
    }
}
