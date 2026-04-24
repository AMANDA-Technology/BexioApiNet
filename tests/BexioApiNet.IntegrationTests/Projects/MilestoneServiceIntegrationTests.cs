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
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="MilestoneService" />. The request path is composed
///     from <see cref="MilestoneConfiguration" /> and the parent project id
///     (<c>3.0/projects/{projectId}/milestones</c>) and must reach WireMock intact when the
///     service is driven through the real connection handler.
/// </summary>
public sealed class MilestoneServiceIntegrationTests : IntegrationTestBase
{
    private const int ProjectId = 1;
    private const string MilestonesPath = "/3.0/projects/1/milestones";

    private const string MilestoneResponse = """
                                             {
                                                 "id": 1,
                                                 "name": "Milestone 1",
                                                 "end_date": "2026-12-31",
                                                 "comment": null,
                                                 "pr_parent_milestone_id": null
                                             }
                                             """;

    /// <summary>
    ///     <c>MilestoneService.GetAsync</c> must issue a <c>GET</c> against
    ///     <c>/3.0/projects/{projectId}/milestones</c> and return a successful <c>ApiResult</c>
    ///     when the server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task MilestoneService_GetAsync_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(MilestonesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new MilestoneService(ConnectionHandler);

        var result = await service.GetAsync(ProjectId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(MilestonesPath));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.GetByIdAsync</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and surface the returned milestone on success.
    /// </summary>
    [Test]
    public async Task MilestoneService_GetByIdAsync_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
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
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.CreateAsync</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="MilestoneCreate" /> payload and surface the returned milestone
    ///     on success.
    /// </summary>
    [Test]
    public async Task MilestoneService_CreateAsync_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(MilestonesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(MilestoneResponse));

        var service = new MilestoneService(ConnectionHandler);

        var payload = new MilestoneCreate("Milestone 1", new DateOnly(2026, 12, 31));

        var result = await service.CreateAsync(ProjectId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(MilestonesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Milestone 1\""));
            Assert.That(request.Body, Does.Contain("\"end_date\":\"2026-12-31\""));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.UpdateAsync</c> must send a <c>PUT</c> request against
    ///     <c>/3.0/projects/{projectId}/milestones/{id}</c> whose body is the serialized
    ///     <see cref="MilestoneUpdate" /> payload.
    /// </summary>
    [Test]
    public async Task MilestoneService_UpdateAsync_SendsPutRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{MilestonesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(MilestoneResponse));

        var service = new MilestoneService(ConnectionHandler);

        var payload = new MilestoneUpdate("Milestone Updated");

        var result = await service.UpdateAsync(ProjectId, id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Milestone Updated\""));
        });
    }

    /// <summary>
    ///     <c>MilestoneService.DeleteAsync</c> must issue a <c>DELETE</c> request that includes
    ///     the target id in the URL path.
    /// </summary>
    [Test]
    public async Task MilestoneService_DeleteAsync_SendsDeleteRequestWithIdInPath()
    {
        const int id = 1;
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