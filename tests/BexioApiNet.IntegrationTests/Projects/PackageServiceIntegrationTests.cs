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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="PackageService" />. The request path is composed
///     from <see cref="PackageConfiguration" /> and the parent project id
///     (<c>3.0/projects/{projectId}/packages</c>) and must reach WireMock intact when the
///     service is driven through the real connection handler. Response payloads mirror the
///     <see href="https://docs.bexio.com/#tag/Projects/operation/ListWorkPackages">List Work Packages</see>
///     OpenAPI schema exactly.
/// </summary>
public sealed class PackageServiceIntegrationTests : IntegrationTestBase
{
    private const int ProjectId = 1;
    private const string PackagesPath = "/3.0/projects/1/packages";

    /// <summary>
    ///     Fully populated <c>Work Package</c> JSON payload — every property defined by the
    ///     <c>Work Package</c> schema in <c>doc/openapi/bexio-v3.json</c> is present so the test
    ///     verifies real deserialization rather than an empty stub.
    /// </summary>
    private const string PackageResponse = """
                                           {
                                               "id": 4,
                                               "name": "Documentation",
                                               "spent_time_in_hours": 0.5,
                                               "estimated_time_in_hours": 1.75,
                                               "comment": "Crete project documentation",
                                               "pr_milestone_id": 3
                                           }
                                           """;

    /// <summary>
    ///     <c>PackageService.GetAsync</c> must issue a <c>GET</c> against
    ///     <c>/3.0/projects/{projectId}/packages</c> and deserialize a fully populated work-package
    ///     payload — every field from the OpenAPI schema must round-trip into the <c>Package</c>
    ///     record without loss.
    /// </summary>
    [Test]
    public async Task PackageService_GetAsync_DeserializesFullPackagePayload()
    {
        var responseBody = "[" + PackageResponse + "]";

        Server
            .Given(Request.Create().WithPath(PackagesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var service = new PackageService(ConnectionHandler);

        var result = await service.GetAsync(ProjectId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(4));
            Assert.That(result.Data[0].Name, Is.EqualTo("Documentation"));
            Assert.That(result.Data[0].SpentTimeInHours, Is.EqualTo(0.5m));
            Assert.That(result.Data[0].EstimatedTimeInHours, Is.EqualTo(1.75m));
            Assert.That(result.Data[0].Comment, Is.EqualTo("Crete project documentation"));
            Assert.That(result.Data[0].MilestoneId, Is.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PackagesPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterPackage" /> is supplied, <c>PackageService.GetAsync</c>
    ///     must translate its <c>limit</c> and <c>offset</c> values into query-string parameters
    ///     on the outgoing request so pagination matches the OpenAPI <c>ListWorkPackages</c>
    ///     contract.
    /// </summary>
    [Test]
    public async Task PackageService_GetAsync_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(PackagesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PackageService(ConnectionHandler);

        var result = await service.GetAsync(
            ProjectId,
            new QueryParameterPackage(20, 100),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PackagesPath));
            Assert.That(request.RawQuery, Does.Contain("limit=20"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>PackageService.GetByIdAsync</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and deserialize every field of the <c>Work Package</c>
    ///     schema.
    /// </summary>
    [Test]
    public async Task PackageService_GetByIdAsync_DeserializesFullPackagePayload()
    {
        const int id = 4;
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
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.Name, Is.EqualTo("Documentation"));
            Assert.That(result.Data.SpentTimeInHours, Is.EqualTo(0.5m));
            Assert.That(result.Data.EstimatedTimeInHours, Is.EqualTo(1.75m));
            Assert.That(result.Data.Comment, Is.EqualTo("Crete project documentation"));
            Assert.That(result.Data.MilestoneId, Is.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>PackageService.CreateAsync</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="PackageCreate" /> payload — every property defined in the
    ///     <c>CreateWorkPackage</c> request body schema must reach the wire.
    /// </summary>
    [Test]
    public async Task PackageService_CreateAsync_SendsPostRequestWithFullPayload()
    {
        Server
            .Given(Request.Create().WithPath(PackagesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PackageResponse));

        var service = new PackageService(ConnectionHandler);

        var payload = new PackageCreate(
            "Documentation",
            0.5m,
            1.75m,
            "Crete project documentation",
            3);

        var result = await service.CreateAsync(ProjectId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.MilestoneId, Is.EqualTo(3));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(PackagesPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Documentation\""));
            Assert.That(request.Body, Does.Contain("\"spent_time_in_hours\":0.5"));
            Assert.That(request.Body, Does.Contain("\"estimated_time_in_hours\":1.75"));
            Assert.That(request.Body, Does.Contain("\"comment\":\"Crete project documentation\""));
            Assert.That(request.Body, Does.Contain("\"pr_milestone_id\":3"));
        });
    }

    /// <summary>
    ///     <c>PackageService.PatchAsync</c> must send a <c>PATCH</c> request against
    ///     <c>/3.0/projects/{projectId}/packages/{id}</c> whose body is the serialized
    ///     <see cref="PackagePatch" /> payload. Bexio's <c>EditWorkPackage</c> spec uses
    ///     <c>PATCH</c> (not <c>POST</c>) for the partial-update edit on this resource.
    /// </summary>
    [Test]
    public async Task PackageService_PatchAsync_SendsPatchRequestWithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{PackagesPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PackageResponse));

        var service = new PackageService(ConnectionHandler);

        var payload = new PackagePatch(
            "Documentation",
            0.5m,
            1.75m,
            "Crete project documentation",
            3);

        var result = await service.PatchAsync(ProjectId, id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Documentation\""));
            Assert.That(request.Body, Does.Contain("\"spent_time_in_hours\":0.5"));
            Assert.That(request.Body, Does.Contain("\"estimated_time_in_hours\":1.75"));
            Assert.That(request.Body, Does.Contain("\"comment\":\"Crete project documentation\""));
            Assert.That(request.Body, Does.Contain("\"pr_milestone_id\":3"));
        });
    }

    /// <summary>
    ///     <c>PackageService.DeleteAsync</c> must issue a <c>DELETE</c> request that includes
    ///     the target id in the URL path. The response shape is Bexio's <c>EntryDeleted</c>
    ///     <c>{ "success": true }</c> envelope.
    /// </summary>
    [Test]
    public async Task PackageService_DeleteAsync_SendsDeleteRequestWithIdInPath()
    {
        const int id = 4;
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
