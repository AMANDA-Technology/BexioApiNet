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
/// Integration smoke tests for <see cref="ItemPositionService" /> against a WireMock stub.
/// Verifies that the real <see cref="BexioConnectionHandler" /> composes the correct
/// <c>kb_position_article</c> path segment, uses the expected HTTP verbs, and forwards
/// payloads correctly — all without hitting the live Bexio API.
/// </summary>
public sealed class ItemPositionServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentType = "kb_invoice";
    private const int DocumentId = 1;
    private const int PositionId = 5;
    private const string CollectionPath = "/2.0/kb_invoice/1/kb_position_article";
    private const string SinglePath = "/2.0/kb_invoice/1/kb_position_article/5";

    private const string ArticlePositionResponse = """
        {
            "id": 5,
            "type": "KbPositionArticle",
            "amount": "1.000000",
            "unit_id": 1,
            "account_id": 100,
            "tax_id": 3,
            "text": "Widget Pro",
            "unit_price": "49.90",
            "discount_in_percent": null,
            "position_total": "49.90",
            "pos": "1",
            "internal_pos": 1,
            "is_optional": false,
            "article_id": 10
        }
        """;

    /// <summary>
    /// <c>ItemPositionService.Get</c> must issue a <c>GET</c> to the collection path
    /// <c>/2.0/kb_invoice/{id}/kb_position_article</c> and surface a successful result.
    /// </summary>
    [Test]
    public async Task ItemPositionService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new ItemPositionService(ConnectionHandler);

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
    /// <c>ItemPositionService.GetById</c> must issue a <c>GET</c> to the single-position path
    /// and surface the deserialized position on success.
    /// </summary>
    [Test]
    public async Task ItemPositionService_GetById_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ArticlePositionResponse));

        var service = new ItemPositionService(ConnectionHandler);

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
    /// <c>ItemPositionService.Create</c> must issue a <c>POST</c> to the collection path
    /// with the serialized <see cref="PositionArticleCreate" /> body and return the created position.
    /// </summary>
    [Test]
    public async Task ItemPositionService_Create_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(CollectionPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ArticlePositionResponse));

        var service = new ItemPositionService(ConnectionHandler);
        var payload = new PositionArticleCreate(Amount: "1.000000", ArticleId: 10, UnitPrice: "49.90");

        var result = await service.Create(DocumentType, DocumentId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(CollectionPath));
            Assert.That(request.Body, Does.Contain("article_id"));
        });
    }

    /// <summary>
    /// <c>ItemPositionService.Update</c> must issue a <c>POST</c> to the single-position path
    /// (Bexio uses POST for updates) and return the updated position.
    /// </summary>
    [Test]
    public async Task ItemPositionService_Update_SendsPostRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(ArticlePositionResponse));

        var service = new ItemPositionService(ConnectionHandler);
        var payload = new PositionArticleCreate(Amount: "2.000000", ArticleId: 10, UnitPrice: "49.90");

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
    /// <c>ItemPositionService.Delete</c> must issue a <c>DELETE</c> to the single-position path.
    /// </summary>
    [Test]
    public async Task ItemPositionService_Delete_SendsDeleteRequest()
    {
        Server
            .Given(Request.Create().WithPath(SinglePath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""{"success":true}"""));

        var service = new ItemPositionService(ConnectionHandler);

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
