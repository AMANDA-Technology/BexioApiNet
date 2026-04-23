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
/// Offline integration tests for <see cref="PagebreakPositionService"/> against a
/// <see cref="WireMockServer"/> stub. Verifies that the correct HTTP verbs and URL paths are
/// used for all five CRUD operations and that request bodies are serialized with the expected
/// snake_case field names.
/// </summary>
public sealed class PagebreakPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = KbDocumentType.Order;
    private const int DocumentId = 5;
    private const int PositionId = 9;

    private static string BasePath => $"/2.0/{DocumentType}/{DocumentId}/kb_position_pagebreak";

    private const string PagebreakResponse = """
        {
            "id": 9,
            "type": "KbPositionPagebreak",
            "parent_id": null,
            "internal_pos": 3,
            "is_optional": false,
            "pagebreak": true
        }
        """;

    /// <summary>
    /// <c>PagebreakPositionService.Get()</c> must issue a <c>GET</c> request against the expected
    /// path and return a successful <c>ApiResult</c> when the server returns an empty array.
    /// </summary>
    [Test]
    public async Task PagebreakPositionService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new PagebreakPositionService(ConnectionHandler);

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
    /// <c>PagebreakPositionService.GetById()</c> must issue a <c>GET</c> request that includes the
    /// position id in the URL path and surface the returned position on success.
    /// </summary>
    [Test]
    public async Task PagebreakPositionService_GetById_SendsGetRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PagebreakResponse));

        var service = new PagebreakPositionService(ConnectionHandler);

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
    /// <c>PagebreakPositionService.Create()</c> must send a <c>POST</c> request whose body
    /// contains the serialized <see cref="PositionPagebreakCreate"/> payload and surface the
    /// created position on success.
    /// </summary>
    [Test]
    public async Task PagebreakPositionService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(PagebreakResponse));

        var service = new PagebreakPositionService(ConnectionHandler);

        var payload = new PositionPagebreakCreate(Pagebreak: true);

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.Body, Does.Contain("\"pagebreak\":true"));
        });
    }

    /// <summary>
    /// <c>PagebreakPositionService.Update()</c> must send a <c>POST</c> (not <c>PUT</c>) request
    /// against the path including the position id and surface the updated position on success.
    /// </summary>
    [Test]
    public async Task PagebreakPositionService_Update_SendsPostRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(PagebreakResponse));

        var service = new PagebreakPositionService(ConnectionHandler);

        var payload = new PositionPagebreakUpdate(Pagebreak: true);

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"pagebreak\":true"));
        });
    }

    /// <summary>
    /// <c>PagebreakPositionService.Delete()</c> must issue a <c>DELETE</c> request that includes
    /// the position id in the URL path.
    /// </summary>
    [Test]
    public async Task PagebreakPositionService_Delete_SendsDeleteRequest_WithPositionIdInPath()
    {
        var expectedPath = $"{BasePath}/{PositionId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new PagebreakPositionService(ConnectionHandler);

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
