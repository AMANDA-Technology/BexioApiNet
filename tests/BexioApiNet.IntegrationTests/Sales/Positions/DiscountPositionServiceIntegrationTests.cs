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
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.IntegrationTests.Sales.Positions;

/// <summary>
/// Integration tests for <see cref="DiscountPositionService"/> against WireMock stubs.
/// Verifies the correct HTTP method and URL path for each operation, the snake_case JSON
/// serialization of <see cref="PositionDiscountCreate"/> payloads, and the field-level
/// deserialization of the full <c>PositionDiscountExtended</c> response schema.
/// </summary>
public sealed class DiscountPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = KbDocumentType.Invoice;
    private const int DocumentId = 1;
    private const int PositionId = 10;
    private static readonly string ListPath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_discount";
    private static readonly string SinglePath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_discount/{PositionId}";

    /// <summary>
    /// Fully-populated <c>PositionDiscountExtended</c> payload modelled on the OpenAPI schema
    /// example values. Discount positions notably do NOT carry <c>parent_id</c>.
    /// </summary>
    private const string DiscountPositionResponse = """
        {
            "id": 10,
            "type": "KbPositionDiscount",
            "text": "Partner discount",
            "is_percentual": true,
            "value": "10.000000",
            "discount_total": "1.780000"
        }
        """;

    /// <summary>
    /// GetAll must issue a <c>GET</c> request to the list path and deserialize a full discount
    /// position response payload, asserting every field matches the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_GetAll_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(ListPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{DiscountPositionResponse}]"));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.GetAll(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ListPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
        });

        var position = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(position.Id, Is.EqualTo(10));
            Assert.That(position.Type, Is.EqualTo("KbPositionDiscount"));
            Assert.That(position.Text, Is.EqualTo("Partner discount"));
            Assert.That(position.IsPercentual, Is.True);
            Assert.That(position.Value, Is.EqualTo("10.000000"));
            Assert.That(position.DiscountTotal, Is.EqualTo("1.780000"));
        });
    }

    /// <summary>
    /// GetById must issue a <c>GET</c> request that includes the position id in the path and
    /// deserialize the full discount position payload.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_GetById_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.InstanceOf<PositionDiscount>());
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });

        var data = result.Data!;
        Assert.Multiple(() =>
        {
            Assert.That(data.Id, Is.EqualTo(PositionId));
            Assert.That(data.Type, Is.EqualTo("KbPositionDiscount"));
            Assert.That(data.Text, Is.EqualTo("Partner discount"));
            Assert.That(data.IsPercentual, Is.True);
            Assert.That(data.Value, Is.EqualTo("10.000000"));
            Assert.That(data.DiscountTotal, Is.EqualTo("1.780000"));
        });
    }

    /// <summary>
    /// Create must issue a <c>POST</c> request to the list path with the serialized
    /// <see cref="PositionDiscountCreate"/> body and deserialize the response.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(ListPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);
        var payload = new PositionDiscountCreate(Text: "Partner discount", IsPercentual: true, Value: "10.000000");

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(ListPath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Partner discount\""));
            Assert.That(request.Body, Does.Contain("\"is_percentual\":true"));
            Assert.That(request.Body, Does.Contain("\"value\":\"10.000000\""));
            Assert.That(request.Body, Does.Not.Contain("\"parent_id\""));
            Assert.That(request.Body, Does.Not.Contain("\"type\""));
            Assert.That(request.Body, Does.Not.Contain("\"discount_total\""));
        });
    }

    /// <summary>
    /// Update must issue a <c>POST</c> request to the single-position path — Bexio uses POST
    /// for position updates rather than PUT — and the response is deserialized.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Update_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DiscountPositionResponse));

        var service = new DiscountPositionService(ConnectionHandler);
        var payload = new PositionDiscountCreate(Text: "Updated discount", IsPercentual: false, Value: "50.000000");

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
            Assert.That(request.Body, Does.Contain("\"text\":\"Updated discount\""));
            Assert.That(request.Body, Does.Contain("\"is_percentual\":false"));
            Assert.That(request.Body, Does.Contain("\"value\":\"50.000000\""));
        });
    }

    /// <summary>
    /// Delete must issue a <c>DELETE</c> request to the single-position path and surface the
    /// <c>{"success":true}</c> payload.
    /// </summary>
    [Test]
    public async Task DiscountPositionService_Delete_SendsDeleteRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("{\"success\":true}"));

        var service = new DiscountPositionService(ConnectionHandler);

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
    /// document types (<c>kb_offer</c>, <c>kb_order</c>, <c>kb_invoice</c>).
    /// </summary>
    [TestCase(KbDocumentType.Offer)]
    [TestCase(KbDocumentType.Order)]
    [TestCase(KbDocumentType.Invoice)]
    public async Task DiscountPositionService_GetAll_UsesCorrectPathForEachDocumentType(string documentType)
    {
        var path = $"/2.0/{documentType}/{DocumentId}/kb_position_discount";

        Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DiscountPositionService(ConnectionHandler);

        var result = await service.GetAll(documentType, DocumentId, TestContext.CurrentContext.CancellationToken);
        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
        });
    }
}
