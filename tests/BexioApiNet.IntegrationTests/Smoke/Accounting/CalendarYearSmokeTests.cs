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

using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Smoke.Accounting;

/// <summary>
///     Smoke tests covering the <see cref="CalendarYearService" /> entry points against
///     WireMock stubs. Verifies the path composed from <see cref="CalendarYearConfiguration" />
///     (<c>3.0/accounting/calendar_years</c>) reaches the handler correctly and that the
///     expected HTTP verbs are used.
/// </summary>
public sealed class CalendarYearSmokeTests : IntegrationTestBase
{
    private const string CalendarYearsPath = "/3.0/accounting/calendar_years";

    private const string CalendarYearResponse = """
                                                {
                                                    "id": 1,
                                                    "start": "2024-01-01",
                                                    "end": "2024-12-31",
                                                    "is_vat_subject": true,
                                                    "is_annual_reporting": false,
                                                    "created_at": "2024-01-01T00:00:00",
                                                    "updated_at": "2024-01-01T00:00:00",
                                                    "vat_accounting_method": "effective",
                                                    "vat_accounting_type": "agreed"
                                                }
                                                """;

    /// <summary>
    ///     <c>CalendarYearService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/3.0/accounting/calendar_years</c> and return a successful <c>ApiResult</c>
    ///     when the server returns an empty array.
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
    ///     <c>CalendarYearService.GetById</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and surface the returned calendar year on success.
    /// </summary>
    [Test]
    public async Task CalendarYearService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{CalendarYearsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CalendarYearResponse));

        var service = new CalendarYearService(ConnectionHandler);

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
    ///     <c>CalendarYearService.Create</c> must issue a <c>POST</c> request whose body is the
    ///     serialized <see cref="CalendarYearCreate" /> payload.
    /// </summary>
    [Test]
    public async Task CalendarYearService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(CalendarYearsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody($"[{CalendarYearResponse}]"));

        var service = new CalendarYearService(ConnectionHandler);

        var result = await service.Create(
            new CalendarYearCreate("2024", true, false, null, null, null, null),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CalendarYearsPath));
            Assert.That(request.Body, Does.Contain("\"year\":\"2024\""));
            Assert.That(request.Body, Does.Contain("\"is_vat_subject\":true"));
        });
    }

    /// <summary>
    ///     <c>CalendarYearService.Search</c> must issue a <c>POST</c> request against
    ///     <c>/3.0/accounting/calendar_years/search</c> with the search criteria as the JSON body.
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

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"is_vat_subject\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
        });
    }
}
