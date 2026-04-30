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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the read-only entry points of <see cref="ContactSectorService" /> against
/// WireMock stubs. The Bexio API exposes contact sectors under the legacy path
/// <c>2.0/contact_branch</c> (see <see cref="ContactSectorConfiguration" />). This fixture verifies
/// that the URL is built correctly, that the expected HTTP verbs are used, and that the OpenAPI
/// <c>ContactSector</c> shape (<c>id</c> + <c>name</c>) deserializes onto every property of the
/// model. The Bexio v3 spec only exposes <c>Get</c> and <c>Search</c> — there are no Create,
/// Update, or Delete endpoints for contact sectors.
/// </summary>
public sealed class ContactSectorServiceIntegrationTests : IntegrationTestBase
{
    private const string ContactBranchPath = "/2.0/contact_branch";

    /// <summary>
    /// Fully-populated <c>ContactSector</c> response body — covers every property in the
    /// Bexio v3 OpenAPI spec for the <c>ContactSector</c> schema (<c>id</c>, <c>name</c>).
    /// </summary>
    private const string ContactSectorResponse = """
                                                 {
                                                     "id": 3,
                                                     "name": "Photography"
                                                 }
                                                 """;

    /// <summary>
    /// <c>ContactSectorService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/contact_branch</c> and deserialize the array body into a list of fully-populated
    /// <c>ContactSector</c> records.
    /// </summary>
    [Test]
    public async Task ContactSectorService_Get_SendsGetRequest_AndDeserializesContactSector()
    {
        Server
            .Given(Request.Create().WithPath(ContactBranchPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactSectorResponse}]"));

        var service = new ContactSectorService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactBranchPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(3));
            Assert.That(result.Data[0].Name, Is.EqualTo("Photography"));
        });
    }

    /// <summary>
    /// <c>ContactSectorService.Get()</c> serializes <c>limit</c>, <c>offset</c> and <c>order_by</c>
    /// onto the request URL when a populated <see cref="QueryParameterContactSector"/> is supplied.
    /// Verifies the query parameter names match the Bexio v3 spec.
    /// </summary>
    [Test]
    public async Task ContactSectorService_Get_WithQueryParameters_SerializesQueryString()
    {
        Server
            .Given(Request.Create().WithPath(ContactBranchPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactSectorService(ConnectionHandler);

        var queryParameter = new QueryParameterContactSector(Limit: 50, Offset: 100, OrderBy: "name");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.RawQuery, Does.Contain("limit=50"));
        Assert.That(request.RawQuery, Does.Contain("offset=100"));
        Assert.That(request.RawQuery, Does.Contain("order_by=name"));
    }

    /// <summary>
    /// <c>ContactSectorService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact_branch/search</c> with the <see cref="SearchCriteria" /> list as the JSON
    /// body and deserialize the array response with full property coverage on every item.
    /// </summary>
    [Test]
    public async Task ContactSectorService_Search_SendsPostRequest_ToSearchPath_AndDeserializesArray()
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
            Assert.That(request.Body, Does.Contain("\"value\":\"Photography\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(3));
            Assert.That(result.Data[0].Name, Is.EqualTo("Photography"));
        });
    }
}
