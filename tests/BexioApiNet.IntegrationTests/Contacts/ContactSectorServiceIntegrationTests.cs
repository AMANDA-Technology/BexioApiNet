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
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the read-only entry points of <see cref="ContactSectorService" /> against
/// WireMock stubs. The Bexio API exposes contact sectors under the legacy path
/// <c>2.0/contact_branch</c> (see <see cref="ContactSectorConfiguration" />). This fixture verifies
/// that the URL is built correctly and that the expected HTTP verbs are used. The service only
/// supports <c>Get</c> and <c>Search</c> — there are no Create, Update, or Delete endpoints.
/// </summary>
public sealed class ContactSectorServiceIntegrationTests : IntegrationTestBase
{
    private const string ContactBranchPath = "/2.0/contact_branch";

    private const string ContactSectorResponse = """
                                                 {
                                                     "id": 1,
                                                     "name": "Photography"
                                                 }
                                                 """;

    /// <summary>
    /// <c>ContactSectorService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/contact_branch</c> and return a successful <c>ApiResult</c> when the server
    /// returns an empty array.
    /// </summary>
    [Test]
    public async Task ContactSectorService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ContactBranchPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactSectorService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactBranchPath));
        });
    }

    /// <summary>
    /// <c>ContactSectorService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact_branch/search</c> with the <see cref="SearchCriteria" /> list as the JSON
    /// body.
    /// </summary>
    [Test]
    public async Task ContactSectorService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{ContactBranchPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactSectorResponse}]"));

        var service = new ContactSectorService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Photography", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
        });
    }
}
