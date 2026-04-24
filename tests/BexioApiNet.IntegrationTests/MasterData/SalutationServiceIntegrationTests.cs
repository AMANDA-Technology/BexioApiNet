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
using BexioApiNet.Abstractions.Models.MasterData.Salutations.Views;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests covering the CRUD entry points of <see cref="SalutationService" /> against
///     WireMock stubs. Verifies the path composed from <see cref="SalutationConfiguration" />
///     (<c>2.0/salutation</c>) reaches the handler correctly, that the expected HTTP verbs are used
///     (including the Bexio-specific <c>PUT</c> for edits), and that payloads are serialized with the
///     expected snake_case field names.
/// </summary>
public sealed class SalutationServiceIntegrationTests : IntegrationTestBase
{
    private const string SalutationPath = "/2.0/salutation";

    private const string SalutationResponse = """
                                              {
                                                  "id": 1,
                                                  "name": "Herr"
                                              }
                                              """;

    /// <summary>
    ///     <c>SalutationService.Get()</c> must issue a <c>GET</c> request against
    ///     <c>/2.0/salutation</c> and return a successful <c>ApiResult</c> when the server
    ///     returns an empty array.
    /// </summary>
    [Test]
    public async Task SalutationService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(SalutationPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new SalutationService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SalutationPath));
        });
    }

    /// <summary>
    ///     <c>SalutationService.GetById</c> must issue a <c>GET</c> request that includes the target
    ///     id in the URL path and surface the returned salutation on success.
    /// </summary>
    [Test]
    public async Task SalutationService_GetById_SendsGetRequest()
    {
        const int id = 1;
        var expectedPath = $"{SalutationPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(SalutationResponse));

        var service = new SalutationService(ConnectionHandler);

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
    ///     <c>SalutationService.Create</c> must send a <c>POST</c> request whose body is the
    ///     serialized <see cref="SalutationCreate" /> payload, and must surface the returned salutation
    ///     on success.
    /// </summary>
    [Test]
    public async Task SalutationService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SalutationPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(SalutationResponse));

        var service = new SalutationService(ConnectionHandler);

        var payload = new SalutationCreate("Herr");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SalutationPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"Herr\""));
        });
    }

    /// <summary>
    ///     <c>SalutationService.Search</c> must send a <c>POST</c> request against
    ///     <c>/2.0/salutation/search</c> with the <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task SalutationService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{SalutationPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{SalutationResponse}]"));

        var service = new SalutationService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Herr", Criteria = "like" }
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
        });
    }

    /// <summary>
    ///     <c>SalutationService.Update</c> must send a <c>PUT</c> request against
    ///     <c>/2.0/salutation/{id}</c>.
    /// </summary>
    [Test]
    public async Task SalutationService_Update_SendsPutRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{SalutationPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(SalutationResponse));

        var service = new SalutationService(ConnectionHandler);

        var payload = new SalutationUpdate("Frau");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>SalutationService.Delete</c> must issue a <c>DELETE</c> request that includes the
    ///     target id in the URL path.
    /// </summary>
    [Test]
    public async Task SalutationService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{SalutationPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new SalutationService(ConnectionHandler);

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