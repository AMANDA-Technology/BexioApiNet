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
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet;
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Timesheets;

namespace BexioApiNet.IntegrationTests.Timesheets;

/// <summary>
/// Integration tests covering <see cref="TimesheetService" />. The request path is composed
/// from <see cref="TimesheetConfiguration" /> (<c>2.0/timesheet</c>) and must reach WireMock
/// intact when the service is driven through the real connection handler. Stub bodies use
/// fully populated payloads matching the v3 OpenAPI schema (<c>v2TimesheetResponse</c> and
/// the three <c>tracking</c> oneOf variants) so end-to-end deserialization is exercised.
/// </summary>
public sealed class TimesheetServiceIntegrationTests : IntegrationTestBase
{
    private const string TimesheetsPath = "/2.0/timesheet";

    private const string TimesheetResponse = """
                                              {
                                                  "id": 2,
                                                  "user_id": 1,
                                                  "status_id": 4,
                                                  "client_service_id": 1,
                                                  "text": "Implementation work",
                                                  "allowable_bill": true,
                                                  "charge": "100.00",
                                                  "contact_id": 2,
                                                  "sub_contact_id": 3,
                                                  "pr_project_id": 7,
                                                  "pr_package_id": 11,
                                                  "pr_milestone_id": 13,
                                                  "travel_time": "00:30",
                                                  "travel_charge": "10.00",
                                                  "travel_distance": 25,
                                                  "estimated_time": "02:30",
                                                  "date": "2026-04-20",
                                                  "duration": "01:40",
                                                  "running": false,
                                                  "tracking": {
                                                      "type": "duration",
                                                      "date": "2026-04-20",
                                                      "duration": "01:40"
                                                  }
                                              }
                                              """;

    private const string TimesheetListBody = """
                                              [
                                                  {
                                                      "id": 1,
                                                      "user_id": 1,
                                                      "status_id": 4,
                                                      "client_service_id": 1,
                                                      "text": "Range tracking",
                                                      "allowable_bill": true,
                                                      "charge": null,
                                                      "contact_id": null,
                                                      "sub_contact_id": null,
                                                      "pr_project_id": null,
                                                      "pr_package_id": null,
                                                      "pr_milestone_id": null,
                                                      "travel_time": null,
                                                      "travel_charge": null,
                                                      "travel_distance": 0,
                                                      "estimated_time": null,
                                                      "date": "2026-04-20",
                                                      "duration": "01:51",
                                                      "running": false,
                                                      "tracking": {
                                                          "type": "range",
                                                          "start": "2026-04-20 14:22:48",
                                                          "end": "2026-04-20 16:13:25"
                                                      }
                                                  },
                                                  {
                                                      "id": 2,
                                                      "user_id": 1,
                                                      "status_id": 4,
                                                      "client_service_id": 1,
                                                      "text": "Stopwatch tracking",
                                                      "allowable_bill": false,
                                                      "charge": "0.00",
                                                      "contact_id": null,
                                                      "sub_contact_id": null,
                                                      "pr_project_id": null,
                                                      "pr_package_id": null,
                                                      "pr_milestone_id": null,
                                                      "travel_time": null,
                                                      "travel_charge": null,
                                                      "travel_distance": 0,
                                                      "estimated_time": null,
                                                      "date": "2026-04-20",
                                                      "duration": "00:45",
                                                      "running": true,
                                                      "tracking": {
                                                          "type": "stopwatch",
                                                          "duration": "00:45"
                                                      }
                                                  }
                                              ]
                                              """;

    /// <summary>
    /// <c>TimesheetService.Get()</c> must issue a <c>GET</c> against <c>/2.0/timesheet</c>
    /// and deserialize a populated array response — including the polymorphic
    /// <c>tracking</c> oneOf for both <c>range</c> and <c>stopwatch</c> variants.
    /// </summary>
    [Test]
    public async Task TimesheetService_Get_DeserializesPopulatedListWithRangeAndStopwatchTracking()
    {
        Server
            .Given(Request.Create().WithPath(TimesheetsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TimesheetListBody));

        var service = new TimesheetService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TimesheetsPath));

            var rangeEntry = result.Data![0];
            Assert.That(rangeEntry.Id, Is.EqualTo(1));
            Assert.That(rangeEntry.Tracking, Is.InstanceOf<TimesheetRangeTracking>());
            var range = (TimesheetRangeTracking)rangeEntry.Tracking!;
            Assert.That(range.Type, Is.EqualTo("range"));
            Assert.That(range.Start, Is.EqualTo("2026-04-20 14:22:48"));
            Assert.That(range.End, Is.EqualTo("2026-04-20 16:13:25"));

            var stopwatchEntry = result.Data[1];
            Assert.That(stopwatchEntry.Id, Is.EqualTo(2));
            Assert.That(stopwatchEntry.Running, Is.True);
            Assert.That(stopwatchEntry.Tracking, Is.InstanceOf<TimesheetStopwatchTracking>());
            var stopwatch = (TimesheetStopwatchTracking)stopwatchEntry.Tracking!;
            Assert.That(stopwatch.Type, Is.EqualTo("stopwatch"));
            Assert.That(stopwatch.Duration, Is.EqualTo("00:45"));
        });
    }

    /// <summary>
    /// <c>TimesheetService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and deserialize every field including the <c>duration</c>
    /// tracking variant.
    /// </summary>
    [Test]
    public async Task TimesheetService_GetById_DeserializesAllFields()
    {
        const int id = 2;
        var expectedPath = $"{TimesheetsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TimesheetResponse));

        var service = new TimesheetService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));

            var data = result.Data;
            Assert.That(data, Is.Not.Null);
            Assert.That(data!.Id, Is.EqualTo(2));
            Assert.That(data.UserId, Is.EqualTo(1));
            Assert.That(data.StatusId, Is.EqualTo(4));
            Assert.That(data.ClientServiceId, Is.EqualTo(1));
            Assert.That(data.Text, Is.EqualTo("Implementation work"));
            Assert.That(data.AllowableBill, Is.True);
            Assert.That(data.Charge, Is.EqualTo("100.00"));
            Assert.That(data.ContactId, Is.EqualTo(2));
            Assert.That(data.SubContactId, Is.EqualTo(3));
            Assert.That(data.PrProjectId, Is.EqualTo(7));
            Assert.That(data.PrPackageId, Is.EqualTo(11));
            Assert.That(data.PrMilestoneId, Is.EqualTo(13));
            Assert.That(data.TravelTime, Is.EqualTo("00:30"));
            Assert.That(data.TravelCharge, Is.EqualTo("10.00"));
            Assert.That(data.TravelDistance, Is.EqualTo(25));
            Assert.That(data.EstimatedTime, Is.EqualTo("02:30"));
            Assert.That(data.Date, Is.EqualTo(new DateOnly(2026, 4, 20)));
            Assert.That(data.Duration, Is.EqualTo("01:40"));
            Assert.That(data.Running, Is.False);

            Assert.That(data.Tracking, Is.InstanceOf<TimesheetDurationTracking>());
            var tracking = (TimesheetDurationTracking)data.Tracking!;
            Assert.That(tracking.Type, Is.EqualTo("duration"));
            Assert.That(tracking.Date, Is.EqualTo(new DateOnly(2026, 4, 20)));
            Assert.That(tracking.Duration, Is.EqualTo("01:40"));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="TimesheetCreate" /> payload, with the <c>tracking</c>
    /// discriminator on the wire and the response correctly deserialized.
    /// </summary>
    [Test]
    public async Task TimesheetService_Create_SendsPostRequestWithTrackingDiscriminator()
    {
        Server
            .Given(Request.Create().WithPath(TimesheetsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(TimesheetResponse));

        var service = new TimesheetService(ConnectionHandler);

        var payload = new TimesheetCreate(
            UserId: 1,
            ClientServiceId: 1,
            AllowableBill: true,
            Tracking: new TimesheetDurationTracking(new DateOnly(2026, 4, 20), "01:40"));

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TimesheetsPath));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"client_service_id\":1"));
            Assert.That(request.Body, Does.Contain("\"allowable_bill\":true"));
            Assert.That(request.Body, Does.Contain("\"type\":\"duration\""));
            Assert.That(request.Body, Does.Contain("\"date\":\"2026-04-20\""));
            Assert.That(request.Body, Does.Contain("\"duration\":\"01:40\""));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/timesheet/search</c> with the <see cref="SearchCriteria" /> list as the
    /// JSON body, and forward optional pagination / order parameters via the query string.
    /// </summary>
    [Test]
    public async Task TimesheetService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{TimesheetsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TimesheetListBody));

        var service = new TimesheetService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "user_id", Value = "1", Criteria = "=" }
        };
        var queryParameter = new QueryParameterTimesheet(Limit: 10, Offset: 0, OrderBy: "date_desc");

        var result = await service.Search(criteria, queryParameter, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"user_id\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"1\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
            Assert.That(request.Url, Does.Contain("limit=10"));
            Assert.That(request.Url, Does.Contain("offset=0"));
            Assert.That(request.Url, Does.Contain("order_by=date_desc"));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Update</c> must send a <c>POST</c> request against
    /// <c>/2.0/timesheet/{id}</c> whose body is the serialized <see cref="TimesheetUpdate" />
    /// payload. Bexio v2 timesheet edits use POST, not PUT, per the OpenAPI spec
    /// (operationId <c>v2EditTimesheet</c>).
    /// </summary>
    [Test]
    public async Task TimesheetService_Update_SendsPostRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TimesheetsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TimesheetResponse));

        var service = new TimesheetService(ConnectionHandler);

        var payload = new TimesheetUpdate(
            UserId: 1,
            ClientServiceId: 1,
            AllowableBill: true,
            Tracking: new TimesheetDurationTracking(new DateOnly(2026, 4, 20), "02:30"));

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path and deserialize the <c>{ "success": true }</c> body.
    /// </summary>
    [Test]
    public async Task TimesheetService_Delete_SendsDeleteRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TimesheetsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new TimesheetService(ConnectionHandler);

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
