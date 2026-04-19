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
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations.Views;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Smoke.Contacts;

/// <summary>
///     Smoke tests covering the CRUD entry points of <see cref="ContactRelationService" /> against
///     WireMock stubs. Verifies the path composed from <see cref="ContactRelationConfiguration" />
///     (<c>2.0/contact_relation</c>) reaches the handler correctly, that the expected HTTP verbs are
///     used (including the Bexio-specific <c>POST</c> for edits), and that payloads are serialized with
///     the expected snake_case field names.
/// </summary>
public sealed class ContactRelationSmokeTests : IntegrationTestBase
{
    private const string ContactRelationPath = "/2.0/contact_relation";

    private const string ContactRelationResponse = """
                                                   {
                                                       "id": 1,
                                                       "contact_id": 10,
                                                       "contact_sub_id": 20,
                                                       "description": "Partner",
                                                       "updated_at": "2024-01-01 12:00:00"
                                                   }
                                                   """;

    /// <summary>
    ///     <c>ContactRelationService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/2.0/contact_relation</c> and return a successful <c>ApiResult</c> when the server
    ///     returns an empty array.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ContactRelationPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactRelationService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactRelationPath));
        });
    }

    /// <summary>
    ///     <c>ContactRelationService.GetById</c> must issue a <c>GET</c> request that includes the
    ///     target id in the URL path and surface the returned contact relation on success.
    /// </summary>
    [Test]
    public async Task ContactRelationService_GetById_SendsGetRequest()
    {
        const int id = 1;
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
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>ContactRelationService.Create</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="ContactRelationCreate" /> payload, and must surface the returned
    ///     contact relation on success.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(ContactRelationPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ContactRelationResponse));

        var service = new ContactRelationService(ConnectionHandler);

        var payload = new ContactRelationCreate(
            10,
            20,
            "Partner");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactRelationPath));
            Assert.That(request.Body, Does.Contain("\"contact_id\":10"));
            Assert.That(request.Body, Does.Contain("\"contact_sub_id\":20"));
        });
    }

    /// <summary>
    ///     <c>ContactRelationService.Search</c> must send a <c>POST</c> request against
    ///     <c>/2.0/contact_relation/search</c> with the <see cref="SearchCriteria" /> list as the
    ///     JSON body.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Search_SendsPostRequest_ToSearchPath()
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
        });
    }

    /// <summary>
    ///     <c>ContactRelationService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request
    ///     against <c>/2.0/contact_relation/{id}</c> — Bexio uses POST for full-replacement edits on
    ///     v2.0 resources.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{ContactRelationPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactRelationResponse));

        var service = new ContactRelationService(ConnectionHandler);

        var payload = new ContactRelationUpdate(
            10,
            20,
            "Partner");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>ContactRelationService.Delete</c> must issue a <c>DELETE</c> request that includes the
    ///     target id in the URL path.
    /// </summary>
    [Test]
    public async Task ContactRelationService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
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
}