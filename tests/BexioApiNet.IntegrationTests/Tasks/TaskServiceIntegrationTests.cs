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
using BexioApiNet.Abstractions.Models.Tasks.Task.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Tasks;

namespace BexioApiNet.IntegrationTests.Tasks;

/// <summary>
///     Integration tests covering <see cref="TaskService" />. The request path is composed from
///     <see cref="TaskConfiguration" /> (<c>2.0/task</c>) and must reach WireMock intact when the
///     service is driven through the real connection handler. The response body matches the
///     Bexio v2 OpenAPI <c>Task</c> schema exactly so deserialization of every documented field
///     can be asserted.
/// </summary>
public sealed class TaskServiceIntegrationTests : IntegrationTestBase
{
    private const string TasksPath = "/2.0/task";

    /// <summary>
    ///     Fully populated v2 Bexio <c>Task</c> response body. Every field documented in
    ///     <c>doc/openapi/bexio-v3.json</c> for the v2 task schema is included so the
    ///     deserializer is exercised on the complete payload.
    /// </summary>
    private const string TaskResponse = """
                                        {
                                            "id": 1,
                                            "user_id": 2,
                                            "finish_date": "2018-04-09T07:44:10+00:00",
                                            "subject": "Unterlagen versenden",
                                            "place": 3,
                                            "info": "so schnell wie möglich.",
                                            "contact_id": 4,
                                            "sub_contact_id": 5,
                                            "project_id": 6,
                                            "entry_id": 7,
                                            "module_id": 8,
                                            "todo_status_id": 9,
                                            "todo_priority_id": 10,
                                            "has_reminder": true,
                                            "remember_type_id": 11,
                                            "remember_time_id": 12,
                                            "communication_kind_id": 13
                                        }
                                        """;

    /// <summary>
    ///     <c>TaskService.Get()</c> must issue a <c>GET</c> against <c>/2.0/task</c> and surface the
    ///     fully populated array response, deserializing every field from the v2 Bexio task schema.
    /// </summary>
    [Test]
    public async Task TaskService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{TaskResponse}]"));

        var service = new TaskService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var task = result.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
            AssertFullyPopulatedTask(task);
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterTask" /> is supplied, <c>TaskService.Get</c> must translate
    ///     its <c>limit</c>, <c>offset</c> and <c>order_by</c> values into query-string parameters on
    ///     the outgoing request. These are the three filters documented for
    ///     <c>GET /2.0/task</c>.
    /// </summary>
    [Test]
    public async Task TaskService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterTask(25, 100, OrderBy: "finish_date_desc"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
            Assert.That(request.RawQuery, Does.Contain("order_by=finish_date_desc"));
        });
    }

    /// <summary>
    ///     <c>TaskService.GetById</c> must issue a <c>GET</c> request that includes the target id in
    ///     the URL path and must deserialize every documented field from the v2 task response.
    /// </summary>
    [Test]
    public async Task TaskService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TasksPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaskResponse));

        var service = new TaskService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            AssertFullyPopulatedTask(result.Data!);
        });
    }

    /// <summary>
    ///     <c>TaskService.Create</c> must send a <c>POST</c> request whose body serializes the
    ///     <see cref="TaskCreate" /> payload with snake_case Bexio field names, must accept the v2
    ///     <c>201 Created</c> response, and must deserialize every field of the returned task.
    /// </summary>
    [Test]
    public async Task TaskService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(TaskResponse));

        var service = new TaskService(ConnectionHandler);

        var payload = new TaskCreate(
            UserId: 1,
            Subject: "Unterlagen versenden",
            FinishDate: new DateTimeOffset(2018, 4, 9, 7, 44, 10, TimeSpan.Zero),
            Info: "so schnell wie möglich.",
            ContactId: 1,
            SubContactId: 2,
            PrProjectId: 3,
            EntryId: 4,
            ModuleId: 5,
            TodoStatusId: 1,
            TodoPriorityId: 1,
            HaveRemember: true,
            RememberTypeId: 1,
            RememberTimeId: 1,
            CommunicationKindId: 1);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Unterlagen versenden\""));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"have_remember\":true"));
            Assert.That(request.Body, Does.Contain("\"pr_project_id\":3"));
            AssertFullyPopulatedTask(result.Data!);
        });
    }

    /// <summary>
    ///     <c>TaskService.Search</c> must send a <c>POST</c> request against <c>/2.0/task/search</c>
    ///     with the <see cref="SearchCriteria" /> list as the JSON body and must deserialize the v2
    ///     task array response.
    /// </summary>
    [Test]
    public async Task TaskService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{TasksPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{TaskResponse}]"));

        var service = new TaskService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Unterlagen versenden", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"subject\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            AssertFullyPopulatedTask(result.Data!.Single());
        });
    }

    /// <summary>
    ///     <c>TaskService.Search</c> must propagate <see cref="QueryParameterTask" /> values as
    ///     <c>limit</c>, <c>offset</c> and <c>order_by</c> query-string parameters on the outgoing
    ///     <c>POST /2.0/task/search</c> request.
    /// </summary>
    [Test]
    public async Task TaskService_Search_WithQueryParameter_AppendsQueryString()
    {
        var expectedPath = $"{TasksPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Send docs", Criteria = "=" }
        };

        var result = await service.Search(
            criteria,
            new QueryParameterTask(20, 60, OrderBy: "id_asc"),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.RawQuery, Does.Contain("limit=20"));
            Assert.That(request.RawQuery, Does.Contain("offset=60"));
            Assert.That(request.RawQuery, Does.Contain("order_by=id_asc"));
        });
    }

    /// <summary>
    ///     <c>TaskService.Update</c> must send a <c>POST</c> request against <c>/2.0/task/{id}</c> —
    ///     Bexio edits tasks via POST on this resource — with the serialized <see cref="TaskUpdate" />
    ///     payload as the JSON body, and must deserialize every field of the returned task.
    /// </summary>
    [Test]
    public async Task TaskService_Update_SendsPostRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TasksPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaskResponse));

        var service = new TaskService(ConnectionHandler);

        var payload = new TaskUpdate(
            UserId: 1,
            Subject: "Unterlagen versenden",
            FinishDate: new DateTimeOffset(2018, 4, 9, 7, 44, 10, TimeSpan.Zero),
            Info: "so schnell wie möglich.",
            ContactId: 1,
            SubContactId: 2,
            PrProjectId: 3,
            EntryId: 4,
            ModuleId: 5,
            TodoStatusId: 2,
            TodoPriorityId: 2);

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Unterlagen versenden\""));
            Assert.That(request.Body, Does.Contain("\"todo_status_id\":2"));
            AssertFullyPopulatedTask(result.Data!);
        });
    }

    /// <summary>
    ///     <c>TaskService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    ///     in the URL path and surface the <c>200</c> Bexio response as a successful
    ///     <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task TaskService_Delete_SendsDeleteRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{TasksPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("true"));

        var service = new TaskService(ConnectionHandler);

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
    ///     Asserts that every field of the v2 Bexio task schema deserialized from
    ///     <see cref="TaskResponse" /> matches the expected example value. Centralized so each
    ///     CRUD test path can verify schema completeness without duplicating ~17 assertions.
    /// </summary>
    private static void AssertFullyPopulatedTask(BexioApiNet.Abstractions.Models.Tasks.Task.BexioTask task)
    {
        Assert.That(task.Id, Is.EqualTo(1));
        Assert.That(task.UserId, Is.EqualTo(2));
        Assert.That(task.FinishDate, Is.EqualTo(new DateTimeOffset(2018, 4, 9, 7, 44, 10, TimeSpan.Zero)));
        Assert.That(task.Subject, Is.EqualTo("Unterlagen versenden"));
        Assert.That(task.Place, Is.EqualTo(3));
        Assert.That(task.Info, Is.EqualTo("so schnell wie möglich."));
        Assert.That(task.ContactId, Is.EqualTo(4));
        Assert.That(task.SubContactId, Is.EqualTo(5));
        Assert.That(task.ProjectId, Is.EqualTo(6));
        Assert.That(task.EntryId, Is.EqualTo(7));
        Assert.That(task.ModuleId, Is.EqualTo(8));
        Assert.That(task.TodoStatusId, Is.EqualTo(9));
        Assert.That(task.TodoPriorityId, Is.EqualTo(10));
        Assert.That(task.HasReminder, Is.True);
        Assert.That(task.RememberTypeId, Is.EqualTo(11));
        Assert.That(task.RememberTimeId, Is.EqualTo(12));
        Assert.That(task.CommunicationKindId, Is.EqualTo(13));
    }
}
