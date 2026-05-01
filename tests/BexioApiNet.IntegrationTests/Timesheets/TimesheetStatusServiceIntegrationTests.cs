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

using BexioApiNet.Services.Connectors.Timesheets;

namespace BexioApiNet.IntegrationTests.Timesheets;

/// <summary>
/// Integration tests covering <see cref="TimesheetStatusService" />. The request path is
/// composed from <see cref="TimesheetStatusConfiguration" /> (<c>2.0/timesheet_status</c>)
/// and must reach WireMock intact when the service is driven through the real connection
/// handler. Stub bodies use the <c>v2TimeSheetStatus</c> schema so deserialization is
/// exercised end-to-end.
/// </summary>
public sealed class TimesheetStatusServiceIntegrationTests : IntegrationTestBase
{
    private const string TimesheetStatusPath = "/2.0/timesheet_status";

    /// <summary>
    /// <c>TimesheetStatusService.Get()</c> must issue a <c>GET</c> against
    /// <c>/2.0/timesheet_status</c> and return a successful <c>ApiResult</c> when the
    /// server responds with an empty collection.
    /// </summary>
    [Test]
    public async Task TimesheetStatusService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(TimesheetStatusPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TimesheetStatusService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(TimesheetStatusPath));
        });
    }

    /// <summary>
    /// When the server returns a populated list, <c>TimesheetStatusService.Get()</c> must
    /// deserialize each <c>v2TimeSheetStatus</c> entry intact (<c>id</c>, <c>name</c>).
    /// </summary>
    [Test]
    public async Task TimesheetStatusService_Get_DeserializesPopulatedList()
    {
        const string body = """
                            [
                                { "id": 1, "name": "Open" },
                                { "id": 2, "name": "In Progress" },
                                { "id": 4, "name": "Done" }
                            ]
                            """;

        Server
            .Given(Request.Create().WithPath(TimesheetStatusPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(body));

        var service = new TimesheetStatusService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Has.Count.EqualTo(3));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Open"));
            Assert.That(result.Data[1].Id, Is.EqualTo(2));
            Assert.That(result.Data[1].Name, Is.EqualTo("In Progress"));
            Assert.That(result.Data[2].Id, Is.EqualTo(4));
            Assert.That(result.Data[2].Name, Is.EqualTo("Done"));
        });
    }
}
