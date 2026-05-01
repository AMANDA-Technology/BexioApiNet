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
/// Integration tests for <see cref="DefaultPositionService" /> against a WireMock stub.
/// Verifies that the real <see cref="BexioConnectionHandler" /> composes the correct
/// <c>kb_position_custom</c> path segment, uses the expected HTTP verbs, forwards payloads
/// correctly, and deserializes the full <c>PositionCustomExtended</c> OpenAPI schema —
/// all without hitting the live Bexio API.
/// </summary>
public sealed class DefaultPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = KbDocumentType.Order;
    private const int DocumentId = 2;
    private const int PositionId = 8;
    private static readonly string CollectionPath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_custom";
    private static readonly string SinglePath = $"/2.0/{DocumentType}/{DocumentId}/kb_position_custom/{PositionId}";

    /// <summary>
    /// Fully-populated <c>PositionCustomExtended</c> response payload modelled on the OpenAPI
    /// schema example values. Used to exercise field-level deserialization assertions.
    /// </summary>
    private const string CustomPositionResponse = """
        {
            "id": 8,
            "type": "KbPositionCustom",
            "parent_id": null,
            "amount": "5.000000",
            "amount_reserved": "5.000000",
            "amount_open": "5.000000",
            "amount_completed": "5.000000",
            "unit_id": 1,
            "account_id": 100,
            "unit_name": "kg",
            "tax_id": 4,
            "tax_value": "7.70",
            "text": "Apples",
            "unit_price": "3.560000",
            "discount_in_percent": "0.000000",
            "position_total": "17.800000",
            "pos": "1",
            "internal_pos": 1,
            "is_optional": false
        }
        """;

    /// <summary>
    /// <c>DefaultPositionService.Get</c> must issue a <c>GET</c> to the collection path
    /// <c>/2.0/{kb_document_type}/{id}/kb_position_custom</c> and deserialize a list of fully
    /// populated positions, asserting every field round-trips per the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{CustomPositionResponse}]"));

        var service = new DefaultPositionService(ConnectionHandler);

        var result = await service.Get(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CollectionPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
        });

        var position = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(position.Id, Is.EqualTo(8));
            Assert.That(position.Type, Is.EqualTo("KbPositionCustom"));
            Assert.That(position.ParentId, Is.Null);
            Assert.That(position.Amount, Is.EqualTo("5.000000"));
            Assert.That(position.AmountReserved, Is.EqualTo("5.000000"));
            Assert.That(position.AmountOpen, Is.EqualTo("5.000000"));
            Assert.That(position.AmountCompleted, Is.EqualTo("5.000000"));
            Assert.That(position.UnitId, Is.EqualTo(1));
            Assert.That(position.AccountId, Is.EqualTo(100));
            Assert.That(position.UnitName, Is.EqualTo("kg"));
            Assert.That(position.TaxId, Is.EqualTo(4));
            Assert.That(position.TaxValue, Is.EqualTo("7.70"));
            Assert.That(position.Text, Is.EqualTo("Apples"));
            Assert.That(position.UnitPrice, Is.EqualTo("3.560000"));
            Assert.That(position.DiscountInPercent, Is.EqualTo("0.000000"));
            Assert.That(position.PositionTotal, Is.EqualTo("17.800000"));
            Assert.That(position.Pos, Is.EqualTo("1"));
            Assert.That(position.InternalPos, Is.EqualTo(1));
            Assert.That(position.IsOptional, Is.False);
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.GetById</c> must issue a <c>GET</c> to the single-position path
    /// and deserialize the response into a fully populated <see cref="PositionCustomCreate" />-
    /// derived <c>PositionCustom</c>.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_GetById_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CustomPositionResponse));

        var service = new DefaultPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });

        var data = result.Data!;
        Assert.Multiple(() =>
        {
            Assert.That(data.Id, Is.EqualTo(PositionId));
            Assert.That(data.Type, Is.EqualTo("KbPositionCustom"));
            Assert.That(data.Amount, Is.EqualTo("5.000000"));
            Assert.That(data.UnitId, Is.EqualTo(1));
            Assert.That(data.AccountId, Is.EqualTo(100));
            Assert.That(data.TaxId, Is.EqualTo(4));
            Assert.That(data.Text, Is.EqualTo("Apples"));
            Assert.That(data.UnitPrice, Is.EqualTo("3.560000"));
            Assert.That(data.DiscountInPercent, Is.EqualTo("0.000000"));
            Assert.That(data.IsOptional, Is.False);
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Get</c> tolerates the <c>unit_price: null</c> override that
    /// appears in the GET response schema (<c>PositionCustomResponse</c>) but not in the create
    /// or update schemas, per the OpenAPI spec.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_GetById_ToleratesNullUnitPrice()
    {
        const string responseWithNullUnitPrice = """
            {
                "id": 8,
                "type": "KbPositionCustom",
                "parent_id": null,
                "amount": "5.000000",
                "amount_reserved": "5.000000",
                "amount_open": "5.000000",
                "amount_completed": "5.000000",
                "unit_id": 1,
                "account_id": 100,
                "unit_name": "kg",
                "tax_id": 4,
                "tax_value": "7.70",
                "text": "Apples",
                "unit_price": null,
                "discount_in_percent": null,
                "position_total": "0.000000",
                "pos": "1",
                "internal_pos": 1,
                "is_optional": false
            }
            """;

        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseWithNullUnitPrice));

        var service = new DefaultPositionService(ConnectionHandler);

        var result = await service.GetById(DocumentType, DocumentId, PositionId, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.UnitPrice, Is.Null);
            Assert.That(result.Data.DiscountInPercent, Is.Null);
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Create</c> must issue a <c>POST</c> to the collection path
    /// with the serialized <see cref="PositionCustomCreate" /> body, using snake_case JSON field
    /// names. Verifies the create response is also deserialized correctly.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Create_SendsPostRequest_WithSnakeCaseBody()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(CustomPositionResponse));

        var service = new DefaultPositionService(ConnectionHandler);
        var payload = new PositionCustomCreate(
            Amount: "5.000000",
            UnitId: 1,
            AccountId: 100,
            TaxId: 4,
            Text: "Apples",
            UnitPrice: "3.560000",
            DiscountInPercent: "0.000000",
            IsOptional: false);

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CollectionPath));
            Assert.That(request.Body, Does.Contain("\"amount\":\"5.000000\""));
            Assert.That(request.Body, Does.Contain("\"unit_id\":1"));
            Assert.That(request.Body, Does.Contain("\"account_id\":100"));
            Assert.That(request.Body, Does.Contain("\"tax_id\":4"));
            Assert.That(request.Body, Does.Contain("\"text\":\"Apples\""));
            Assert.That(request.Body, Does.Contain("\"unit_price\":\"3.560000\""));
            Assert.That(request.Body, Does.Contain("\"discount_in_percent\":\"0.000000\""));
            Assert.That(request.Body, Does.Contain("\"is_optional\":false"));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Update</c> must issue a <c>POST</c> to the single-position path
    /// (Bexio uses POST for updates) and return the deserialized updated position.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Update_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CustomPositionResponse));

        var service = new DefaultPositionService(ConnectionHandler);
        var payload = new PositionCustomCreate(
            Amount: "10.000000",
            Text: "Updated apples",
            UnitPrice: "4.000000");

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
            Assert.That(request.Body, Does.Contain("\"amount\":\"10.000000\""));
            Assert.That(request.Body, Does.Contain("\"text\":\"Updated apples\""));
            Assert.That(request.Body, Does.Contain("\"unit_price\":\"4.000000\""));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Delete</c> must issue a <c>DELETE</c> to the single-position path
    /// and surface the <c>{"success":true}</c> payload.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Delete_SendsDeleteRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""{"success":true}"""));

        var service = new DefaultPositionService(ConnectionHandler);

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
    public async Task DefaultPositionService_Get_UsesCorrectPathForEachDocumentType(string documentType)
    {
        var path = $"/2.0/{documentType}/{DocumentId}/kb_position_custom";

        Server
            .Given(Request.Create().WithPath(path).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DefaultPositionService(ConnectionHandler);

        var result = await service.Get(documentType, DocumentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.AbsolutePath, Is.EqualTo(path));
        });
    }
}
