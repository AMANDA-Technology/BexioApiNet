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
///     the service is driven through the real connection handler. Response payloads mirror the
///     <see href="https://docs.bexio.com/#tag/Projects/operation/v2ListProjects">List Projects</see>
///     OpenAPI schema exactly.
/// </summary>
public sealed class ProjectServiceIntegrationTests : IntegrationTestBase
{
    private const string ProjectsPath = "/2.0/pr_project";

    /// <summary>
    ///     Fully populated <c>Project</c> JSON payload — every property defined by the
    ///     <c>Project</c> schema in <c>doc/openapi/bexio-v3.json</c> is present so the test
    ///     verifies real deserialization rather than an empty stub. Values mirror the example
    ///     payload published by Bexio (<c>"046b6c7f-..."</c> uuid, <c>"Villa Kunterbunt"</c>
    ///     name, etc.).
    /// </summary>
    private const string ProjectResponse = """
                                           {
                                               "id": 2,
                                               "uuid": "046b6c7f-0b8a-43b9-b35d-6489e6daee91",
                                               "nr": "000002",
                                               "name": "Villa Kunterbunt",
                                               "start_date": "2019-07-12 00:00:00",
                                               "end_date": null,
                                               "comment": "",
                                               "pr_state_id": 2,
                                               "pr_project_type_id": 2,
                                               "contact_id": 2,
                                               "contact_sub_id": null,
                                               "pr_invoice_type_id": 3,
                                               "pr_invoice_type_amount": "230.00",
                                               "pr_budget_type_id": 1,
                                               "pr_budget_type_amount": "200.00",
                                               "user_id": 1
                                           }
                                           """;

    /// <summary>
    ///     <c>ProjectService.Get()</c> must issue a <c>GET</c> against <c>/2.0/pr_project</c> and
    ///     deserialize a fully populated project payload — every field from the OpenAPI schema
    ///     must round-trip into the <c>Project</c> record without loss.
    /// </summary>
    [Test]
    public async Task ProjectService_Get_DeserializesFullProjectPayload()
    {
        var responseBody = "[" + ProjectResponse + "]";

        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            var project = result.Data![0];
            Assert.That(project.Id, Is.EqualTo(2));
            Assert.That(project.Uuid, Is.EqualTo("046b6c7f-0b8a-43b9-b35d-6489e6daee91"));
            Assert.That(project.Nr, Is.EqualTo("000002"));
            Assert.That(project.Name, Is.EqualTo("Villa Kunterbunt"));
            Assert.That(project.StartDate, Is.EqualTo("2019-07-12 00:00:00"));
            Assert.That(project.EndDate, Is.Null);
            Assert.That(project.Comment, Is.EqualTo(""));
            Assert.That(project.PrStateId, Is.EqualTo(2));
            Assert.That(project.PrProjectTypeId, Is.EqualTo(2));
            Assert.That(project.ContactId, Is.EqualTo(2));
            Assert.That(project.ContactSubId, Is.Null);
            Assert.That(project.PrInvoiceTypeId, Is.EqualTo(3));
            Assert.That(project.PrInvoiceTypeAmount, Is.EqualTo("230.00"));
            Assert.That(project.PrBudgetTypeId, Is.EqualTo(1));
            Assert.That(project.PrBudgetTypeAmount, Is.EqualTo("200.00"));
            Assert.That(project.UserId, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterProject" /> is supplied, <c>ProjectService.Get</c> must
    ///     translate its <c>limit</c>, <c>offset</c>, and <c>order_by</c> values into query-string
    ///     parameters on the outgoing request.
    /// </summary>
    [Test]
    public async Task ProjectService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ProjectService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterProject(25, 100, "name"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
            Assert.That(request.RawQuery, Does.Contain("order_by=name"));
        });
    }

    /// <summary>
    ///     <c>ProjectService.GetById</c> must issue a <c>GET</c> request that includes the target id
    ///     in the URL path and deserialize every field of the <c>Project</c> schema.
    /// </summary>
    [Test]
    public async Task ProjectService_GetById_DeserializesFullProjectPayload()
    {
        const int id = 2;
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
            Assert.That(result.Data!.Id, Is.EqualTo(2));
            Assert.That(result.Data.Uuid, Is.EqualTo("046b6c7f-0b8a-43b9-b35d-6489e6daee91"));
            Assert.That(result.Data.Name, Is.EqualTo("Villa Kunterbunt"));
            Assert.That(result.Data.PrInvoiceTypeAmount, Is.EqualTo("230.00"));
            Assert.That(result.Data.PrBudgetTypeAmount, Is.EqualTo("200.00"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Create</c> must send a <c>POST</c> request whose body is the serialized
    ///     <see cref="ProjectCreate" /> payload — every property defined in the
    ///     <c>v2CreateProject</c> request body schema (including the write-only
    ///     <c>document_nr</c>) must reach the wire.
    /// </summary>
    [Test]
    public async Task ProjectService_Create_SendsPostRequestWithFullPayload()
    {
        Server
            .Given(Request.Create().WithPath(ProjectsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ProjectResponse));

        var service = new ProjectService(ConnectionHandler);

        var payload = new ProjectCreate(
            "Villa Kunterbunt",
            2,
            1,
            2,
            2,
            DocumentNr: "project name",
            StartDate: "2019-07-12 00:00:00",
            Comment: "",
            PrInvoiceTypeId: 3,
            PrInvoiceTypeAmount: "230.00",
            PrBudgetTypeId: 1,
            PrBudgetTypeAmount: "200.00");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectsPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Villa Kunterbunt\""));
            Assert.That(request.Body, Does.Contain("\"contact_id\":2"));
            Assert.That(request.Body, Does.Contain("\"user_id\":1"));
            Assert.That(request.Body, Does.Contain("\"pr_state_id\":2"));
            Assert.That(request.Body, Does.Contain("\"pr_project_type_id\":2"));
            Assert.That(request.Body, Does.Contain("\"document_nr\":\"project name\""));
            Assert.That(request.Body, Does.Contain("\"start_date\":\"2019-07-12 00:00:00\""));
            Assert.That(request.Body, Does.Contain("\"pr_invoice_type_id\":3"));
            Assert.That(request.Body, Does.Contain("\"pr_invoice_type_amount\":\"230.00\""));
            Assert.That(request.Body, Does.Contain("\"pr_budget_type_id\":1"));
            Assert.That(request.Body, Does.Contain("\"pr_budget_type_amount\":\"200.00\""));
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
        var responseBody = "[" + ProjectResponse + "]";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var service = new ProjectService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Villa Kunterbunt", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("Villa Kunterbunt"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Villa Kunterbunt\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
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
        const int id = 2;
        var expectedPath = $"{ProjectsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ProjectResponse));

        var service = new ProjectService(ConnectionHandler);

        var payload = new ProjectUpdate(
            "Villa Kunterbunt",
            2,
            1,
            2,
            2,
            StartDate: "2019-07-12 00:00:00",
            Comment: "",
            PrInvoiceTypeId: 3,
            PrInvoiceTypeAmount: "230.00",
            PrBudgetTypeId: 1,
            PrBudgetTypeAmount: "200.00");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(2));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Villa Kunterbunt\""));
        });
    }

    /// <summary>
    ///     <c>ProjectService.Archive</c> must send a body-less <c>POST</c> against
    ///     <c>/2.0/pr_project/{id}/archive</c>. The response shape is Bexio's
    ///     <c>SuccessResponse</c> <c>{ "success": true }</c> envelope.
    /// </summary>
    [Test]
    public async Task ProjectService_Archive_SendsPostRequestToArchivePath()
    {
        const int id = 2;
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
    ///     <c>/2.0/pr_project/{id}/reactivate</c>. The response shape is Bexio's
    ///     <c>SuccessResponse</c> <c>{ "success": true }</c> envelope.
    /// </summary>
    [Test]
    public async Task ProjectService_Reactivate_SendsPostRequestToReactivatePath()
    {
        const int id = 2;
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
    ///     in the URL path. The response shape is Bexio's <c>EntryDeleted</c>
    ///     <c>{ "success": true }</c> envelope.
    /// </summary>
    [Test]
    public async Task ProjectService_Delete_SendsDeleteRequestWithIdInPath()
    {
        const int id = 2;
        var expectedPath = $"{ProjectsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

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
