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

using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Enums;
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="CalendarYearService" /> entry points against
/// WireMock stubs. Verifies the path composed from <see cref="CalendarYearConfiguration" />
/// (<c>3.0/accounting/calendar_years</c>) reaches the handler correctly and that the
/// expected HTTP verbs are used.
/// </summary>
public sealed class CalendarYearServiceIntegrationTests : IntegrationTestBase
{
    private const string CalendarYearsPath = "/3.0/accounting/calendar_years";

    /// <summary>
    /// Single calendar-year payload matching the <c>CalendarYear</c> schema in
    /// <c>doc/openapi/bexio-v3.json</c>. Uses the OpenAPI example timestamp format
    /// (ISO 8601 with timezone offset) for <c>created_at</c> / <c>updated_at</c>.
    /// </summary>
    private const string CalendarYearResponse = """
                                                {
                                                    "id": 1,
                                                    "start": "2018-01-01",
                                                    "end": "2018-12-31",
                                                    "is_vat_subject": true,
                                                    "is_annual_reporting": false,
                                                    "created_at": "2017-04-28T19:58:58+00:00",
                                                    "updated_at": "2018-04-30T19:58:58+00:00",
                                                    "vat_accounting_method": "effective",
                                                    "vat_accounting_type": "agreed"
                                                }
                                                """;

    /// <summary>
    /// <c>CalendarYearService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/3.0/accounting/calendar_years</c> and return a successful <c>ApiResult</c>
    /// when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CalendarYearsPath));
        });
    }

    /// <summary>
    /// When the server returns a fully-populated array, the deserialized
    /// <c>CalendarYear</c> record must round-trip every field defined by the
    /// <c>CalendarYear</c> schema (id, start, end, is_vat_subject,
    /// is_annual_reporting, created_at, updated_at, vat_accounting_method,
    /// vat_accounting_type).
    /// </summary>
    [Test]
    public async Task CalendarYearService_Get_DeserializesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var year = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(year.Id, Is.EqualTo(1));
            Assert.That(year.Start, Is.EqualTo(new DateOnly(2018, 1, 1)));
            Assert.That(year.End, Is.EqualTo(new DateOnly(2018, 12, 31)));
            Assert.That(year.IsVatSubject, Is.True);
            Assert.That(year.IsAnnualReporting, Is.False);
            Assert.That(year.CreatedAt.ToUniversalTime(), Is.EqualTo(new DateTime(2017, 4, 28, 19, 58, 58, DateTimeKind.Utc)));
            Assert.That(year.UpdatedAt.ToUniversalTime(), Is.EqualTo(new DateTime(2018, 4, 30, 19, 58, 58, DateTimeKind.Utc)));
            Assert.That(year.VatAccountingMethod, Is.EqualTo(VatAccountingMethod.effective));
            Assert.That(year.VatAccountingType, Is.EqualTo(VatAccountingType.agreed));
        });
    }

    /// <summary>
    /// The <c>net_tax</c> / <c>collected</c> enum values must round-trip through the
    /// <see cref="VatAccountingMethod" /> and <see cref="VatAccountingType" /> enum
    /// converters.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Get_DeserializesAlternateEnumValues()
    {
        const string netTaxYear = """
                                  [{
                                      "id": 9,
                                      "start": "2026-01-01",
                                      "end": "2026-12-31",
                                      "is_vat_subject": false,
                                      "is_annual_reporting": true,
                                      "created_at": "2025-12-15T08:00:00+00:00",
                                      "updated_at": "2025-12-15T08:00:00+00:00",
                                      "vat_accounting_method": "net_tax",
                                      "vat_accounting_type": "collected"
                                  }]
                                  """;

        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(netTaxYear));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var year = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(year.IsVatSubject, Is.False);
            Assert.That(year.IsAnnualReporting, Is.True);
            Assert.That(year.VatAccountingMethod, Is.EqualTo(VatAccountingMethod.net_tax));
            Assert.That(year.VatAccountingType, Is.EqualTo(VatAccountingType.collected));
        });
    }

    /// <summary>
    /// When a <see cref="QueryParameterCalendarYear" /> is supplied, the service must
    /// translate its <c>limit</c> and <c>offset</c> values into query-string parameters
    /// on the outgoing request.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterCalendarYear(Limit: 5, Offset: 10),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CalendarYearsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=5"));
            Assert.That(request.RawQuery, Does.Contain("offset=10"));
        });
    }

    /// <summary>
    /// <c>CalendarYearService.GetById</c> must issue a <c>GET</c> request that includes
    /// the target id in the URL path and surface the returned calendar year on success
    /// with every schema field correctly deserialized.
    /// </summary>
    [Test]
    public async Task CalendarYearService_GetById_DeserializesAllSchemaFields()
    {
        const int id = 1;
        var expectedPath = $"{CalendarYearsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CalendarYearResponse));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Start, Is.EqualTo(new DateOnly(2018, 1, 1)));
            Assert.That(result.Data!.End, Is.EqualTo(new DateOnly(2018, 12, 31)));
            Assert.That(result.Data!.IsVatSubject, Is.True);
            Assert.That(result.Data!.IsAnnualReporting, Is.False);
            Assert.That(result.Data!.VatAccountingMethod, Is.EqualTo(VatAccountingMethod.effective));
            Assert.That(result.Data!.VatAccountingType, Is.EqualTo(VatAccountingType.agreed));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>CalendarYearService.Create</c> must issue a <c>POST</c> request whose body is
    /// the serialized <see cref="CalendarYearCreate" /> payload, return <c>201</c>, and
    /// produce a populated array of <c>CalendarYear</c> on success.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Create_SendsPostRequestAndDeserializesResponse()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Create(
            new CalendarYearCreate(
                Year: "2018",
                IsVatSubject: true,
                IsAnnualReporting: false,
                VatAccountingMethod: VatAccountingMethod.effective,
                VatAccountingType: VatAccountingType.agreed,
                DefaultTaxIncomeId: 1,
                DefaultTaxExpenseId: 2),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].VatAccountingMethod, Is.EqualTo(VatAccountingMethod.effective));
            Assert.That(result.Data![0].VatAccountingType, Is.EqualTo(VatAccountingType.agreed));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CalendarYearsPath));
            Assert.That(request.Body, Does.Contain("\"year\":\"2018\""));
            Assert.That(request.Body, Does.Contain("\"is_vat_subject\":true"));
            Assert.That(request.Body, Does.Contain("\"is_annual_reporting\":false"));
            Assert.That(request.Body, Does.Contain("\"vat_accounting_method\":\"effective\""));
            Assert.That(request.Body, Does.Contain("\"vat_accounting_type\":\"agreed\""));
            Assert.That(request.Body, Does.Contain("\"default_tax_income_id\":1"));
            Assert.That(request.Body, Does.Contain("\"default_tax_expense_id\":2"));
        });
    }

    /// <summary>
    /// When only <c>year</c> is supplied (the spec allows omitting all other fields and
    /// inheriting them from the previous year), the optional fields are sent as JSON
    /// <c>null</c> in the request body.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Create_WithOnlyYear_SendsNullableFieldsAsNull()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Create(
            new CalendarYearCreate(
                Year: "2024",
                IsVatSubject: null,
                IsAnnualReporting: null,
                VatAccountingMethod: null,
                VatAccountingType: null,
                DefaultTaxIncomeId: null,
                DefaultTaxExpenseId: null),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CalendarYearsPath));
            Assert.That(request.Body, Does.Contain("\"year\":\"2024\""));
            Assert.That(request.Body, Does.Contain("\"is_vat_subject\":null"));
            Assert.That(request.Body, Does.Contain("\"is_annual_reporting\":null"));
            Assert.That(request.Body, Does.Contain("\"vat_accounting_method\":null"));
            Assert.That(request.Body, Does.Contain("\"vat_accounting_type\":null"));
            Assert.That(request.Body, Does.Contain("\"default_tax_income_id\":null"));
            Assert.That(request.Body, Does.Contain("\"default_tax_expense_id\":null"));
        });
    }

    /// <summary>
    /// <c>CalendarYearService.Search</c> must issue a <c>POST</c> request against
    /// <c>/3.0/accounting/calendar_years/search</c> with the search criteria as the JSON
    /// body and round-trip the deserialized list of calendar years.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{CalendarYearsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "is_vat_subject", Value = "true", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].IsVatSubject, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"is_vat_subject\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"true\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
        });
    }

    /// <summary>
    /// <c>CalendarYearService.Search</c> must forward limit/offset query parameters to
    /// the outgoing request alongside the JSON body.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Search_WithQueryParams_AppendsParams()
    {
        var expectedPath = $"{CalendarYearsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "start", Value = "2026-01-01", Criteria = "=" }
        };

        var result = await service.Search(
            criteria,
            new QueryParameterCalendarYear(Limit: 5, Offset: 0),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.RawQuery, Does.Contain("limit=5"));
            Assert.That(request.RawQuery, Does.Contain("offset=0"));
        });
    }
}
