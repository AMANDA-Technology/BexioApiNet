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

using BexioApiNet.Abstractions.Models.Projects.Packages.Views;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="PackageService" />. The request path is composed
///     from <see cref="PackageConfiguration" /> and the parent project id
///     (<c>3.0/projects/{projectId}/packages</c>) and must reach WireMock intact when the
///     service is driven through the real connection handler.
/// </summary>
public sealed class PackageServiceIntegrationTests : IntegrationTestBase
{
    private const int ProjectId = 1;
    private const string PackagesPath = "/3.0/projects/1/packages";

    private const string PackageResponse = """
                                           {
                                               "id": 1,
                                               "name": "Package 1",
                                               "spent_time_in_hours": 0,
                                               "estimated_time_in_hours": 10,
                                               "comment": null,
                                               "pr_milestone_id": null
                                           }
                                           """;

    /// <summary>
    ///     <c>PackageService.GetAsync</c> must issue a <c>GET</c> against
    ///     <c>/3.0/projects/{projectId}/packages</c> and return a successful <c>ApiResult</c>
    ///     when the server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task PackageService_GetAsync_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(PackagesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PackageService(ConnectionHandler);

        var result = await service.GetAsync(ProjectId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PackagesPath));
        });
    }

    /// <summary>
    ///     <c>PackageService.GetByIdAsync</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and surface the returned work package on success.
    /// </summary>
    [Test]
    public async Task PackageService_GetByIdAsync_SendsGetRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{PackagesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PackageResponse));

        var service = new PackageService(ConnectionHandler);

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
    ///     <c>PackageService.CreateAsync</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="PackageCreate" /> payload and surface the returned work package
    ///     on success.
    /// </summary>
    [Test]
    public async Task PackageService_CreateAsync_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(PackagesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PackageResponse));

        var service = new PackageService(ConnectionHandler);

        var payload = new PackageCreate("Package 1", EstimatedTimeInHours: 10m);

        var result = await service.CreateAsync(ProjectId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PackagesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Package 1\""));
            Assert.That(request.Body, Does.Contain("\"estimated_time_in_hours\":10"));
        });
    }

    /// <summary>
    ///     <c>PackageService.PatchAsync</c> must send a <c>PATCH</c> request against
    ///     <c>/3.0/projects/{projectId}/packages/{id}</c> whose body is the serialized
    ///     <see cref="PackagePatch" /> payload.
    /// </summary>
    [Test]
    public async Task PackageService_PatchAsync_SendsPatchRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{PackagesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PackageResponse));

        var service = new PackageService(ConnectionHandler);

        var payload = new PackagePatch("Package Updated");

        var result = await service.PatchAsync(ProjectId, id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Package Updated\""));
        });
    }

    /// <summary>
    ///     <c>PackageService.DeleteAsync</c> must issue a <c>DELETE</c> request that includes
    ///     the target id in the URL path.
    /// </summary>
    [Test]
    public async Task PackageService_DeleteAsync_SendsDeleteRequestWithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{PackagesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new PackageService(ConnectionHandler);

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