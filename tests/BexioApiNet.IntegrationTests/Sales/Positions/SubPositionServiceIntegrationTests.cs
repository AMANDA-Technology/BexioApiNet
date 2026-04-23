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

using BexioApiNet.Abstractions.Enums.Sales;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.IntegrationTests.Sales.Positions;

/// <summary>
/// Offline integration tests for <see cref="SubPositionService"/> against a
/// <see cref="WireMockServer"/> stub. Verifies that the correct HTTP verbs and URL paths are
/// used for all five CRUD operations and that request bodies are serialized with the expected
/// snake_case field names.
/// </summary>
public sealed class SubPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = KbDocumentType.Invoice;
    private const int DocumentId = 4;
    private const int PositionId = 7;

    private static string BasePath => $"/2.0/{DocumentType}/{DocumentId}/kb_position_subposition";

    private const string SubpositionResponse = """
        {
            "id": 7,
            "type": "KbPositionSubposition",
            "parent_id": null,
            "text": "Group heading",
            "pos": "1",
            "internal_pos": 1,
            "show_pos_nr": true,
            "is_optional": false,
            "total_sum": "0.00",
            "show_pos_prices": false
        }
        """;

    /// <summary>
    /// <c>SubPositionService.Get()</c> must issue a <c>GET</c> request against the expected path
    /// and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task SubPositionService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new SubPositionService(ConnectionHandler);

        var result = await service.Get(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
        });
    }

    /// <summary>
    /// <c>SubPositionService.GetById()</c> must issue a <c>GET</c> request that includes the
    /// position id in the URL path and surface the returned position on success.
    /// </summary>
    [Test]
    public async Task SubPositionService_GetById_SendsGetRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(SubpositionResponse));

        var service = new SubPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>SubPositionService.Create()</c> must send a <c>POST</c> request whose body contains the
    /// serialized <see cref="PositionSubpositionCreate"/> payload in snake_case and surface the
    /// created position on success.
    /// </summary>
    [Test]
    public async Task SubPositionService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(SubpositionResponse));

        var service = new SubPositionService(ConnectionHandler);

        var payload = new PositionSubpositionCreate(Text: "Group heading", ShowPosNr: true);

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Group heading\""));
            Assert.That(request.Body, Does.Contain("\"show_pos_nr\":true"));
        });
    }

    /// <summary>
    /// <c>SubPositionService.Update()</c> must send a <c>POST</c> (not <c>PUT</c>) request against
    /// the path including the position id and surface the updated position on success.
    /// </summary>
    [Test]
    public async Task SubPositionService_Update_SendsPostRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(SubpositionResponse));

        var service = new SubPositionService(ConnectionHandler);

        var payload = new PositionSubpositionUpdate(Text: "Updated heading");

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Updated heading\""));
        });
    }

    /// <summary>
    /// <c>SubPositionService.Delete()</c> must issue a <c>DELETE</c> request that includes the
    /// position id in the URL path.
    /// </summary>
    [Test]
    public async Task SubPositionService_Delete_SendsDeleteRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new SubPositionService(ConnectionHandler);

        var result = await service.Delete(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
