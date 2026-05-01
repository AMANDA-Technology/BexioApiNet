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
using BexioApiNet.Abstractions.Models.Items.Units.Views;
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.IntegrationTests.Items;

/// <summary>
/// Integration tests covering the CRUD entry points of <see cref="UnitService" /> against
/// WireMock stubs. Verifies the path composed from <see cref="UnitConfiguration" />
/// (<c>2.0/unit</c>) reaches the handler correctly, that the expected HTTP verbs are used
/// (Bexio's <c>v2EditUnit</c> is exposed as <c>POST /2.0/unit/{unit_id}</c>), and that
/// payloads are serialized with the expected snake_case field names.
/// </summary>
public sealed class UnitServiceIntegrationTests : IntegrationTestBase
{
    private const string UnitPath = "/2.0/unit";

    private const string UnitResponse = """
                                        {
                                            "id": 1,
                                            "name": "kg"
                                        }
                                        """;

    /// <summary>
    /// <c>UnitService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/unit</c> and deserialize each returned <see cref="Abstractions.Models.Items.Units.Unit"/>
    /// from the OpenAPI-shaped JSON array returned by Bexio.
    /// </summary>
    [Test]
    public async Task UnitService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(UnitPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{UnitResponse}]"));

        var service = new UnitService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(UnitPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("kg"));
        });
    }

    /// <summary>
    /// <c>UnitService.GetById</c> must issue a <c>GET</c> request that includes the target
    /// id in the URL path and deserialize every field defined in the OpenAPI <c>Unit</c> schema.
    /// </summary>
    [Test]
    public async Task UnitService_GetById_SendsGetRequest_AndDeserializesAllFields()
    {
        const int id = 1;
        var expectedPath = $"{UnitPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(UnitResponse));

        var service = new UnitService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data!.Name, Is.EqualTo("kg"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>UnitService.Create</c> must send a <c>POST</c> request whose body is the
    /// serialized <see cref="UnitCreate" /> payload, and must surface the returned unit
    /// on success.
    /// </summary>
    [Test]
    public async Task UnitService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(UnitPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(UnitResponse));

        var service = new UnitService(ConnectionHandler);

        var payload = new UnitCreate("kg");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(UnitPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"kg\""));
        });
    }

    /// <summary>
    /// <c>UnitService.Search</c> must send a <c>POST</c> request against
    /// <c>/2.0/unit/search</c> with the <see cref="SearchCriteria" /> list as the JSON body.
    /// </summary>
    [Test]
    public async Task UnitService_Search_SendsPostRequest_ToSearchPath()
    {
        var expectedPath = $"{UnitPath}/search";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{UnitResponse}]"));

        var service = new UnitService(ConnectionHandler);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "kg", Criteria = "like" }
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
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(result.Data![0].Id, Is.EqualTo(1));
            Assert.That(result.Data![0].Name, Is.EqualTo("kg"));
        });
    }

    /// <summary>
    /// <c>UnitService.Update</c> must send a <c>POST</c> request against
    /// <c>/2.0/unit/{id}</c>. Bexio's <c>v2EditUnit</c> operation is exposed as POST on this resource
    /// in <c>doc/openapi/bexio-v3.json</c>, in line with the v2 convention of using POST for edits.
    /// </summary>
    [Test]
    public async Task UnitService_Update_SendsPostRequest_WithIdInPath()
    {
        const int id = 1;
        var expectedPath = $"{UnitPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(UnitResponse));

        var service = new UnitService(ConnectionHandler);

        var payload = new UnitUpdate("kilogram");

        var result = await service.Update(id, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"name\":\"kilogram\""));
        });
    }

    /// <summary>
    /// <c>UnitService.Delete</c> must issue a <c>DELETE</c> request that includes the
    /// target id in the URL path.
    /// </summary>
    [Test]
    public async Task UnitService_Delete_SendsDeleteRequest()
    {
        const int idToDelete = 1;
        var expectedPath = $"{UnitPath}/{idToDelete}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new UnitService(ConnectionHandler);

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
