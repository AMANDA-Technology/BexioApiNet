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

using BexioApiNet.Abstractions.Models.Sales.Positions.Views;
using BexioApiNet.Services.Connectors.Sales.Positions;

namespace BexioApiNet.IntegrationTests.Sales;

/// <summary>
/// Integration smoke tests for <see cref="DefaultPositionService" /> against a WireMock stub.
/// Verifies that the real <see cref="BexioConnectionHandler" /> composes the correct
/// <c>kb_position_custom</c> path segment, uses the expected HTTP verbs, and forwards
/// payloads correctly — all without hitting the live Bexio API.
/// </summary>
public sealed class DefaultPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = "kb_order";
    private const int DocumentId = 2;
    private const int PositionId = 8;
    private const string CollectionPath = "/2.0/kb_order/2/kb_position_custom";
    private const string SinglePath = "/2.0/kb_order/2/kb_position_custom/8";

    private const string CustomPositionResponse = """
        {
            "id": 8,
            "type": "KbPositionCustom",
            "amount": "1.000000",
            "unit_id": 1,
            "account_id": 100,
            "tax_id": 3,
            "text": "Custom consulting fee",
            "unit_price": "150.00",
            "discount_in_percent": null,
            "position_total": "150.00",
            "pos": "1",
            "internal_pos": 1,
            "is_optional": false
        }
        """;

    /// <summary>
    /// <c>DefaultPositionService.Get</c> must issue a <c>GET</c> to the collection path
    /// <c>/2.0/kb_order/{id}/kb_position_custom</c> and surface a successful result.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DefaultPositionService(ConnectionHandler);

        var result = await service.Get(DocumentType, DocumentId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CollectionPath));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.GetById</c> must issue a <c>GET</c> to the single-position path
    /// and surface the deserialized position on success.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_GetById_SendsGetRequest()
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
            Assert.That(result.Data!.Id, Is.EqualTo(PositionId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Create</c> must issue a <c>POST</c> to the collection path
    /// with the serialized <see cref="PositionCustomCreate" /> body and return the created position.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CustomPositionResponse));

        var service = new DefaultPositionService(ConnectionHandler);
        var payload = new PositionCustomCreate(Amount: "1.000000", Text: "Custom consulting fee", UnitPrice: "150.00");

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CollectionPath));
            Assert.That(request.Body, Does.Contain("text"));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Update</c> must issue a <c>POST</c> to the single-position path
    /// (Bexio uses POST for updates) and return the updated position.
    /// </summary>
    [Test]
    public async Task DefaultPositionService_Update_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(CustomPositionResponse));

        var service = new DefaultPositionService(ConnectionHandler);
        var payload = new PositionCustomCreate(Amount: "2.000000", Text: "Updated consulting fee", UnitPrice: "175.00");

        var result = await service.Update(DocumentType, DocumentId, PositionId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(SinglePath));
        });
    }

    /// <summary>
    /// <c>DefaultPositionService.Delete</c> must issue a <c>DELETE</c> to the single-position path.
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
}
