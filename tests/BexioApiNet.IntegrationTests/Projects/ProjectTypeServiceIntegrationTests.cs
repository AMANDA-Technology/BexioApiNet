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

using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="ProjectTypeService" />. The request path is composed
///     from <see cref="ProjectConfiguration" /> (<c>2.0/pr_project_type</c>) and must reach WireMock
///     intact when the service is driven through the real connection handler. Response payloads
///     mirror the
///     <see href="https://docs.bexio.com/#tag/Projects/operation/v2ListProjectType">List Project Type</see>
///     OpenAPI schema exactly.
/// </summary>
public sealed class ProjectTypeServiceIntegrationTests : IntegrationTestBase
{
    private const string ProjectTypesPath = "/2.0/pr_project_type";

    /// <summary>
    ///     <c>ProjectTypeService.Get()</c> must issue a <c>GET</c> against <c>/2.0/pr_project_type</c>
    ///     and deserialize a fully populated <c>ProjectType</c> payload — the schema only defines
    ///     <c>id</c> and <c>name</c>, both of which must round-trip into the <c>ProjectType</c>
    ///     record.
    /// </summary>
    [Test]
    public async Task ProjectTypeService_Get_DeserializesFullProjectTypePayload()
    {
        const string responseBody = """
                                    [
                                        { "id": 1, "name": "Internal Project" },
                                        { "id": 2, "name": "Customer Project" }
                                    ]
                                    """;

        Server
            .Given(Request.Create().WithPath(ProjectTypesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseBody));

        var service = new ProjectTypeService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Internal Project"));
            Assert.That(result.Data[1].Id, Is.EqualTo(2));
            Assert.That(result.Data[1].Name, Is.EqualTo("Customer Project"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectTypesPath));
        });
    }

    /// <summary>
    ///     When a <see cref="QueryParameterProjectType" /> is supplied, <c>ProjectTypeService.Get</c>
    ///     must translate its <c>order_by</c> value into a query-string parameter on the outgoing
    ///     request — the OpenAPI <c>v2ListProjectType</c> operation accepts an <c>order_by</c>
    ///     sort clause.
    /// </summary>
    [Test]
    public async Task ProjectTypeService_Get_WithQueryParams_AppendsOrderBy()
    {
        Server
            .Given(Request.Create().WithPath(ProjectTypesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ProjectTypeService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterProjectType("name"),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectTypesPath));
            Assert.That(request.RawQuery, Does.Contain("order_by=name"));
        });
    }
}
