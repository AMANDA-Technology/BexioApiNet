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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.IntegrationTests.Smoke.Contacts;

/// <summary>
///     Smoke tests covering the CRUD entry points of <see cref="AdditionalAddressService" /> against
///     WireMock stubs. Additional addresses are nested under a parent contact, so all routes follow the
///     pattern <c>2.0/contact/{contactId}/additional_address</c> (see
///     <see cref="AdditionalAddressConfiguration" />). Verifies URL construction with the parent contact
///     id, that the expected HTTP verbs are used (including the Bexio-specific <c>POST</c> for edits),
///     and that payloads are serialized with the expected snake_case field names.
/// </summary>
public sealed class AdditionalAddressSmokeTests : IntegrationTestBase
{
    private const int TestContactId = 42;
    private const string AdditionalAddressPath = "/2.0/contact/42/additional_address";

    private const string AdditionalAddressResponse = """
                                                     {
                                                         "id": 1,
                                                         "name": "Warehouse",
                                                         "name_addition": null,
                                                         "address": "Walter Street 22",
                                                         "street_name": "Walter Street",
                                                         "house_number": "22",
                                                         "address_addition": null,
                                                         "postcode": "8000",
                                                         "city": "Zurich",
                                                         "country_id": 1,
                                                         "subject": "Delivery address",
                                                         "description": null
                                                     }
                                                     """;

    /// <summary>
    ///     <c>AdditionalAddressService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/2.0/contact/{contactId}/additional_address</c> and return a successful <c>ApiResult</c>
    ///     when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(AdditionalAddressPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AdditionalAddressService(ConnectionHandler);

        var result = await service.Get(TestContactId, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AdditionalAddressPath));
        });
    }

    /// <summary>
    ///     <c>AdditionalAddressService.GetById</c> must issue a <c>GET</c> request that includes both
    ///     the parent contact id and the address id in the URL path.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_GetById_SendsGetRequest()
    {
        const int id = 1;
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
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>AdditionalAddressService.Create</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="AdditionalAddressCreate" /> payload, and must surface the returned
    ///     additional address on success.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(AdditionalAddressPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(AdditionalAddressResponse));

        var service = new AdditionalAddressService(ConnectionHandler);

        var payload = new AdditionalAddressCreate(
            "Warehouse",
            null,
            "Walter Street",
            "22",
            null,
            "8000",
            "Zurich",
            1,
            "Delivery address",
            null);

        var result = await service.Create(TestContactId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AdditionalAddressPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Warehouse\""));
            Assert.That(request.Body, Does.Contain("\"street_name\":\"Walter Street\""));
        });
    }

    /// <summary>
    ///     <c>AdditionalAddressService.Search</c> must send a <c>POST</c> request against
    ///     <c>/2.0/contact/{contactId}/additional_address/search</c> with the <see cref="SearchCriteria" />
    ///     list as the JSON body.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Search_SendsPostRequest_ToSearchPath()
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
        });
    }

    /// <summary>
    ///     <c>AdditionalAddressService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request
    ///     against <c>/2.0/contact/{contactId}/additional_address/{id}</c> — Bexio uses POST for
    ///     full-replacement edits on v2.0 resources.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{AdditionalAddressPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AdditionalAddressResponse));

        var service = new AdditionalAddressService(ConnectionHandler);

        var payload = new AdditionalAddressUpdate(
            "Warehouse",
            null,
            "Walter Street",
            "22",
            null,
            "8000",
            "Zurich",
            1,
            "Delivery address",
            null);

        var result = await service.Update(TestContactId, id, payload, TestContext.CurrentContext.CancellationToken);

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
    ///     <c>AdditionalAddressService.Delete</c> must issue a <c>DELETE</c> request that includes
    ///     both the parent contact id and the address id in the URL path.
    /// </summary>
    [Test]
    public async Task AdditionalAddressService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
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
}