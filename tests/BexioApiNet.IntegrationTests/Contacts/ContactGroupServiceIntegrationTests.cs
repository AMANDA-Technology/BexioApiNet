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
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="ContactGroupService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="ContactGroupConfiguration" />
/// (<c>2.0/contact_group</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits), and that payloads are serialized with the
/// expected snake_case field names. Each list / read response stub carries the exact OpenAPI
/// shape (id + name) so deserialization is asserted on every property.
/// </summary>
public sealed class ContactGroupServiceIntegrationTests : IntegrationTestBase
{
    private const string ContactGroupPath = "/2.0/contact_group";

    /// <summary>
    /// Fully-populated <c>ContactGroup</c> response body — covers every property in the
    /// Bexio v3 OpenAPI <c>ContactGroup</c> schema (read-only <c>id</c> and required <c>name</c>).
    /// </summary>
    private const string ContactGroupResponse = """
                                                {
                                                    "id": 7,
                                                    "name": "VIP Customers"
                                                }
                                                """;

    /// <summary>
    /// <c>ContactGroupService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/contact_group</c> and deserialize the array body into a list of fully-populated
    /// <c>ContactGroup</c> records.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Get_SendsGetRequest_AndDeserializesContactGroup()
    {
        Server
            .Given(Request.Create().WithPath(ContactGroupPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactGroupResponse}]"));

        var service = new ContactGroupService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactGroupPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(7));
            Assert.That(result.Data[0].Name, Is.EqualTo("VIP Customers"));
        });
    }

    /// <summary>
    /// <c>ContactGroupService.Get()</c> serializes <c>limit</c>, <c>offset</c>, and <c>order_by</c>
    /// onto the request URL when a populated <see cref="QueryParameterContactGroup"/> is supplied.
    /// Verifies the query parameter names match the Bexio v3 spec.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Get_WithQueryParameters_SerializesQueryString()
    {
        Server
            .Given(Request.Create().WithPath(ContactGroupPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactGroupService(ConnectionHandler);

        var queryParameter = new QueryParameterContactGroup(Limit: 50, Offset: 100, OrderBy: "name");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.RawQuery, Does.Contain("limit=50"));
        Assert.That(request.RawQuery, Does.Contain("offset=100"));
        Assert.That(request.RawQuery, Does.Contain("order_by=name"));
    }

    /// <summary>
    /// <c>ContactGroupService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// id in the URL path and deserialize the returned <c>ContactGroup</c> body.
    /// </summary>
    [Test]
    public async Task ContactGroupService_GetById_SendsGetRequest_AndDeserializesContactGroup()
    {
        const int id = 7;
        var expectedPath = $"{ContactGroupPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactGroupResponse));

        var service = new ContactGroupService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Name, Is.EqualTo("VIP Customers"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ContactGroupService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="ContactGroupCreate" /> payload (with the snake_case <c>name</c> field),
    /// and must surface the returned contact group on success with all properties populated.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Create_SendsPostRequest_AndDeserializesContactGroup()
    {
        Server
            .Given(Request.Create().WithPath(ContactGroupPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ContactGroupResponse));

        var service = new ContactGroupService(ConnectionHandler);

        var payload = new ContactGroupCreate("VIP Customers");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(7));
            Assert.That(result.Data.Name, Is.EqualTo("VIP Customers"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactGroupPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"VIP Customers\""));
        });
    }

    /// <summary>
    /// <c>ContactGroupService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact_group/search</c> with the <see cref="SearchCriteria" /> list as the JSON
    /// body and deserialize the array response with full property coverage on every item.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Search_SendsPostRequest_ToSearchPath_AndDeserializesArray()
    {
        var expectedPath = $"{ContactGroupPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactGroupResponse}]"));

        var service = new ContactGroupService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "VIP", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"VIP\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(7));
            Assert.That(result.Data[0].Name, Is.EqualTo("VIP Customers"));
        });
    }

    /// <summary>
    /// <c>ContactGroupService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/contact_group/{id}</c> — Bexio uses POST for full-replacement edits on v2.0 resources.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Update_SendsPostRequest_WithIdInPath_AndDeserializesContactGroup()
    {
        const int id = 7;
        var expectedPath = $"{ContactGroupPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactGroupResponse));

        var service = new ContactGroupService(ConnectionHandler);

        var payload = new ContactGroupUpdate("VIP Customers");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Name, Is.EqualTo("VIP Customers"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"VIP Customers\""));
        });
    }

    /// <summary>
    /// <c>ContactGroupService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path and surface Bexio's <c>EntryDeleted</c> envelope.
    /// </summary>
    [Test]
    public async Task ContactGroupService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 7;
        var expectedPath = $"{ContactGroupPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ContactGroupService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
