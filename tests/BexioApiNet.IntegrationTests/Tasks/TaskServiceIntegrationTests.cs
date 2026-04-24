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
///     service is driven through the real connection handler.
/// </summary>
public sealed class TaskServiceIntegrationTests : IntegrationTestBase
{
    private const string TasksPath = "/2.0/task";

    private const string TaskResponse = """
                                        {
                                            "id": 1,
                                            "user_id": 1,
                                            "finish_date": null,
                                            "subject": "Send documents",
                                            "place": null,
                                            "info": null,
                                            "contact_id": null,
                                            "sub_contact_id": null,
                                            "project_id": null,
                                            "entry_id": null,
                                            "module_id": null,
                                            "todo_status_id": 1,
                                            "todo_priority_id": null,
                                            "has_reminder": null,
                                            "remember_type_id": null,
                                            "remember_time_id": null,
                                            "communication_kind_id": null
                                        }
                                        """;

    /// <summary>
    ///     <c>TaskService.Get()</c> must issue a <c>GET</c> against <c>/2.0/task</c> and return a
    ///     successful <c>ApiResult</c> when the server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task TaskService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterTask" /> is supplied, <c>TaskService.Get</c> must translate
    ///     its <c>limit</c> and <c>offset</c> values into query-string parameters on the outgoing
    ///     request.
    /// </summary>
    [Test]
    public async Task TaskService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterTask(25, 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>TaskService.GetById</c> must issue a <c>GET</c> request that includes the target id in
    ///     the URL path and surface the returned task on success.
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
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>TaskService.Create</c> must send a <c>POST</c> request whose body is the serialized
    ///     <see cref="TaskCreate" /> payload, and must surface the returned task on success.
    /// </summary>
    [Test]
    public async Task TaskService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(TasksPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(TaskResponse));

        var service = new TaskService(ConnectionHandler);

        var payload = new TaskCreate(
            1,
            "Send documents");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TasksPath));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Send documents\""));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
        });
    }

    /// <summary>
    ///     <c>TaskService.Search</c> must send a <c>POST</c> request against <c>/2.0/task/search</c>
    ///     with the <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task TaskService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{TasksPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Send docs", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"subject\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    ///     <c>TaskService.Update</c> must send a <c>POST</c> request against <c>/2.0/task/{id}</c> —
    ///     Bexio edits tasks via POST on this resource — with the serialized <see cref="TaskUpdate" />
    ///     payload as the JSON body.
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
            1,
            "Send documents");

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
    ///     <c>TaskService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    ///     in the URL path.
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
}
