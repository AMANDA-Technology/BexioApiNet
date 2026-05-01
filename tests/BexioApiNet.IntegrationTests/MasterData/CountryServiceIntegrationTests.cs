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
using BexioApiNet.Abstractions.Models.MasterData.Countries.Views;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="CountryService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="CountryConfiguration" />
/// (<c>2.0/country</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (including the Bexio-specific <c>POST</c> for edits — operationId <c>v2EditCountry</c>),
/// that payloads are serialized with the expected snake_case field names, and that the response
/// JSON deserialises into the <see cref="BexioApiNet.Abstractions.Models.MasterData.Countries.Country"/>
/// record without loss.
/// </summary>
public sealed class CountryServiceIntegrationTests : IntegrationTestBase
{
    private const string CountryPath = "/2.0/country";

    private const string CountryResponse = """
                                           {
                                               "id": 1,
                                               "name": "Kiribati",
                                               "name_short": "KI",
                                               "iso3166_alpha2": "KI"
                                           }
                                           """;

    /// <summary>
    /// <c>CountryService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/country</c> and return a successful <c>ApiResult</c> when the server
    /// returns a fully populated array, deserialising every field on the country payload.
    /// </summary>
    [Test]
    public async Task CountryService_Get_SendsGetRequest_AndDeserialisesEveryField()
    {
        Server
            .Given(Request.Create().WithPath(CountryPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CountryResponse}]"));

        var service = new CountryService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CountryPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data[0].Name, Is.EqualTo("Kiribati"));
            Assert.That(result.Data[0].NameShort, Is.EqualTo("KI"));
            Assert.That(result.Data[0].Iso3166Alpha2, Is.EqualTo("KI"));
        });
    }

    /// <summary>
    /// <c>CountryService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// id in the URL path and surface the returned country on success with all fields populated.
    /// </summary>
    [Test]
    public async Task CountryService_GetById_SendsGetRequest_AndDeserialisesEveryField()
    {
        const int id = 1;
        var expectedPath = $"{CountryPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CountryResponse));

        var service = new CountryService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Name, Is.EqualTo("Kiribati"));
            Assert.That(result.Data.NameShort, Is.EqualTo("KI"));
            Assert.That(result.Data.Iso3166Alpha2, Is.EqualTo("KI"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>CountryService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="CountryCreate" /> payload, and must surface the returned country
    /// on success.
    /// </summary>
    [Test]
    public async Task CountryService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(CountryPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CountryResponse));

        var service = new CountryService(ConnectionHandler);

        var payload = new CountryCreate("Kiribati", "KI", "KI");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CountryPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Kiribati\""));
            Assert.That(request.Body, Does.Contain("\"name_short\":\"KI\""));
            Assert.That(request.Body, Does.Contain("\"iso3166_alpha2\":\"KI\""));
        });
    }

    /// <summary>
    /// <c>CountryService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/country/search</c> with the <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task CountryService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{CountryPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CountryResponse}]"));

        var service = new CountryService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Kiribati", Criteria = "like" }
        };

        var result = await service.Search(criteria, cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Iso3166Alpha2, Is.EqualTo("KI"));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"field\":\"name\""));
            Assert.That(request.Body, Does.Contain("\"criteria\":\"like\""));
        });
    }

    /// <summary>
    /// <c>CountryService.Update</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// <c>/2.0/country/{id}</c>. Bexio's v2 country edit endpoint is documented as
    /// <c>POST /2.0/country/{country_id}</c> (operationId <c>v2EditCountry</c>) and the body
    /// is serialised with the same snake_case names as the create payload.
    /// </summary>
    [Test]
    public async Task CountryService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{CountryPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CountryResponse));

        var service = new CountryService(ConnectionHandler);

        var payload = new CountryUpdate("Kiribati", "KIR", "KI");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Kiribati\""));
            Assert.That(request.Body, Does.Contain("\"name_short\":\"KIR\""));
            Assert.That(request.Body, Does.Contain("\"iso3166_alpha2\":\"KI\""));
        });
    }

    /// <summary>
    /// <c>CountryService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path.
    /// </summary>
    [Test]
    public async Task CountryService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{CountryPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new CountryService(ConnectionHandler);

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
