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
using BexioApiNet.Services.Connectors.Timesheets;

namespace BexioApiNet.IntegrationTests.Timesheets;

/// <summary>
/// Integration tests covering <see cref="TimesheetService" />. The request path is composed
/// from <see cref="TimesheetConfiguration" /> (<c>2.0/timesheet</c>) and must reach WireMock
/// intact when the service is driven through the real connection handler.
/// </summary>
public sealed class TimesheetServiceIntegrationTests : IntegrationTestBase
{
    private const string TimesheetsPath = "/2.0/timesheet";

    private const string TimesheetResponse = """
                                              {
                                                  "id": 1,
                                                  "user_id": 1,
                                                  "status_id": 4,
                                                  "client_service_id": 1,
                                                  "text": "Implementation work",
                                                  "allowable_bill": true,
                                                  "charge": null,
                                                  "contact_id": 2,
                                                  "sub_contact_id": null,
                                                  "pr_project_id": null,
                                                  "pr_package_id": null,
                                                  "pr_milestone_id": null,
                                                  "travel_time": null,
                                                  "travel_charge": null,
                                                  "travel_distance": 0,
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

    /// <summary>
    /// <c>TimesheetService.Get()</c> must issue a <c>GET</c> against <c>/2.0/timesheet</c>
    /// and return a successful <c>ApiResult</c> when the server responds with an empty
    /// collection.
    /// </summary>
    [Test]
    public async Task TimesheetService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(TimesheetsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TimesheetService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TimesheetsPath));
        });
    }

    /// <summary>
    /// <c>TimesheetService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and surface the returned timesheet on success.
    /// </summary>
    [Test]
    public async Task TimesheetService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
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
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="TimesheetCreate" /> payload, and must surface the returned
    /// timesheet on success.
    /// </summary>
    [Test]
    public async Task TimesheetService_Create_SendsPostRequestWithPayload()
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
            Assert.That(request.Body, Does.Contain("\"allowable_bill\":true"));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/timesheet/search</c> with the <see cref="SearchCriteria" /> list as the
    /// JSON body.
    /// </summary>
    [Test]
    public async Task TimesheetService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{TimesheetsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TimesheetService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "user_id", Value = "1", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"user_id\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Update</c> must send a <c>PUT</c> request against
    /// <c>/2.0/timesheet/{id}</c> whose body is the serialized <see cref="TimesheetUpdate" />
    /// payload.
    /// </summary>
    [Test]
    public async Task TimesheetService_Update_SendsPutRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TimesheetsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
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
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>TimesheetService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path.
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
