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
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="ContactService"/> against
/// WireMock stubs. Verifies the path composed from <see cref="ContactConfiguration"/>
/// (<c>2.0/contact</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits and <c>PATCH</c> for restore), and that
/// payloads are serialized with the expected snake_case field names. Each list / read response
/// stub is fully populated so the tests assert deserialization of every property in the
/// <c>Contact</c> / <c>ContactWithDetails</c> schemas defined by the OpenAPI spec.
/// </summary>
public sealed class ContactServiceIntegrationTests : IntegrationTestBase
{
    private const string ContactsPath = "/2.0/contact";

    /// <summary>
    /// Fully-populated <c>ContactWithDetails</c> response body — covers every property in the
    /// Bexio v3 OpenAPI spec for <c>ContactWithDetails</c>: the 32 base <c>Contact</c> fields
    /// (including the read-only <c>title_id</c>, write-only <c>titel_id</c>, the deprecated
    /// <c>is_lead</c>, and the response-only composed <c>address</c>), plus <c>profile_image</c>.
    /// </summary>
    private const string ContactResponse = """
        {
            "id": 4,
            "nr": "1000",
            "contact_type_id": 1,
            "name_1": "Acme AG",
            "name_2": "Branch Zurich",
            "salutation_id": 2,
            "salutation_form": 3,
            "title_id": 7,
            "birthday": "1980-05-15",
            "address": "Smith Street 22",
            "street_name": "Smith Street",
            "house_number": "22",
            "address_addition": "Building C",
            "postcode": "8001",
            "city": "Zurich",
            "country_id": 1,
            "mail": "info@acme.example",
            "mail_second": "billing@acme.example",
            "phone_fixed": "+41 44 000 00 00",
            "phone_fixed_second": "+41 44 000 00 01",
            "phone_mobile": "+41 79 000 00 00",
            "fax": "+41 44 000 00 02",
            "url": "https://acme.example",
            "skype_name": "acme.skype",
            "remarks": "VIP customer",
            "language_id": 1,
            "is_lead": false,
            "contact_group_ids": "1,2",
            "contact_branch_ids": "3,4",
            "user_id": 11,
            "owner_id": 12,
            "updated_at": "2024-01-01 12:00:00",
            "profile_image": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAAB"
        }
        """;

    /// <summary>
    /// <c>ContactService.Get()</c> must issue a <c>GET</c> request against <c>/2.0/contact</c>
    /// and deserialize a fully-populated array response into a <see cref="System.Collections.Generic.List{T}"/>
    /// of <c>Contact</c> with every property mapped from snake_case JSON to PascalCase C#.
    /// </summary>
    [Test]
    public async Task ContactService_Get_SendsGetRequest_AndDeserializesContact()
    {
        Server
            .Given(Request.Create().WithPath(ContactsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactResponse}]"));

        var service = new ContactService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactsPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertContactFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>ContactService.Get()</c> serializes <c>limit</c>, <c>offset</c>, <c>order_by</c>, and
    /// <c>show_archived</c> onto the request URL when a populated <see cref="QueryParameterContact"/>
    /// is supplied. Verifies the query parameter names match the Bexio v3 spec.
    /// </summary>
    [Test]
    public async Task ContactService_Get_WithQueryParameters_SerializesQueryString()
    {
        Server
            .Given(Request.Create().WithPath(ContactsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ContactService(ConnectionHandler);

        var queryParameter = new QueryParameterContact(
            Limit: 25,
            Offset: 10,
            OrderBy: "name_1",
            ShowArchived: true);

        await service.Get(queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.RawQuery, Does.Contain("limit=25"));
        Assert.That(request.RawQuery, Does.Contain("offset=10"));
        Assert.That(request.RawQuery, Does.Contain("order_by=name_1"));
        Assert.That(request.RawQuery, Does.Contain("show_archived=true"));
    }

    /// <summary>
    /// <c>ContactService.GetById</c> must issue a <c>GET</c> request that includes the target id
    /// in the URL path and deserialize the full <c>ContactWithDetails</c> body returned by Bexio.
    /// </summary>
    [Test]
    public async Task ContactService_GetById_SendsGetRequest_AndDeserializesContactWithDetails()
    {
        const int id = 4;
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
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertContactFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactService.Create</c> must send a <c>POST</c> request whose body is the serialized
    /// <see cref="ContactCreate"/> payload. Verifies that snake_case field names appear on the
    /// wire and that the response is fully deserialized.
    /// </summary>
    [Test]
    public async Task ContactService_Create_SendsPostRequest_AndDeserializesContactWithDetails()
    {
        Server
            .Given(Request.Create().WithPath(ContactsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(ContactResponse));

        var service = new ContactService(ConnectionHandler);

        var payload = new ContactCreate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 11,
            OwnerId: 12,
            Nr: "1000",
            Name2: "Branch Zurich",
            TitelId: 7,
            Mail: "info@acme.example",
            City: "Zurich");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ContactsPath));
            Assert.That(request.Body, Does.Contain("\"contact_type_id\":1"));
            Assert.That(request.Body, Does.Contain("\"name_1\":\"Acme AG\""));
            Assert.That(request.Body, Does.Contain("\"user_id\":11"));
            Assert.That(request.Body, Does.Contain("\"owner_id\":12"));
            Assert.That(request.Body, Does.Contain("\"nr\":\"1000\""));
            Assert.That(request.Body, Does.Contain("\"name_2\":\"Branch Zurich\""));
            Assert.That(request.Body, Does.Contain("\"titel_id\":7"));
            Assert.That(request.Body, Does.Contain("\"mail\":\"info@acme.example\""));
            Assert.That(request.Body, Does.Contain("\"city\":\"Zurich\""));
        });

        AssertContactFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactService.BulkCreate</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact/_bulk_create</c> with a JSON array body and deserialize the array
    /// response into a list of fully-populated <c>Contact</c> records.
    /// </summary>
    [Test]
    public async Task ContactService_BulkCreate_SendsPostRequest_ToBulkCreatePath_AndDeserializesArray()
    {
        var expectedPath = $"{ContactsPath}/_bulk_create";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{ContactResponse}]"));

        var service = new ContactService(ConnectionHandler);

        var payload = new List<ContactCreate>
        {
            new(ContactTypeId: 1, Name1: "Acme AG", UserId: 11, OwnerId: 12)
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

        AssertContactFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>ContactService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact/search</c> with the <see cref="SearchCriteria"/> list as the JSON body
    /// and deserialize the array response into fully-populated <c>Contact</c> records.
    /// </summary>
    [Test]
    public async Task ContactService_Search_SendsPostRequest_ToSearchPath_AndDeserializesArray()
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
            Assert.That(request.Body, Does.Contain("\"value\":\"Acme\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertContactFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>ContactService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/contact/{id}</c> — Bexio uses POST for full-replacement edits on this resource —
    /// and deserialize the response into a fully-populated <c>Contact</c>.
    /// </summary>
    [Test]
    public async Task ContactService_Update_SendsPostRequest_WithIdInPath_AndDeserializesContactWithDetails()
    {
        const int id = 4;
        var expectedPath = $"{ContactsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ContactResponse));

        var service = new ContactService(ConnectionHandler);

        var payload = new ContactUpdate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 11,
            OwnerId: 12);

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertContactFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>ContactService.Restore</c> must send a <c>PATCH</c> request against
    /// <c>/2.0/contact/{id}/restore</c> with no body — Bexio uses PATCH for this action and
    /// returns a <c>{ "success": true }</c> envelope (schema title <c>SuccessResponse</c>).
    /// </summary>
    [Test]
    public async Task ContactService_Restore_SendsPatchRequest()
    {
        const int id = 4;
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
            Assert.That(request.Body, Is.Null.Or.Empty);
        });
    }

    /// <summary>
    /// <c>ContactService.Delete</c> must issue a <c>DELETE</c> request that includes the target id
    /// in the URL path and surface Bexio's <c>EntryDeleted</c> envelope as a successful result.
    /// </summary>
    [Test]
    public async Task ContactService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 4;
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

    /// <summary>
    /// Verifies that every property in a <c>ContactWithDetails</c>-shaped JSON response is mapped
    /// onto the corresponding C# property.
    /// </summary>
    private static void AssertContactFullyDeserialized(BexioApiNet.Abstractions.Models.Contacts.Contacts.Contact contact)
    {
        Assert.Multiple(() =>
        {
            Assert.That(contact.Id, Is.EqualTo(4));
            Assert.That(contact.Nr, Is.EqualTo("1000"));
            Assert.That(contact.ContactTypeId, Is.EqualTo(1));
            Assert.That(contact.Name1, Is.EqualTo("Acme AG"));
            Assert.That(contact.Name2, Is.EqualTo("Branch Zurich"));
            Assert.That(contact.SalutationId, Is.EqualTo(2));
            Assert.That(contact.SalutationForm, Is.EqualTo(3));
            Assert.That(contact.TitleId, Is.EqualTo(7));
            Assert.That(contact.Birthday, Is.EqualTo(new DateOnly(1980, 5, 15)));
            Assert.That(contact.Address, Is.EqualTo("Smith Street 22"));
            Assert.That(contact.StreetName, Is.EqualTo("Smith Street"));
            Assert.That(contact.HouseNumber, Is.EqualTo("22"));
            Assert.That(contact.AddressAddition, Is.EqualTo("Building C"));
            Assert.That(contact.Postcode, Is.EqualTo("8001"));
            Assert.That(contact.City, Is.EqualTo("Zurich"));
            Assert.That(contact.CountryId, Is.EqualTo(1));
            Assert.That(contact.Mail, Is.EqualTo("info@acme.example"));
            Assert.That(contact.MailSecond, Is.EqualTo("billing@acme.example"));
            Assert.That(contact.PhoneFixed, Is.EqualTo("+41 44 000 00 00"));
            Assert.That(contact.PhoneFixedSecond, Is.EqualTo("+41 44 000 00 01"));
            Assert.That(contact.PhoneMobile, Is.EqualTo("+41 79 000 00 00"));
            Assert.That(contact.Fax, Is.EqualTo("+41 44 000 00 02"));
            Assert.That(contact.Url, Is.EqualTo("https://acme.example"));
            Assert.That(contact.SkypeName, Is.EqualTo("acme.skype"));
            Assert.That(contact.Remarks, Is.EqualTo("VIP customer"));
            Assert.That(contact.LanguageId, Is.EqualTo(1));
            Assert.That(contact.IsLead, Is.False);
            Assert.That(contact.ContactGroupIds, Is.EqualTo("1,2"));
            Assert.That(contact.ContactBranchIds, Is.EqualTo("3,4"));
            Assert.That(contact.UserId, Is.EqualTo(11));
            Assert.That(contact.OwnerId, Is.EqualTo(12));
            Assert.That(contact.UpdatedAt, Is.EqualTo("2024-01-01 12:00:00"));
            Assert.That(contact.ProfileImage, Is.EqualTo("iVBORw0KGgoAAAANSUhEUgAAAAEAAAAB"));
        });
    }
}
