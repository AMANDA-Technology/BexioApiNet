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

using BexioApiNet.Abstractions.Models.Projects.Milestones.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="MilestoneService" />. The request path is composed
///     from <see cref="MilestoneConfiguration" /> and the parent project id
///     (<c>3.0/projects/{projectId}/milestones</c>) and must reach WireMock intact when the
///     service is driven through the real connection handler. Response payloads mirror the
///     <see href="https://docs.bexio.com/#tag/Projects/operation/ListMilestones">List Milestones</see>
///     OpenAPI schema exactly.
/// </summary>
public sealed class MilestoneServiceIntegrationTests : IntegrationTestBase
{
    private const int ProjectId = 1;
    private const string MilestonesPath = "/3.0/projects/1/milestones";

    /// <summary>
    ///     Fully populated <c>Milestone</c> JSON payload — every property defined by the
    ///     <c>Milestone</c> schema in <c>doc/openapi/bexio-v3.json</c> is present so the test
    ///     verifies real deserialization rather than an empty stub.
    /// </summary>
    private const string MilestoneResponse = """
                                             {
                                                 "id": 4,
                                                 "name": "project documentation",
                                                 "end_date": "2018-05-18",
                                                 "comment": "Finish project documentation.",
                                                 "pr_parent_milestone_id": 3
                                             }
                                             """;

    /// <summary>
    ///     <c>MilestoneService.GetAsync</c> must issue a <c>GET</c> against
    ///     <c>/3.0/projects/{projectId}/milestones</c> and deserialize a fully populated milestone
    ///     payload — every field from the OpenAPI schema must round-trip into the <c>Milestone</c>
    ///     record without loss.
    /// </summary>
    [Test]
    public async Task MilestoneService_GetAsync_DeserializesFullMilestonePayload()
    {
        var responseBody = "[" + MilestoneResponse + "]";

        Server
            .Given(Request.Create().WithPath(MilestonesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var service = new MilestoneService(ConnectionHandler);

        var result = await service.GetAsync(ProjectId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(4));
            Assert.That(result.Data[0].Name, Is.EqualTo("project documentation"));
            Assert.That(result.Data[0].EndDate, Is.EqualTo(new DateOnly(2018, 5, 18)));
            Assert.That(result.Data[0].Comment, Is.EqualTo("Finish project documentation."));
            Assert.That(result.Data[0].ParentMilestoneId, Is.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(MilestonesPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterMilestone" /> is supplied, <c>MilestoneService.GetAsync</c>
    ///     must translate its <c>limit</c> and <c>offset</c> values into query-string parameters on
    ///     the outgoing request so pagination matches the OpenAPI <c>ListMilestones</c> contract.
    /// </summary>
    [Test]
    public async Task MilestoneService_GetAsync_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(MilestonesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new MilestoneService(ConnectionHandler);

        var result = await service.GetAsync(
            ProjectId,
            new QueryParameterMilestone(20, 100),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(MilestonesPath));
            Assert.That(request.RawQuery, Does.Contain("limit=20"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.GetByIdAsync</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and deserialize every field of the <c>Milestone</c> schema.
    /// </summary>
    [Test]
    public async Task MilestoneService_GetByIdAsync_DeserializesFullMilestonePayload()
    {
        const int id = 4;
        var expectedPath = $"{MilestonesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(MilestoneResponse));

        var service = new MilestoneService(ConnectionHandler);

        var result = await service.GetByIdAsync(ProjectId, id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.Name, Is.EqualTo("project documentation"));
            Assert.That(result.Data.EndDate, Is.EqualTo(new DateOnly(2018, 5, 18)));
            Assert.That(result.Data.Comment, Is.EqualTo("Finish project documentation."));
            Assert.That(result.Data.ParentMilestoneId, Is.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.CreateAsync</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="MilestoneCreate" /> payload — every property defined in the
    ///     <c>CreateMilestone</c> request body schema must reach the wire.
    /// </summary>
    [Test]
    public async Task MilestoneService_CreateAsync_SendsPostRequestWithFullPayload()
    {
        Server
            .Given(Request.Create().WithPath(MilestonesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(MilestoneResponse));

        var service = new MilestoneService(ConnectionHandler);

        var payload = new MilestoneCreate(
            "project documentation",
            new DateOnly(2018, 5, 18),
            "Finish project documentation.",
            3);

        var result = await service.CreateAsync(ProjectId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.Name, Is.EqualTo("project documentation"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(MilestonesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"project documentation\""));
            Assert.That(request.Body, Does.Contain("\"end_date\":\"2018-05-18\""));
            Assert.That(request.Body, Does.Contain("\"comment\":\"Finish project documentation.\""));
            Assert.That(request.Body, Does.Contain("\"pr_parent_milestone_id\":3"));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.UpdateAsync</c> must send a <c>POST</c> request against
    ///     <c>/3.0/projects/{projectId}/milestones/{id}</c> — Bexio uses <c>POST</c> (not <c>PUT</c>)
    ///     for the <c>EditMilestone</c> operation per <c>doc/openapi/bexio-v3.json</c>. The body
    ///     must carry the full <see cref="MilestoneUpdate" /> payload and the response must
    ///     deserialize end-to-end.
    /// </summary>
    [Test]
    public async Task MilestoneService_UpdateAsync_SendsPostRequestWithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{MilestonesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(MilestoneResponse));

        var service = new MilestoneService(ConnectionHandler);

        var payload = new MilestoneUpdate(
            "project documentation",
            new DateOnly(2018, 5, 18),
            "Finish project documentation.",
            3);

        var result = await service.UpdateAsync(ProjectId, id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"project documentation\""));
            Assert.That(request.Body, Does.Contain("\"end_date\":\"2018-05-18\""));
            Assert.That(request.Body, Does.Contain("\"comment\":\"Finish project documentation.\""));
            Assert.That(request.Body, Does.Contain("\"pr_parent_milestone_id\":3"));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.DeleteAsync</c> must issue a <c>DELETE</c> request that includes
    ///     the target id in the URL path. The response shape is Bexio's <c>EntryDeleted</c>
    ///     <c>{ "success": true }</c> envelope.
    /// </summary>
    [Test]
    public async Task MilestoneService_DeleteAsync_SendsDeleteRequestWithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{MilestonesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new MilestoneService(ConnectionHandler);

        var result = await service.DeleteAsync(ProjectId, id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
