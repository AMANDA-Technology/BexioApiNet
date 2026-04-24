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
using BexioApiNet.Abstractions.Models.Projects.Project.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="ProjectService" />. The request path is composed from
///     <see cref="ProjectConfiguration" /> (<c>2.0/pr_project</c>) and must reach WireMock intact when
///     the service is driven through the real connection handler.
/// </summary>
public sealed class ProjectServiceIntegrationTests : IntegrationTestBase
{
    private const string ProjectsPath = "/2.0/pr_project";

    private const string ProjectResponse = """
                                           {
                                               "id": 1,
                                               "uuid": "5bceb11d-e2ec-4c47-aa32-55c9d56a18e7",
                                               "nr": "0001",
                                               "name": "Amanda Portal",
                                               "start_date": null,
                                               "end_date": null,
                                               "comment": null,
                                               "pr_state_id": 1,
                                               "pr_project_type_id": 1,
                                               "contact_id": 2,
                                               "contact_sub_id": null,
                                               "pr_invoice_type_id": 4,
                                               "pr_invoice_type_amount": "0.00",
                                               "pr_budget_type_id": 1,
                                               "pr_budget_type_amount": "0.00",
                                               "user_id": 1
                                           }
                                           """;

    /// <summary>
    ///     <c>ProjectService.Get()</c> must issue a <c>GET</c> against <c>/2.0/pr_project</c> and return
    ///     a successful <c>ApiResult</c> when the server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task ProjectService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterProject" /> is supplied, <c>ProjectService.Get</c> must
    ///     translate its <c>limit</c> and <c>offset</c> values into query-string parameters on the
    ///     outgoing request.
    /// </summary>
    [Test]
    public async Task ProjectService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterProject(25, 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>ProjectService.GetById</c> must issue a <c>GET</c> request that includes the target id in
    ///     the URL path and surface the returned project on success.
    /// </summary>
    [Test]
    public async Task ProjectService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{ProjectsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ProjectResponse));

        var service = new ProjectService(ConnectionHandler);

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
    ///     <c>ProjectService.Create</c> must send a <c>POST</c> request whose body is the serialized
    ///     <see cref="ProjectCreate" /> payload, and must surface the returned project on success.
    /// </summary>
    [Test]
    public async Task ProjectService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ProjectResponse));

        var service = new ProjectService(ConnectionHandler);

        var payload = new ProjectCreate(
            "Amanda Portal",
            2,
            1,
            1,
            1);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Amanda Portal\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":2"));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Search</c> must send a <c>POST</c> request against
    ///     <c>/2.0/pr_project/search</c> with the <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task ProjectService_Search_SendsPostRequestToSearchPath()
    {
        var expectedPath = $"{ProjectsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ProjectService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Amanda", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Update</c> must send a <c>POST</c> request against
    ///     <c>/2.0/pr_project/{id}</c> — Bexio edits projects via POST on this resource — with the
    ///     serialized <see cref="ProjectUpdate" /> payload as the JSON body.
    /// </summary>
    [Test]
    public async Task ProjectService_Update_SendsPostRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{ProjectsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ProjectResponse));

        var service = new ProjectService(ConnectionHandler);

        var payload = new ProjectUpdate(
            "Amanda Portal",
            2,
            1,
            1,
            1);

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
    ///     <c>ProjectService.Archive</c> must send a body-less <c>POST</c> against
    ///     <c>/2.0/pr_project/{id}/archive</c>.
    /// </summary>
    [Test]
    public async Task ProjectService_Archive_SendsPostRequestToArchivePath()
    {
        const int id = 1;
        var expectedPath = $"{ProjectsPath}/{id}/archive";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Archive(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Reactivate</c> must send a body-less <c>POST</c> against
    ///     <c>/2.0/pr_project/{id}/reactivate</c>.
    /// </summary>
    [Test]
    public async Task ProjectService_Reactivate_SendsPostRequestToReactivatePath()
    {
        const int id = 1;
        var expectedPath = $"{ProjectsPath}/{id}/reactivate";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Reactivate(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    ///     in the URL path.
    /// </summary>
    [Test]
    public async Task ProjectService_Delete_SendsDeleteRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{ProjectsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("true"));

        var service = new ProjectService(ConnectionHandler);

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
