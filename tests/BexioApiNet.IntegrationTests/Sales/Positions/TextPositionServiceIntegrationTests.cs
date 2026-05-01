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
/// Offline integration tests for <see cref="TextPositionService"/> against a
/// <see cref="WireMockServer"/> stub. Verifies that the correct HTTP verbs and URL paths are
/// used for all five CRUD operations, that request bodies are serialized with the expected
/// snake_case field names, and that the full <c>PositionTextExtended</c> response payload
/// deserializes per the OpenAPI schema.
/// </summary>
public sealed class TextPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = KbDocumentType.Offer;
    private const int DocumentId = 2;
    private const int PositionId = 20;

    private static string BasePath => $"/2.0/{DocumentType}/{DocumentId}/kb_position_text";
    private static string SinglePath => $"{BasePath}/{PositionId}";

    /// <summary>
    /// Fully-populated <c>PositionTextExtended</c> response payload modelled on the OpenAPI
    /// schema. Includes the read-only <c>pos</c>, <c>internal_pos</c>, <c>is_optional</c> and
    /// <c>parent_id</c> fields and the <c>type</c> discriminator.
    /// </summary>
    private const string TextResponse = """
        {
            "id": 20,
            "type": "KbPositionText",
            "parent_id": null,
            "text": "Payment terms: 30 days net.",
            "show_pos_nr": false,
            "pos": "1",
            "internal_pos": 1,
            "is_optional": false
        }
        """;

    /// <summary>
    /// <c>TextPositionService.GetAll()</c> must issue a <c>GET</c> request against the list
    /// path and deserialize a list with full field coverage per the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task TextPositionService_GetAll_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{TextResponse}]"));

        var service = new TextPositionService(ConnectionHandler);

        var result = await service.GetAll(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
        });

        var position = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(position.Id, Is.EqualTo(20));
            Assert.That(position.Type, Is.EqualTo("KbPositionText"));
            Assert.That(position.ParentId, Is.Null);
            Assert.That(position.Text, Is.EqualTo("Payment terms: 30 days net."));
            Assert.That(position.ShowPosNr, Is.False);
            Assert.That(position.Pos, Is.EqualTo("1"));
            Assert.That(position.InternalPos, Is.EqualTo(1));
            Assert.That(position.IsOptional, Is.False);
        });
    }

    /// <summary>
    /// <c>TextPositionService.GetById()</c> must issue a <c>GET</c> request that includes the
    /// position id in the URL path and deserialize the full text-position payload.
    /// </summary>
    [Test]
    public async Task TextPositionService_GetById_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TextResponse));

        var service = new TextPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(result.Data.Type, Is.EqualTo("KbPositionText"));
            Assert.That(result.Data.Text, Is.EqualTo("Payment terms: 30 days net."));
            Assert.That(result.Data.ShowPosNr, Is.False);
            Assert.That(result.Data.Pos, Is.EqualTo("1"));
            Assert.That(result.Data.InternalPos, Is.EqualTo(1));
            Assert.That(result.Data.IsOptional, Is.False);
        });
    }

    /// <summary>
    /// <c>TextPositionService.Create()</c> must send a <c>POST</c> request whose body contains
    /// the serialized <see cref="PositionTextCreate"/> payload as
    /// <c>{"text":..., "show_pos_nr":...}</c> per the OpenAPI request schema.
    /// </summary>
    [Test]
    public async Task TextPositionService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(BasePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(TextResponse));

        var service = new TextPositionService(ConnectionHandler);
        var payload = new PositionTextCreate(Text: "Payment terms: 30 days net.", ShowPosNr: false);

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BasePath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Payment terms: 30 days net.\""));
            Assert.That(request.Body, Does.Contain("\"show_pos_nr\":false"));
        });
    }

    /// <summary>
    /// <c>TextPositionService.Update()</c> must send a <c>POST</c> (not <c>PUT</c>) request
    /// against the path including the position id with a <see cref="PositionTextCreate"/>
    /// body and surface the updated position on success.
    /// </summary>
    [Test]
    public async Task TextPositionService_Update_SendsPostRequest_WithPositionIdInPath()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(TextResponse));

        var service = new TextPositionService(ConnectionHandler);
        var payload = new PositionTextCreate(Text: "Updated payment terms.", ShowPosNr: true);

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Updated payment terms.\""));
            Assert.That(request.Body, Does.Contain("\"show_pos_nr\":true"));
        });
    }

    /// <summary>
    /// <c>TextPositionService.Delete()</c> must issue a <c>DELETE</c> request to the
    /// single-position path and surface the <c>{"success":true}</c> payload.
    /// </summary>
    [Test]
    public async Task TextPositionService_Delete_SendsDeleteRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new TextPositionService(ConnectionHandler);

        var result = await service.Delete(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }

    /// <summary>
    /// Smoke test verifying the path is correctly composed for each of the three OpenAPI-allowed
    /// document types (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>). Text positions are
    /// not valid on deliveries per the spec.
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task TextPositionService_GetAll_UsesCorrectPathForEachDocumentType(string documentType)
    {
        var path = $"/2.0/{documentType}/{DocumentId}/kb_position_text";

        Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new TextPositionService(ConnectionHandler);

        var result = await service.GetAll(documentType, DocumentId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
        });
    }
}
