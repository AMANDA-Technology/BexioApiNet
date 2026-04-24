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

using BexioApiNet.Services.Connectors.Projects;

namespace BexioApiNet.IntegrationTests.Projects;

/// <summary>
///     Integration tests covering <see cref="ProjectTypeService" />. The request path is composed
///     from <see cref="ProjectConfiguration" /> (<c>2.0/pr_project_type</c>) and must reach WireMock
///     intact when the service is driven through the real connection handler.
/// </summary>
public sealed class ProjectTypeServiceIntegrationTests : IntegrationTestBase
{
    private const string ProjectTypesPath = "/2.0/pr_project_type";

    /// <summary>
    ///     <c>ProjectTypeService.Get()</c> must issue a <c>GET</c> against <c>/2.0/pr_project_type</c>
    ///     and deserialize the returned project type entries.
    /// </summary>
    [Test]
    public async Task ProjectTypeService_Get_SendsGetRequestToCorrectPath()
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

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Name, Is.EqualTo("Internal Project"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ProjectTypesPath));
        });
    }
}
