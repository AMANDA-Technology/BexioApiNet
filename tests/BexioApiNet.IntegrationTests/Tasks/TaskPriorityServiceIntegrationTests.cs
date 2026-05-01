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

using BexioApiNet.Services.Connectors.Tasks;

namespace BexioApiNet.IntegrationTests.Tasks;

/// <summary>
///     Integration tests covering <see cref="TaskPriorityService" />. The request path is composed
///     from <see cref="TaskPriorityConfiguration" /> (<c>2.0/task_priority</c>) and must reach
///     WireMock intact when the service is driven through the real connection handler. Responses
///     mirror the OpenAPI <c>task_priority</c> schema (<c>id</c>, <c>name</c>) so deserialization
///     of every documented field can be asserted end-to-end.
/// </summary>
public sealed class TaskPriorityServiceIntegrationTests : IntegrationTestBase
{
    private const string TaskPrioritiesPath = "/2.0/task_priority";

    /// <summary>
    ///     Fully populated v2 Bexio <c>task_priority</c> response body matching the OpenAPI
    ///     <c>task_priority</c> schema. Includes every documented field.
    /// </summary>
    private const string TaskPrioritiesResponse = """
                                                  [
                                                      { "id": 1, "name": "High" },
                                                      { "id": 2, "name": "Medium" },
                                                      { "id": 3, "name": "Low" }
                                                  ]
                                                  """;

    /// <summary>
    ///     <c>TaskPriorityService.Get()</c> must issue a <c>GET</c> against
    ///     <c>/2.0/task_priority</c> and deserialize every documented field of the
    ///     <c>task_priority</c> schema — currently <c>id</c> and <c>name</c>.
    /// </summary>
    [Test]
    public async Task TaskPriorityService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(TaskPrioritiesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TaskPrioritiesResponse));

        var service = new TaskPriorityService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TaskPrioritiesPath));
            Assert.That(result.Data, Has.Count.EqualTo(3));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("High"));
            Assert.That(result.Data[1].Id, Is.EqualTo(2));
            Assert.That(result.Data[1].Name, Is.EqualTo("Medium"));
            Assert.That(result.Data[2].Id, Is.EqualTo(3));
            Assert.That(result.Data[2].Name, Is.EqualTo("Low"));
        });
    }

    /// <summary>
    ///     When Bexio responds with an empty array, <c>TaskPriorityService.Get()</c> must surface
    ///     an empty collection on a successful <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task TaskPriorityService_Get_WithEmptyResponse_ReturnsEmptyCollection()
    {
        Server
            .Given(Request.Create().WithPath(TaskPrioritiesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TaskPriorityService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        });
    }
}
