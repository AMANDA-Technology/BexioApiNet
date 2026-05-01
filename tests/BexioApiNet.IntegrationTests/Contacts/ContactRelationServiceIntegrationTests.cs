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
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations;
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="ContactRelationService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="ContactRelationConfiguration" />
/// (<c>2.0/contact_relation</c>) reaches the handler correctly, that the expected HTTP verbs are
/// used (including the Bexio-specific <c>POST</c> for edits), and that payloads are serialized with
/// the expected snake_case field names. Each list / read response stub matches the OpenAPI
/// <c>ContactRelation</c> shape (5 properties) and asserts deserialization on every property.
/// </summary>
public sealed class ContactRelationServiceIntegrationTests : IntegrationTestBase
{
    private const string ContactRelationPath = "/2.0/contact_relation";

    /// <summary>
    /// Fully-populated <c>ContactRelation</c> response body — covers every property in the
    /// Bexio v3 OpenAPI <c>ContactRelation</c> schema (<c>id</c>, <c>contact_id</c>,
    /// <c>contact_sub_id</c>, <c>description</c>, <c>updated_at</c>).
    /// </summary>
    private const string ContactRelationResponse = """
                                                   {
                                                       "id": 3,
                                                       "contact_id": 10,
                                                       "contact_sub_id": 20,
                                                       "description": "Partner",
                                                       "updated_at": "2024-01-01 12:00:00"
                                                   }
                                                   """;

    /// <summary>
    /// <c>ContactRelationService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/contact_relation</c> and deserialize the array body into a list of fully-populated
    /// <c>ContactRelation</c> records.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Get_SendsGetRequest_AndDeserializesContactRelation()
    {
        Server
            .Given(Request.Create().WithPath(ContactRelationPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactRelationResponse}]"));

        var service = new ContactRelationService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactRelationPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertContactRelationFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>ContactRelationService.Get()</c> serializes <c>limit</c>, <c>offset</c>, and
    /// <c>order_by</c> onto the request URL when a populated <see cref="QueryParameterContactRelation"/>
    /// is supplied. Verifies the query parameter names match the Bexio v3 spec.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Get_WithQueryParameters_SerializesQueryString()
    {
        Server
            .Given(Request.Create().WithPath(ContactRelationPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactRelationService(ConnectionHandler);

        var queryParameter = new QueryParameterContactRelation(Limit: 25, Offset: 50, OrderBy: "contact_id");

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.RawQuery, Does.Contain("limit=25"));
        Assert.That(request.RawQuery, Does.Contain("offset=50"));
        Assert.That(request.RawQuery, Does.Contain("order_by=contact_id"));
    }

    /// <summary>
    /// <c>ContactRelationService.GetById</c> must issue a <c>GET</c> request that includes the
    /// target id in the URL path and deserialize the returned <c>ContactRelation</c> body.
    /// </summary>
    [Test]
    public async Task ContactRelationService_GetById_SendsGetRequest_AndDeserializesContactRelation()
    {
        const int id = 3;
        var expectedPath = $"{ContactRelationPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactRelationResponse));

        var service = new ContactRelationService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertContactRelationFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactRelationService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="ContactRelationCreate" /> payload, and must surface the returned
    /// contact relation on success with all properties populated.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Create_SendsPostRequest_AndDeserializesContactRelation()
    {
        Server
            .Given(Request.Create().WithPath(ContactRelationPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ContactRelationResponse));

        var service = new ContactRelationService(ConnectionHandler);

        var payload = new ContactRelationCreate(
            ContactId: 10,
            ContactSubId: 20,
            Description: "Partner");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactRelationPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":10"));
            Assert.That(request.Body, Does.Contain("\"contact_sub_id\":20"));
            Assert.That(request.Body, Does.Contain("\"description\":\"Partner\""));
        });

        AssertContactRelationFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactRelationService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact_relation/search</c> with the <see cref="SearchCriteria" /> list as the
    /// JSON body and deserialize the array response with full property coverage on every item.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Search_SendsPostRequest_ToSearchPath_AndDeserializesArray()
    {
        var expectedPath = $"{ContactRelationPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactRelationResponse}]"));

        var service = new ContactRelationService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "contact_id", Value = "10", Criteria = "=" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"contact_id\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"10\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertContactRelationFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>ContactRelationService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request
    /// against <c>/2.0/contact_relation/{id}</c> — Bexio uses POST for full-replacement edits on
    /// v2.0 resources.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Update_SendsPostRequest_WithIdInPath_AndDeserializesContactRelation()
    {
        const int id = 3;
        var expectedPath = $"{ContactRelationPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactRelationResponse));

        var service = new ContactRelationService(ConnectionHandler);

        var payload = new ContactRelationUpdate(
            ContactId: 10,
            ContactSubId: 20,
            Description: "Partner");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertContactRelationFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactRelationService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 3;
        var expectedPath = $"{ContactRelationPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ContactRelationService(ConnectionHandler);

        var result = await service.Delete(idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// Verifies that every property in a <c>ContactRelation</c>-shaped JSON response is mapped onto
    /// the corresponding C# property.
    /// </summary>
    private static void AssertContactRelationFullyDeserialized(ContactRelation contactRelation)
    {
        Assert.Multiple(() =>
        {
            Assert.That(contactRelation.Id, Is.EqualTo(3));
            Assert.That(contactRelation.ContactId, Is.EqualTo(10));
            Assert.That(contactRelation.ContactSubId, Is.EqualTo(20));
            Assert.That(contactRelation.Description, Is.EqualTo("Partner"));
            Assert.That(contactRelation.UpdatedAt, Is.EqualTo("2024-01-01 12:00:00"));
        });
    }
}
