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
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Smoke.Contacts;

/// <summary>
/// Smoke tests covering the CRUD entry points of <see cref="ContactService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="ContactConfiguration"/>
/// (<c>2.0/contact</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits and <c>PATCH</c> for restore), and that
/// payloads are serialized with the expected snake_case field names.
/// </summary>
public sealed class ContactSmokeTests : IntegrationTestBase
{
    private const string ContactsPath = "/2.0/contact";

    private const string ContactResponse = """
        {
            "id": 1,
            "nr": "1000",
            "contact_type_id": 1,
            "name_1": "Acme AG",
            "name_2": null,
            "salutation_id": null,
            "salutation_form": null,
            "title_id": null,
            "birthday": null,
            "address": null,
            "street_name": null,
            "house_number": null,
            "address_addition": null,
            "postcode": null,
            "city": null,
            "country_id": null,
            "mail": null,
            "mail_second": null,
            "phone_fixed": null,
            "phone_fixed_second": null,
            "phone_mobile": null,
            "fax": null,
            "url": null,
            "skype_name": null,
            "remarks": null,
            "language_id": null,
            "is_lead": false,
            "contact_group_ids": null,
            "contact_branch_ids": null,
            "user_id": 1,
            "owner_id": 1,
            "updated_at": "2024-01-01 12:00:00"
        }
        """;

    /// <summary>
    /// <c>ContactService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/contact</c>
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task ContactService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(ContactsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactsPath));
        });
    }

    /// <summary>
    /// <c>ContactService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and surface the returned contact on success.
    /// </summary>
    [Test]
    public async Task ContactService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{ContactsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactResponse));

        var service = new ContactService(ConnectionHandler);

        var result = await service.GetById(id, cancellationToken: TestContext.CurrentContext.CancellationToken);

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
    /// <c>ContactService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="ContactCreate"/> payload, and must surface the returned contact on success.
    /// </summary>
    [Test]
    public async Task ContactService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(ContactsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ContactResponse));

        var service = new ContactService(ConnectionHandler);

        var payload = new ContactCreate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 1,
            OwnerId: 1);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactsPath));
            Assert.That(request.Body, Does.Contain("\"name_1\":\"Acme AG\""));
            Assert.That(request.Body, Does.Contain("\"contact_type_id\":1"));
        });
    }

    /// <summary>
    /// <c>ContactService.BulkCreate</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact/_bulk_create</c> with a JSON array body.
    /// </summary>
    [Test]
    public async Task ContactService_BulkCreate_SendsPostRequest_ToBulkCreatePath()
    {
        var expectedPath = $"{ContactsPath}/_bulk_create";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody($"[{ContactResponse}]"));

        var service = new ContactService(ConnectionHandler);

        var payload = new List<ContactCreate>
        {
            new(ContactTypeId: 1, Name1: "Acme AG", UserId: 1, OwnerId: 1)
        };

        var result = await service.BulkCreate(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.StartWith("["));
        });
    }

    /// <summary>
    /// <c>ContactService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact/search</c> with the <see cref="SearchCriteria"/> list as the JSON body.
    /// </summary>
    [Test]
    public async Task ContactService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{ContactsPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactResponse}]"));

        var service = new ContactService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name_1", Value = "Acme", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name_1\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    /// <c>ContactService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/contact/{id}</c> — Bexio uses POST for full-replacement edits on this resource.
    /// </summary>
    [Test]
    public async Task ContactService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{ContactsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactResponse));

        var service = new ContactService(ConnectionHandler);

        var payload = new ContactUpdate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 1,
            OwnerId: 1);

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
    /// <c>ContactService.Restore</c> must send a <c>PATCH</c> request against
    /// <c>/2.0/contact/{id}/restore</c> with no body — Bexio uses PATCH for this action.
    /// </summary>
    [Test]
    public async Task ContactService_Restore_SendsPatchRequest()
    {
        const int id = 1;
        var expectedPath = $"{ContactsPath}/{id}/restore";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ContactService(ConnectionHandler);

        var result = await service.Restore(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>ContactService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path.
    /// </summary>
    [Test]
    public async Task ContactService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{ContactsPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new ContactService(ConnectionHandler);

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
