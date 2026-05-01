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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses;
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Contacts;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="AdditionalAddressService" /> against
/// WireMock stubs. Additional addresses are nested under a parent contact, so all routes follow the
/// pattern <c>2.0/contact/{contactId}/additional_address</c> (see
/// <see cref="AdditionalAddressConfiguration" />). Verifies URL construction with the parent contact
/// id, that the expected HTTP verbs are used (including the Bexio-specific <c>POST</c> for edits),
/// and that payloads are serialized with the expected snake_case field names. Each list / read
/// response stub is fully populated so the tests assert deserialization of every property in the
/// OpenAPI <c>ContactRelation</c>-titled additional-address schema.
/// </summary>
public sealed class AdditionalAddressServiceIntegrationTests : IntegrationTestBase
{
    private const int TestContactId = 42;
    private const string AdditionalAddressPath = "/2.0/contact/42/additional_address";

    /// <summary>
    /// Fully-populated additional address response body — covers every property exposed by the
    /// (mistitled <c>ContactRelation</c>) additional-address schema in the Bexio v3 OpenAPI spec.
    /// 12 properties: <c>id</c>, <c>name</c>, <c>name_addition</c>, <c>address</c>, <c>street_name</c>,
    /// <c>house_number</c>, <c>address_addition</c>, <c>postcode</c>, <c>city</c>, <c>country_id</c>,
    /// <c>subject</c>, <c>description</c>.
    /// </summary>
    private const string AdditionalAddressResponse = """
                                                     {
                                                         "id": 5,
                                                         "name": "Warehouse",
                                                         "name_addition": "North wing",
                                                         "address": "Walter Street 22",
                                                         "street_name": "Walter Street",
                                                         "house_number": "22",
                                                         "address_addition": "Building C",
                                                         "postcode": "8000",
                                                         "city": "Zurich",
                                                         "country_id": 1,
                                                         "subject": "Delivery address",
                                                         "description": "Backup site"
                                                     }
                                                     """;

    /// <summary>
    /// <c>AdditionalAddressService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/contact/{contactId}/additional_address</c> and deserialize the array body into a
    /// list of fully-populated <c>AdditionalAddress</c> records.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Get_SendsGetRequest_AndDeserializesAdditionalAddress()
    {
        Server
            .Given(Request.Create().WithPath(AdditionalAddressPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{AdditionalAddressResponse}]"));

        var service = new AdditionalAddressService(ConnectionHandler);

        var result = await service.Get(TestContactId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AdditionalAddressPath));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertAdditionalAddressFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>AdditionalAddressService.Get()</c> serializes <c>limit</c>, <c>offset</c> and
    /// <c>order_by</c> onto the request URL when a populated <see cref="QueryParameterAdditionalAddress"/>
    /// is supplied. Verifies the query parameter names match the Bexio v3 spec.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Get_WithQueryParameters_SerializesQueryString()
    {
        Server
            .Given(Request.Create().WithPath(AdditionalAddressPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AdditionalAddressService(ConnectionHandler);

        var queryParameter = new QueryParameterAdditionalAddress(Limit: 25, Offset: 50, OrderBy: "name");

        await service.Get(TestContactId, queryParameter, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.That(request.RawQuery, Does.Contain("limit=25"));
        Assert.That(request.RawQuery, Does.Contain("offset=50"));
        Assert.That(request.RawQuery, Does.Contain("order_by=name"));
    }

    /// <summary>
    /// <c>AdditionalAddressService.GetById</c> must issue a <c>GET</c> request that includes both
    /// the parent contact id and the address id in the URL path and deserialize the returned body.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_GetById_SendsGetRequest_AndDeserializesAdditionalAddress()
    {
        const int id = 5;
        var expectedPath = $"{AdditionalAddressPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AdditionalAddressResponse));

        var service = new AdditionalAddressService(ConnectionHandler);

        var result = await service.GetById(TestContactId, id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertAdditionalAddressFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>AdditionalAddressService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="AdditionalAddressCreate" /> payload, and must surface the returned
    /// additional address on success with all properties populated.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Create_SendsPostRequest_AndDeserializesAdditionalAddress()
    {
        Server
            .Given(Request.Create().WithPath(AdditionalAddressPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(AdditionalAddressResponse));

        var service = new AdditionalAddressService(ConnectionHandler);

        var payload = new AdditionalAddressCreate(
            "Warehouse",
            "North wing",
            "Walter Street",
            "22",
            "Building C",
            "8000",
            "Zurich",
            1,
            "Delivery address",
            "Backup site");

        var result = await service.Create(TestContactId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AdditionalAddressPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Warehouse\""));
            Assert.That(request.Body, Does.Contain("\"name_addition\":\"North wing\""));
            Assert.That(request.Body, Does.Contain("\"street_name\":\"Walter Street\""));
            Assert.That(request.Body, Does.Contain("\"house_number\":\"22\""));
            Assert.That(request.Body, Does.Contain("\"address_addition\":\"Building C\""));
            Assert.That(request.Body, Does.Contain("\"postcode\":\"8000\""));
            Assert.That(request.Body, Does.Contain("\"city\":\"Zurich\""));
            Assert.That(request.Body, Does.Contain("\"country_id\":1"));
            Assert.That(request.Body, Does.Contain("\"subject\":\"Delivery address\""));
            Assert.That(request.Body, Does.Contain("\"description\":\"Backup site\""));
        });

        AssertAdditionalAddressFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>AdditionalAddressService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/contact/{contactId}/additional_address/search</c> with the <see cref="SearchCriteria" />
    /// list as the JSON body and deserialize the array response with full property coverage on every item.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Search_SendsPostRequest_ToSearchPath_AndDeserializesArray()
    {
        var expectedPath = $"{AdditionalAddressPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{AdditionalAddressResponse}]"));

        var service = new AdditionalAddressService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Warehouse", Criteria = "=" }
        };

        var result = await service.Search(TestContactId, criteria,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"=\""));
            Assert.That(request.Body, Does.Contain("\"value\":\"Warehouse\""));
            Assert.That(result.Data, Has.Count.EqualTo(1));
        });

        AssertAdditionalAddressFullyDeserialized(result.Data![0]);
    }

    /// <summary>
    /// <c>AdditionalAddressService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request
    /// against <c>/2.0/contact/{contactId}/additional_address/{id}</c> — Bexio uses POST for
    /// full-replacement edits on v2.0 resources.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Update_SendsPostRequest_WithIdInPath_AndDeserializesAdditionalAddress()
    {
        const int id = 5;
        var expectedPath = $"{AdditionalAddressPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AdditionalAddressResponse));

        var service = new AdditionalAddressService(ConnectionHandler);

        var payload = new AdditionalAddressUpdate(
            "Warehouse",
            "North wing",
            "Walter Street",
            "22",
            "Building C",
            "8000",
            "Zurich",
            1,
            "Delivery address",
            "Backup site");

        var result = await service.Update(TestContactId, id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        AssertAdditionalAddressFullyDeserialized(result.Data!);
    }

    /// <summary>
    /// <c>AdditionalAddressService.Delete</c> must issue a <c>DELETE</c> request that includes
    /// both the parent contact id and the address id in the URL path.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 5;
        var expectedPath = $"{AdditionalAddressPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new AdditionalAddressService(ConnectionHandler);

        var result = await service.Delete(TestContactId, idToDelete, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// Verifies that every property in an <c>AdditionalAddress</c>-shaped JSON response is mapped
    /// onto the corresponding C# property.
    /// </summary>
    private static void AssertAdditionalAddressFullyDeserialized(AdditionalAddress address)
    {
        Assert.Multiple(() =>
        {
            Assert.That(address.Id, Is.EqualTo(5));
            Assert.That(address.Name, Is.EqualTo("Warehouse"));
            Assert.That(address.NameAddition, Is.EqualTo("North wing"));
            Assert.That(address.Address, Is.EqualTo("Walter Street 22"));
            Assert.That(address.StreetName, Is.EqualTo("Walter Street"));
            Assert.That(address.HouseNumber, Is.EqualTo("22"));
            Assert.That(address.AddressAddition, Is.EqualTo("Building C"));
            Assert.That(address.Postcode, Is.EqualTo("8000"));
            Assert.That(address.City, Is.EqualTo("Zurich"));
            Assert.That(address.CountryId, Is.EqualTo(1));
            Assert.That(address.Subject, Is.EqualTo("Delivery address"));
            Assert.That(address.Description, Is.EqualTo("Backup site"));
        });
    }
}
